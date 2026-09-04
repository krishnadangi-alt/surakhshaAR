"""
SurakshaAR — PPE Detection POC (Day 2)
=======================================
Feature: Camera-based PPE verification (helmet, vest, gloves, mask, goggles)
Model:   YOLOv8m fine-tuned on PPE Combined Model v4 (14 classes)
         Source: Hexmon/vyra-yolo-ppe-detection (HuggingFace, ONNX export)
Runtime: onnxruntime + OpenCV (no PyTorch needed)

Input:  image file path (or numpy BGR array)
Output: structured JSON with detections, pass/fail verdict, latency

Unity contract (input/output) documented in README.md — this file does NOT
modify Unity; it only produces the JSON that Unity will later consume.
"""

import os
import json
import time
import cv2
import numpy as np
import onnxruntime as ort

# ---- Model metadata (from model card) -------------------------------------
# https://huggingface.co/Hexmon/vyra-yolo-ppe-detection
CLASS_NAMES = {
    0: "Fall-Detected", 1: "Gloves", 2: "Goggles", 3: "Hardhat",
    4: "Ladder", 5: "Mask", 6: "NO-Gloves", 7: "NO-Goggles",
    8: "NO-Hardhat", 9: "NO-Mask", 10: "NO-Safety Vest",
    11: "Person", 12: "Safety Cone", 13: "Safety Vest",
}

# PPE items relevant to SurakshaAR's "wear your gear" verification step.
PPE_CLASSES = {
    "Hardhat": 3, "Safety Vest": 13, "Gloves": 1, "Mask": 5, "Goggles": 2,
}
NO_CLASSES = {6, 7, 8, 9, 10}

MODEL_INPUT_SIZE = 640


