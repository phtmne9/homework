import json
import pickle
import statistics
from pathlib import Path

import pandas as pd
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

HERE = Path(__file__).resolve().parent
MODEL_DIR = HERE / "model"

with open(MODEL_DIR / "metrics.json", encoding="utf-8") as f:
    META = json.load(f)

with open(MODEL_DIR / "regressor.pkl", "rb") as f:
    REGRESSOR = pickle.load(f)

with open(MODEL_DIR / "classifier.pkl", "rb") as f:
    CLASSIFIER = pickle.load(f)

FEATURES = META["features"]

LIMITS = {
    "sleep_hours": (0.0, 16.0, "ч"),
    "water_ml": (0.0, 6000.0, "мл"),
    "steps": (0.0, 40000.0, "шагов"),
    "calories": (0.0, 6000.0, "ккал"),
    "protein_g": (0.0, 400.0, "г"),
    "caffeine_mg": (0.0, 1200.0, "мг"),
    "screen_hours": (0.0, 24.0, "ч"),
    "stress_level": (1.0, 10.0, "балл"),
}

TITLES = {
    "sleep_hours": "Сон",
    "water_ml": "Вода",
    "steps": "Шаги",
    "calories": "Калории",
    "protein_g": "Белок",
    "caffeine_mg": "Кофеин",
    "screen_hours": "Экранное время",
    "stress_level": "Стресс",
}

TARGETS = {
    "sleep_hours": 7.6,
    "water_ml": 2200,
    "steps": 9000,
    "protein_g": 110,
    "caffeine_mg": 150,
    "screen_hours": 5.0,
    "stress_level": 4.0,
}

LOWER_IS_BETTER = {"caffeine_mg", "screen_hours", "stress_level"}

app = FastAPI(
    title="Health Tracker API",
    description="Оценка самочувствия по дневным показателям",
    version="1.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["GET", "POST"],
    allow_headers=["*"],
)


class DayLog(BaseModel):
    sleep_hours: float = Field(..., ge=0, le=16)
    water_ml: float = Field(..., ge=0, le=6000)
    steps: float = Field(..., ge=0, le=40000)
    calories: float = Field(..., ge=0, le=6000)
    protein_g: float = Field(..., ge=0, le=400)
    caffeine_mg: float = Field(..., ge=0, le=1200)
    screen_hours: float = Field(..., ge=0, le=24)
    stress_level: float = Field(..., ge=1, le=10)


class HistoryRequest(BaseModel):
    days: list[DayLog]


def frame(log: DayLog) -> pd.DataFrame:
    return pd.DataFrame([[getattr(log, name) for name in FEATURES]], columns=FEATURES)


def advice(log: DayLog, limit: int = 3):
    scored = []

    for item in META["feature_importance"]:
        name = item["feature"]
        target = TARGETS.get(name)
        if target is None:
            continue

        value = getattr(log, name)
        gap = (value - target) / target

        if name in LOWER_IS_BETTER:
            if gap <= 0.15:
                continue
            text = f"{TITLES[name]} выше ориентира: {value:g} против {target:g}"
        else:
            if gap >= -0.12:
                continue
            text = f"{TITLES[name]} ниже ориентира: {value:g} против {target:g}"

        scored.append({
            "feature": name,
            "title": TITLES[name],
            "message": text,
            "impact": round(item["importance"] * abs(gap), 4),
        })

    scored.sort(key=lambda x: x["impact"], reverse=True)

    return scored[:limit]


@app.get("/health")
def health():
    return {
        "status": "ok",
        "model_loaded": True,
        "features": len(FEATURES),
        "regressor": META["regressor"]["name"],
        "classifier": META["classifier"]["name"],
    }


@app.get("/features")
def features():
    return {
        "features": [
            {
                "name": name,
                "title": TITLES[name],
                "min": LIMITS[name][0],
                "max": LIMITS[name][1],
                "unit": LIMITS[name][2],
                "target": TARGETS.get(name),
            }
            for name in FEATURES
        ]
    }


@app.get("/metrics")
def metrics():
    return META


@app.post("/predict")
def predict(log: DayLog):
    data = frame(log)

    score = float(REGRESSOR.predict(data)[0])
    label = str(CLASSIFIER.predict(data)[0])

    probabilities = {}
    if hasattr(CLASSIFIER, "predict_proba"):
        proba = CLASSIFIER.predict_proba(data)[0]
        probabilities = {
            str(cls): round(float(p), 4)
            for cls, p in zip(CLASSIFIER.classes_, proba)
        }

    return {
        "energy_score": round(max(1.0, min(score, 10.0)), 2),
        "wellbeing_class": label,
        "probabilities": probabilities,
        "advice": advice(log),
        "model_mae": META["regressor"]["mae"],
    }


@app.post("/insights")
def insights(request: HistoryRequest):
    if len(request.days) < 3:
        raise HTTPException(400, "Нужно минимум 3 дня истории")

    rows = pd.DataFrame([[getattr(d, n) for n in FEATURES] for d in request.days],
                        columns=FEATURES)
    scores = [float(REGRESSOR.predict(frame(d))[0]) for d in request.days]

    baseline = {
        name: {
            "title": TITLES[name],
            "average": round(float(rows[name].mean()), 1),
            "target": TARGETS.get(name),
            "unit": LIMITS[name][2],
        }
        for name in FEATURES
    }

    correlations = []
    if len(request.days) >= 5:
        for name in FEATURES:
            if rows[name].nunique() < 2:
                continue
            value = rows[name].corr(pd.Series(scores))
            if pd.isna(value):
                continue
            correlations.append({
                "feature": name,
                "title": TITLES[name],
                "correlation": round(float(value), 3),
            })
        correlations.sort(key=lambda x: abs(x["correlation"]), reverse=True)

    return {
        "days": len(request.days),
        "average_score": round(statistics.fmean(scores), 2),
        "best_score": round(max(scores), 2),
        "worst_score": round(min(scores), 2),
        "baseline": baseline,
        "correlations": correlations[:5],
        "model_importance": META["feature_importance"][:5],
    }
