"""
Day 2 POC Re-Validation Script
Runs the existing PPE detector on ALL current sample_data images and saves JSON results.
Output format matches the existing result_*.json convention (summary + details).
"""
import os
import sys
import json
import glob
import time

# Add project root to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..', '..'))

from ml.vision.inference.detector import PPEDetector

SAMPLE_DIR = os.path.join(os.path.dirname(__file__), '..', 'sample_data')
MODEL_PATH = os.path.join(os.path.dirname(__file__), '..', 'models', 'best.onnx')
RESULTS_DIR = os.path.dirname(__file__)

# Supported image extensions
EXTS = ('*.jpg', '*.jpeg', '*.png', '*.bmp')


def main():
    import cv2
    import numpy as np

    # Load model once
    detector = PPEDetector(MODEL_PATH)

    # Collect all image paths
    image_paths = []
    for ext in EXTS:
        image_paths.extend(glob.glob(os.path.join(SAMPLE_DIR, ext)))
    image_paths.sort()

    if not image_paths:
        print("No images found in sample_data/")
        return

    print(f"Found {len(image_paths)} images to test\n")
    print("=" * 80)

    results_summary = []

    for img_path in image_paths:
        fname = os.path.basename(img_path)
        print(f"\n>>> Testing: {fname}")

        img = cv2.imread(img_path)
        if img is None:
            print(f"  ERROR: Could not read image {fname}")
            continue

        # Run detection with default required PPE
        result = detector.verify_ppe(img, required=["Hardhat", "Safety Vest"])

        # Build summary wrapper (matching detector.py __main__ output format)
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

        # Save individual JSON result
        result_fname = f"result_{os.path.splitext(fname)[0]}.json"
        result_path = os.path.join(RESULTS_DIR, result_fname)
        with open(result_path, 'w') as f:
            json.dump(summary, f, indent=2, ensure_ascii=False)
        print(f"  Saved: {result_path}")

        # Print summary
        detections = result['detections']
        latency = result['latency_ms']['total']

        det_str = ", ".join([f"{d['label']} ({d['confidence']:.3f})" for d in detections]) if detections else "none"
        print(f"  Detections: {det_str}")
        print(f"  Worn: {ppe['worn']}")
        print(f"  Missing: {ppe['missing']}")
        print(f"  Status: {ppe['status']}")
        print(f"  Latency: {latency:.2f} ms")

        results_summary.append({
            "image": fname,
            "detections": det_str,
            "worn": ppe['worn'],
            "missing": ppe['missing'],
            "status": ppe['status'],
            "latency_ms": latency,
        })

    # Print final summary table
    print("\n" + "=" * 80)
    print("VALIDATION SUMMARY")
    print("=" * 80)
    print(f"{'Image':<35} {'Status':<8} {'Latency':<12} {'Detections'}")
    print("-" * 80)
    for r in results_summary:
        print(f"{r['image']:<35} {r['status']:<8} {r['latency_ms']:<12.2f} {r['detections']}")

    latencies = [r['latency_ms'] for r in results_summary]
    print(f"\nLatency range: {min(latencies):.2f} - {max(latencies):.2f} ms")
    print(f"Average latency: {sum(latencies)/len(latencies):.2f} ms")


if __name__ == "__main__":
    main()
