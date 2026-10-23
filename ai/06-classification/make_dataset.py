import numpy as np
import pandas as pd

RNG = np.random.default_rng(7)
ROWS = 50
CLASS_ONE = 31


def build():
    study = RNG.uniform(0.5, 9.5, ROWS).round(1)
    attendance = RNG.integers(40, 101, ROWS)
    homework = RNG.integers(30, 101, ROWS)
    previous = RNG.integers(35, 99, ROWS)
    sleep = RNG.uniform(4.0, 9.0, ROWS).round(1)

    z = (
        0.52 * study
        + 0.045 * attendance
        + 0.035 * homework
        + 0.040 * previous
        + 0.30 * sleep
        + RNG.normal(0, 0.9, ROWS)
    )

    passed = np.zeros(ROWS, dtype=int)
    passed[np.argsort(z)[-CLASS_ONE:]] = 1

    return pd.DataFrame({
        "study_hours": study,
        "attendance_percent": attendance,
        "homework_score": homework,
        "previous_score": previous,
        "sleep_hours": sleep,
        "passed": passed,
    })


if __name__ == "__main__":
    data = build()
    data.to_excel("student_exam_classification.xlsx", index=False)
    print(f"строк: {len(data)}, колонок: {len(data.columns)}")
    print(data["passed"].value_counts().to_string())
    print(data.head().to_string(index=False))
