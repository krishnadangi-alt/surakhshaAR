"""
SurakshaAR - Fire/Safety Observation Mapper (Day 3)
=====================================================
Converts PPE verification results into structured fire/safety observations.

Use Case: Worker entering or operating in a fire-risk/hazardous area.
The PPE detector checks required PPE. If required PPE is missing,
this module converts the AI result into a safety observation such as:
    "Required PPE incomplete: Hardhat missing."

Input:  PPE verification result (dict) from PPEDetector.verify_ppe()
Output: Fire/safety observation (dict) for Member 1 to consume

This module does NOT modify Unity or backend code. It only transforms
the existing PPE verification output into a safety observation format.
"""

import datetime


# Fire-risk area PPE requirements
FIRE_RISK_REQUIRED_PPE = ["Hardhat", "Safety Vest"]

# Severity levels based on PPE compliance
SEVERITY = {
    "pass": "low",
    "fail": "high",
    "uncertain": "medium",
}


def create_fire_safety_observation(ppe_result: dict,
                                   required_ppe: list = None,
                                   location: str = "fire-risk area") -> dict:
    """
    Convert a PPE verification result into a fire/safety observation.

    Args:
        ppe_result: Output from PPEDetector.verify_ppe()
        required_ppe: List of required PPE items (default: FIRE_RISK_REQUIRED_PPE)
        location: Description of the hazardous location/context

    Returns:
        A structured fire/safety observation dict
    """
    if required_ppe is None:
        required_ppe = FIRE_RISK_REQUIRED_PPE

    ppe_check = ppe_result.get("ppe_check", {})
    detections = ppe_result.get("detections", [])
    latency_ms = ppe_result.get("latency_ms", {}).get("total", 0.0)

    worn = ppe_check.get("worn", [])
    missing = ppe_check.get("missing", [])
    all_present = ppe_check.get("all_required_present", False)

    # Calculate confidence from detections
    import numpy as np
    confidence = round(
        float(np.mean([d["confidence"] for d in detections])) if detections else 0.0, 4
    )

    # Determine status and severity
    if all_present:
        status = "pass"
        severity = SEVERITY["pass"]
        observation = f"Required PPE complete. Worker compliant for {location}."
    elif missing:
        status = "fail"
        severity = SEVERITY["fail"]
        missing_str = ", ".join(missing)
        observation = f"Required PPE incomplete: {missing_str} missing."
    else:
        status = "uncertain"
        severity = SEVERITY["uncertain"]
        observation = f"PPE status uncertain for {location}. Manual verification recommended."

    timestamp = datetime.datetime.now().isoformat(timespec="seconds")

    return {
        "feature": "ppe_verification",
        "observation_type": "fire_safety",
        "timestamp": timestamp,
        "location": location,
        "status": status,
        "severity": severity,
        "confidence": confidence,
        "detected": all_present,
        "worn": worn,
        "missing": missing,
        "observation": observation,
        "latency_ms": latency_ms,
        "details": {
            "required_ppe": required_ppe,
            "raw_detections": detections,
        },
    }


def create_fallback_observation(reason: str = "inference_failed",
                                required_ppe: list = None,
                                location: str = "fire-risk area") -> dict:
    """
    Create a fallback safety observation when AI inference fails or is unavailable.

    Args:
        reason: Why fallback is triggered
        required_ppe: List of required PPE items
        location: Description of the hazardous location/context

    Returns:
        A fallback fire/safety observation dict
    """
    if required_ppe is None:
        required_ppe = FIRE_RISK_REQUIRED_PPE

    timestamp = datetime.datetime.now().isoformat(timespec="seconds")

    return {
        "feature": "ppe_verification",
        "observation_type": "fire_safety",
        "timestamp": timestamp,
        "location": location,
        "status": "uncertain",
        "severity": "medium",
        "confidence": 0.0,
        "detected": False,
        "worn": [],
        "missing": required_ppe,
        "observation": (
            f"AI inference unavailable ({reason}). "
            f"Manual PPE checklist required for {location}. "
            f"Required PPE: {', '.join(required_ppe)}."
        ),
        "latency_ms": 0.0,
        "details": {
            "required_ppe": required_ppe,
            "fallback_reason": reason,
            "raw_detections": [],
        },
    }


if __name__ == "__main__":
    import os
    import sys
    import json
    import cv2

    # Add project root to path
    sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..', '..'))

    from ml.vision.inference.detector import PPEDetector

    MODEL_PATH = os.path.join(os.path.dirname(__file__), '..', 'models', 'best.onnx')
    SAMPLE_DIR = os.path.join(os.path.dirname(__file__), '..', 'sample_data')

    if not os.path.exists(MODEL_PATH):
        print(f"Model not found: {MODEL_PATH}")
        sys.exit(1)

    detector = PPEDetector(MODEL_PATH)

    # Test images representing fire-risk scenarios
    test_images = [
        "worker_vest_hardhat.jpg",
        "worker_hardhat.jpg",
        "worker_no_vest_no_hardhat.jpg",
        "safety_vest_only.jpg",
    ]

    print("=" * 70)
    print("FIRE/SAFETY OBSERVATION DEMO")
    print("=" * 70)

    for img_name in test_images:
        img_path = os.path.join(SAMPLE_DIR, img_name)
        if not os.path.exists(img_path):
            print(f"\n[SKIP] Image not found: {img_name}")
            continue

        img = cv2.imread(img_path)
        if img is None:
            print(f"\n[ERROR] Could not read: {img_name}")
            continue

        # Run PPE verification
        ppe_result = detector.verify_ppe(img, required=FIRE_RISK_REQUIRED_PPE)

        # Convert to fire/safety observation
        observation = create_fire_safety_observation(ppe_result)

        print(f"\n--- {img_name} ---")
        print(json.dumps(observation, indent=2, ensure_ascii=False))

    # Demonstrate fallback observation
    print("\n" + "=" * 70)
    print("FALLBACK OBSERVATION DEMO")
    print("=" * 70)
    fallback = create_fallback_observation(reason="device_unavailable")
    print(json.dumps(fallback, indent=2, ensure_ascii=False))

