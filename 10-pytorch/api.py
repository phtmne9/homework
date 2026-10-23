import pickle
from pathlib import Path

import numpy as np
import torch
import torch.nn as nn
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

BASE = Path(__file__).resolve().parent

app = FastAPI(
    title="Iris PyTorch API",
    description="Классификация ириса обученной сетью PyTorch",
    version="1.0",
)


def build(sizes):
    layers = []
    for i in range(len(sizes) - 1):
        layers.append(nn.Linear(sizes[i], sizes[i + 1]))
        if i < len(sizes) - 2:
            layers.append(nn.ReLU())

    return nn.Sequential(*layers)


checkpoint = torch.load(BASE / "model.pt", weights_only=False)
model = build(checkpoint["architecture"])
model.load_state_dict(checkpoint["state_dict"])
model.eval()

with open(BASE / "scaler.pkl", "rb") as f:
    scaler = pickle.load(f)

CLASSES = checkpoint["classes"]
FEATURES = checkpoint["features"]


class PredictRequest(BaseModel):
    features: list[float] = Field(..., description="4 признака ириса в сантиметрах")


class PredictResponse(BaseModel):
    class_id: int
    class_name: str
    probabilities: dict[str, float]


@app.get("/")
def root():
    return {"status": "ok", "model": "pytorch", "features": FEATURES, "classes": CLASSES}


@app.get("/health")
def health():
    return {"status": "OK", "model_loaded": True, "features": len(FEATURES)}


@app.post("/predict", response_model=PredictResponse)
def predict(request: PredictRequest):
    if len(request.features) != len(FEATURES):
        raise HTTPException(
            status_code=400,
            detail=f"Ожидается {len(FEATURES)} признака, получено {len(request.features)}",
        )

    scaled = scaler.transform(np.array(request.features, dtype=float).reshape(1, -1))

    with torch.no_grad():
        logits = model(torch.tensor(scaled, dtype=torch.float32))
        probabilities = torch.softmax(logits, dim=1)[0].numpy()

    class_id = int(probabilities.argmax())

    return PredictResponse(
        class_id=class_id,
        class_name=CLASSES[class_id],
        probabilities={name: round(float(p), 4) for name, p in zip(CLASSES, probabilities)},
    )
