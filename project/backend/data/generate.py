import numpy as np
import pandas as pd
from pathlib import Path

HERE = Path(__file__).resolve().parent
RNG = np.random.default_rng(42)
DAYS = 900


def build():
    sleep = np.clip(RNG.normal(7.0, 1.3, DAYS), 3.0, 11.0).round(1)
    water = np.clip(RNG.normal(1900, 650, DAYS), 300, 4200).round(0)
    steps = np.clip(RNG.normal(7200, 3200, DAYS), 400, 22000).round(0)
    calories = np.clip(RNG.normal(2150, 520, DAYS), 900, 4200).round(0)
    protein = np.clip(calories * RNG.uniform(0.04, 0.09, DAYS), 25, 260).round(0)
    caffeine = np.clip(RNG.gamma(2.0, 70, DAYS), 0, 600).round(0)
    screen = np.clip(RNG.normal(5.5, 2.2, DAYS), 0.5, 14).round(1)
    stress = np.clip(RNG.normal(5.0, 2.0, DAYS), 1, 10).round(0)

    sleep_term = -0.42 * (sleep - 7.6) ** 2 + 1.1
    water_term = 0.85 * np.tanh((water - 1500) / 700)
    steps_term = 0.80 * np.tanh((steps - 5000) / 4500)
    protein_term = 0.45 * np.tanh((protein - 70) / 55)
    caffeine_term = -0.0022 * np.clip(caffeine - 200, 0, None)
    screen_term = -0.11 * np.clip(screen - 6, 0, None)
    stress_term = -0.26 * (stress - 5)

    score = (
        5.9 + sleep_term + water_term + steps_term + protein_term
        + caffeine_term + screen_term + stress_term
        + RNG.normal(0, 0.45, DAYS)
    )

    energy = np.clip(score, 1, 10).round(2)

    df = pd.DataFrame({
        "sleep_hours": sleep,
        "water_ml": water,
        "steps": steps,
        "calories": calories,
        "protein_g": protein,
        "caffeine_mg": caffeine,
        "screen_hours": screen,
        "stress_level": stress,
        "energy_score": energy,
    })

    edges = df["energy_score"].quantile([0.33, 0.66]).tolist()
    df["wellbeing_class"] = pd.cut(
        df["energy_score"], bins=[-np.inf, edges[0], edges[1], np.inf],
        labels=["low", "medium", "high"]).astype(str)

    return df


if __name__ == "__main__":
    data = build()
    data.to_csv(HERE / "daily_logs.csv", index=False)

    print(f"строк: {len(data)}, колонок: {len(data.columns)}")
    print(f"пропусков: {int(data.isnull().sum().sum())}")
    print("\nраспределение классов:")
    print(data["wellbeing_class"].value_counts().to_string())
    print("\nэнергия:")
    print(data["energy_score"].describe().round(2).to_string())