class PPEDetector:
    """Thin ONNX Runtime wrapper around the YOLOv8m PPE model."""

    def __init__(self, model_path: str, conf_thresh: float = 0.45,
                 iou_thresh: float = 0.5, device: str = "cpu"):
        if not os.path.exists(model_path):
            raise FileNotFoundError(f"Model not found: {model_path}")
        self.conf_thresh = conf_thresh
        self.iou_thresh = iou_thresh
        providers = ["CPUExecutionProvider"]
        self.session = ort.InferenceSession(model_path, providers=providers)
        self.input_name = self.session.get_inputs()[0].name
        self.input_shape = self.session.get_inputs()[0].shape
        print(f"[PPEDetector] Loaded {os.path.basename(model_path)} "
              f"| input {self.input_shape} | providers {self.session.get_providers()}")
    # ---- preprocessing -----------------------------------------------------
    @staticmethod
    def _preprocess(img_bgr: np.ndarray) -> tuple:
        """Resize with letterbox, BGR->RGB, HWC->CHW, normalize to [0,1].
        Returns the blob plus a metadata dict for coordinate mapping."""
        h, w = img_bgr.shape[:2]
        scale = min(MODEL_INPUT_SIZE / h, MODEL_INPUT_SIZE / w)
        new_w, new_h = int(w * scale), int(h * scale)
        resized = cv2.resize(img_bgr, (new_w, new_h), interpolation=cv2.INTER_LINEAR)
        # letterbox canvas (gray padding)
        canvas = np.full((MODEL_INPUT_SIZE, MODEL_INPUT_SIZE, 3), 114, dtype=np.uint8)
        pad_x = (MODEL_INPUT_SIZE - new_w) // 2
        pad_y = (MODEL_INPUT_SIZE - new_h) // 2
        canvas[pad_y:pad_y + new_h, pad_x:pad_x + new_w] = resized
        rgb = canvas[:, :, ::-1]  # BGR -> RGB
        blob = rgb.transpose(2, 0, 1).astype(np.float32) / 255.0
        blob = np.expand_dims(blob, 0)  # NCHW
        meta = {"scale": scale, "pad_x": pad_x, "pad_y": pad_y,
                "orig_w": w, "orig_h": h}
        return blob, meta

    # ---- postprocessing (YOLOv8 box format) --------------------------------
    @staticmethod
    def _postprocess(output: np.ndarray, meta: dict) -> list:
        """YOLOv8 output is [1, 18, 8400] -> squeeze batch -> [8400, 18].
        Columns: cx, cy, w, h, then 14 class scores."""
        preds = output[0].squeeze(0).T  # [8400, 18]
        boxes = preds[:, :4]
        scores = preds[:, 4:]
        class_ids = np.argmax(scores, axis=1)
        confs = np.max(scores, axis=1)
        mask = confs >= 0.45
        boxes, class_ids, confs = boxes[mask], class_ids[mask], confs[mask]
        if len(boxes) == 0:
            return []
        # cxcywh -> xyxy
        xyxy = np.zeros_like(boxes)
        xyxy[:, 0] = boxes[:, 0] - boxes[:, 2] / 2
        xyxy[:, 1] = boxes[:, 1] - boxes[:, 3] / 2
        xyxy[:, 2] = boxes[:, 0] + boxes[:, 2] / 2
        xyxy[:, 3] = boxes[:, 1] + boxes[:, 3] / 2
        # scale back to original image coords
        scale, pad_x, pad_y = meta["scale"], meta["pad_x"], meta["pad_y"]
        xyxy[:, [0, 2]] = (xyxy[:, [0, 2]] - pad_x) / scale
        xyxy[:, [1, 3]] = (xyxy[:, [1, 3]] - pad_y) / scale
        xyxy[:, [0, 2]] = np.clip(xyxy[:, [0, 2]], 0, meta["orig_w"])
        xyxy[:, [1, 3]] = np.clip(xyxy[:, [1, 3]], 0, meta["orig_h"])
        # class-aware NMS via OpenCV
        indices = cv2.dnn.NMSBoxes(
            xyxy.tolist(), confs.tolist(), score_threshold=0.45,
            nms_threshold=0.5
        )
        dets = []
        if len(indices):
            for i in indices.flatten():
                dets.append({
                    "label": CLASS_NAMES.get(int(class_ids[i]), f"cls_{class_ids[i]}"),
                    "class_id": int(class_ids[i]),
                    "confidence": round(float(confs[i]), 4),
                    "bbox": [round(float(v), 1) for v in xyxy[i]],
                })
        return dets


    # ---- public API --------------------------------------------------------
    def detect(self, img_bgr: np.ndarray) -> dict:
        """Run full inference on a BGR image and return structured result."""
        t0 = time.perf_counter()
        blob, meta = self._preprocess(img_bgr)
        t_pre = time.perf_counter()
        output = self.session.run(None, {self.input_name: blob})
        t_inf = time.perf_counter()
        detections = self._postprocess(output, meta)
        t_post = time.perf_counter()
        return {
            "detections": detections,
            "latency_ms": {
                "preprocess": round((t_pre - t0) * 1000, 2),
                "inference": round((t_inf - t_pre) * 1000, 2),
                "postprocess": round((t_post - t_inf) * 1000, 2),
                "total": round((t_post - t0) * 1000, 2),
            },
            "input_shape": list(img_bgr.shape[:2]),
        }

    def verify_ppe(self, img_bgr: np.ndarray,
                   required: list = None) -> dict:
        """High-level PPE check: which required items are worn / missing."""
        if required is None:
            required = ["Hardhat", "Safety Vest"]
        result = self.detect(img_bgr)
        dets = result["detections"]
        detected_labels = {d["label"] for d in dets}
        worn, missing = [], []
        for item in required:
            pos_present = item in detected_labels
            no_label = f"NO-{item}"
            no_present = no_label in detected_labels
            if pos_present and not no_present:
                worn.append(item)
            else:
                missing.append(item)
        all_present = len(missing) == 0
        result["ppe_check"] = {
            "required": required,
            "worn": worn,
            "missing": missing,
            "all_required_present": all_present,
            "status": "pass" if all_present else "fail",
        }
        return result
if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description="SurakshaAR PPE Detection POC")
    parser.add_argument("--image", required=True, help="Path to input image")
    parser.add_argument("--model", default="ml/vision/models/best.onnx")
    parser.add_argument("--required", nargs="+", default=["Hardhat", "Safety Vest"],
                        help="Required PPE items to verify")
    parser.add_argument("--output", default=None, help="Optional JSON output path")
    args = parser.parse_args()

    if not os.path.exists(args.image):
        raise SystemExit(f"Image not found: {args.image}")

    img = cv2.imread(args.image)
    detector = PPEDetector(args.model)
    result = detector.verify_ppe(img, required=args.required)

    # Add top-level summary fields for Unity consumption
    ppe = result["ppe_check"]
    summary = {
        "feature": "ppe_verification",
        "detected": ppe["all_required_present"],
        "label": "PPE complete" if ppe["all_required_present"] else "PPE incomplete",
        "confidence": round(
            np.mean([d["confidence"] for d in result["detections"]])
            if result["detections"] else 0.0, 4
        ),
        "latency_ms": result["latency_ms"]["total"],
        "status": ppe["status"],
        "details": result,
    }
    print(json.dumps(summary, indent=2, ensure_ascii=False))
    if args.output:
        with open(args.output, "w") as f:
            json.dump(summary, f, indent=2, ensure_ascii=False)

