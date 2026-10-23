import sys
from pathlib import Path

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel, Field

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / "model"))

from predict import LIMITS, Predictor

app = FastAPI(
    title="House Price Predictor",
    description="Предсказание стоимости жилья в Калифорнии нейронной сетью PyTorch",
    version="1.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173", "http://127.0.0.1:8000"],
    allow_methods=["GET", "POST"],
    allow_headers=["*"],
)

predictor = Predictor()


class PredictRequest(BaseModel):
    MedInc: float = Field(..., description="Средний доход в районе, десятки тысяч долларов")
    HouseAge: float = Field(..., description="Средний возраст домов")
    AveRooms: float = Field(..., description="Среднее число комнат")
    AveBedrms: float = Field(..., description="Среднее число спален")
    Population: float = Field(..., description="Население района")
    AveOccup: float = Field(..., description="Среднее число жильцов на дом")
    Latitude: float = Field(..., description="Широта")
    Longitude: float = Field(..., description="Долгота")


@app.get("/health")
def health():
    return {
        "status": "OK",
        "model_loaded": True,
        "features": len(predictor.features),
        "test_r2": predictor.metrics["test_r2"],
    }


@app.get("/features")
def features():
    return {
        "features": predictor.features,
        "limits": {name: {"min": low, "max": high} for name, (low, high) in LIMITS.items()},
    }


@app.post("/predict")
def predict(request: PredictRequest):
    values = [getattr(request, name) for name in predictor.features]

    try:
        return predictor.predict(values)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


app.mount("/static", StaticFiles(directory=ROOT / "frontend"), name="static")


@app.get("/")
def index():
    return FileResponse(ROOT / "frontend" / "index.html")
