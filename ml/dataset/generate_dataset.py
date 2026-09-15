"""
Synthetic Dataset Generator for SurakshaAR ML Competency Prototype.

PROTOTYPE DISCLAIMER:
This script generates a clearly labeled synthetic/demo dataset for hackathon
prototype demonstration. It is NOT real worker telemetry data.
"""

import os
import random
import csv
from typing import List, Dict

DATASET_HEADER = [
    "worker_id",
    "module",
    "scenario",
    "correct_actions",
    "wrong_actions",
    "unsafe_actions",
    "critical_errors",
    "response_time",
    "completion",
    "score",
    "attempts",
    "competency"
]

MODULES = ["fire", "gas", "machinery"]
SCENARIOS = {
    "fire": ["electrical_fire", "chemical_fire", "structural_fire"],
    "gas": ["methane_leak", "h2s_leak", "confined_space"],
    "machinery": ["conveyor_pinch", "milling_nip", "lathe_entrapment"]
}


def generate_synthetic_records(num_records: int = 500, seed: int = 42) -> List[Dict]:
    """Generate synthetic worker assessment telemetry records."""
    random.seed(seed)
    records = []

    for i in range(1, num_records + 1):
        worker_id = f"W{1000 + (i % 80)}"
        module = random.choice(MODULES)
        scenario = random.choice(SCENARIOS[module])
        attempts = random.randint(1, 4)

        # Determine worker profile quality
        quality = random.choices(
            ["competent", "borderline", "struggling", "unsafe"],
            weights=[0.5, 0.2, 0.2, 0.1]
        )[0]

        if quality == "competent":
            correct_actions = random.randint(5, 8)
            wrong_actions = random.randint(0, 1)
            unsafe_actions = 0
            critical_errors = 0
            response_time = round(random.uniform(2.0, 7.0), 2)
            completion = 1
            score = round(random.uniform(75.0, 98.0), 1)
            competency = 1
        elif quality == "borderline":
            correct_actions = random.randint(4, 6)
            wrong_actions = random.randint(1, 3)
            unsafe_actions = random.randint(0, 1)
            critical_errors = 0
            response_time = round(random.uniform(6.0, 14.0), 2)
            completion = 1
            score = round(random.uniform(65.0, 74.0), 1)
            competency = 1 if score >= 70.0 else 0
        elif quality == "struggling":
            correct_actions = random.randint(2, 4)
            wrong_actions = random.randint(3, 5)
            unsafe_actions = random.randint(1, 2)
            critical_errors = 0
            response_time = round(random.uniform(12.0, 20.0), 2)
            completion = random.choice([0, 1])
            score = round(random.uniform(40.0, 64.0), 1)
            competency = 0
        else:  # unsafe
            correct_actions = random.randint(1, 3)
            wrong_actions = random.randint(2, 4)
            unsafe_actions = random.randint(1, 3)
            critical_errors = random.randint(1, 2)
            response_time = round(random.uniform(5.0, 18.0), 2)
            completion = random.choice([0, 1])
            score = round(random.uniform(20.0, 55.0), 1)
            competency = 0  # Critical error forces FAIL

        record = {
            "worker_id": worker_id,
            "module": module,
            "scenario": scenario,
            "correct_actions": correct_actions,
            "wrong_actions": wrong_actions,
            "unsafe_actions": unsafe_actions,
            "critical_errors": critical_errors,
            "response_time": response_time,
            "completion": completion,
            "score": score,
            "attempts": attempts,
            "competency": competency
        }
        records.append(record)

    return records


def save_dataset(output_path: str, num_records: int = 500) -> str:
    """Generate and save the synthetic dataset CSV file."""
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    records = generate_synthetic_records(num_records=num_records)

    with open(output_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=DATASET_HEADER)
        writer.writeheader()
        writer.writerows(records)

    return output_path


if __name__ == "__main__":
    target_csv = os.path.join(os.path.dirname(__file__), "worker_competency_dataset.csv")
    path = save_dataset(target_csv)
    print(f"Generated synthetic dataset: {path}")
