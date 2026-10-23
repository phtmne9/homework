import json
import pickle
from pathlib import Path

import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier, RandomForestRegressor
from sklearn.linear_model import LinearRegression, LogisticRegression
from sklearn.metrics import (accuracy_score, classification_report, f1_score,
                             mean_absolute_error, mean_squared_error, r2_score)
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler

HERE = Path(__file__).resolve().parent
DATA = HERE.parent / "data" / "daily_logs.csv"

FEATURES = ["sleep_hours", "water_ml", "steps", "calories",
            "protein_g", "caffeine_mg", "screen_hours", "stress_level"]
SEED = 42


def load():
    df = pd.read_csv(DATA)
    return df[FEATURES], df["energy_score"], df["wellbeing_class"]


def main():
    X, y_reg, y_cls = load()
    print(f"Данные: {X.shape[0]} дней, {X.shape[1]} признаков")

    X_train, X_test, yr_train, yr_test, yc_train, yc_test = train_test_split(
        X, y_reg, y_cls, test_size=0.2, random_state=SEED, stratify=y_cls)

    regressors = {
        "LinearRegression": Pipeline([
            ("scaler", StandardScaler()),
            ("model", LinearRegression()),
        ]),
        "RandomForest": Pipeline([
            ("model", RandomForestRegressor(
                n_estimators=120, max_depth=8, random_state=SEED, n_jobs=-1)),
        ]),
    }

    reg_rows = {}
    for name, pipe in regressors.items():
        pipe.fit(X_train, yr_train)
        pred = pipe.predict(X_test)
        reg_rows[name] = {
            "mae": round(float(mean_absolute_error(yr_test, pred)), 4),
            "rmse": round(float(mean_squared_error(yr_test, pred) ** 0.5), 4),
            "r2": round(float(r2_score(yr_test, pred)), 4),
        }

    best_reg_name = min(reg_rows, key=lambda n: reg_rows[n]["rmse"])
    best_reg = regressors[best_reg_name]

    classifiers = {
        "LogisticRegression": Pipeline([
            ("scaler", StandardScaler()),
            ("model", LogisticRegression(max_iter=1000, random_state=SEED)),
        ]),
        "RandomForest": Pipeline([
            ("model", RandomForestClassifier(
                n_estimators=120, max_depth=8, random_state=SEED, n_jobs=-1)),
        ]),
    }

    cls_rows = {}
    for name, pipe in classifiers.items():
        pipe.fit(X_train, yc_train)
        pred = pipe.predict(X_test)
        cls_rows[name] = {
            "accuracy": round(float(accuracy_score(yc_test, pred)), 4),
            "f1_macro": round(float(f1_score(yc_test, pred, average="macro")), 4),
        }

    best_cls_name = max(cls_rows, key=lambda n: cls_rows[n]["f1_macro"])
    best_cls = classifiers[best_cls_name]

    print("\nРегрессия (предсказание энергии):")
    print(pd.DataFrame(reg_rows).T.to_string())
    print(f"выбрана: {best_reg_name}")

    print("\nКлассификация (уровень самочувствия):")
    print(pd.DataFrame(cls_rows).T.to_string())
    print(f"выбрана: {best_cls_name}")

    print("\nОтчёт по лучшему классификатору:")
    print(classification_report(yc_test, best_cls.predict(X_test), digits=3))

    forest = RandomForestRegressor(
        n_estimators=120, max_depth=8, random_state=SEED, n_jobs=-1).fit(X_train, yr_train)

    importance = (pd.DataFrame({"feature": FEATURES, "importance": forest.feature_importances_})
                  .sort_values("importance", ascending=False)
                  .reset_index(drop=True))
    importance["importance"] = importance["importance"].round(4)

    print("\nВажность признаков:")
    print(importance.to_string(index=False))

    with open(HERE / "regressor.pkl", "wb") as f:
        pickle.dump(best_reg, f)
    with open(HERE / "classifier.pkl", "wb") as f:
        pickle.dump(best_cls, f)

    meta = {
        "features": FEATURES,
        "classes": sorted(y_cls.unique().tolist()),
        "regressor": {"name": best_reg_name, **reg_rows[best_reg_name]},
        "classifier": {"name": best_cls_name, **cls_rows[best_cls_name]},
        "all_regressors": reg_rows,
        "all_classifiers": cls_rows,
        "feature_importance": importance.to_dict("records"),
        "train_size": int(len(X_train)),
        "test_size": int(len(X_test)),
    }

    with open(HERE / "metrics.json", "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)

    print(f"\nСохранено: regressor.pkl, classifier.pkl, metrics.json")


if __name__ == "__main__":
    main()
