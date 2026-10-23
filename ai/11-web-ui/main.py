import json
import joblib
import torch

from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse
from pydantic import BaseModel, Field

from model import HousePriceModel

app = FastAPI(
    title= "House Price Predict",
    description="Fastapi + pytorch",
    version="1.0.0"
)

app.mount("/static", StaticFiles(directory="static"), name="static")

x_scaler = joblib.load("artifacts/x_scaler.joblib")
y_scaler = joblib.load("artifacts/y_scaler.joblib")
feature_columns = joblib.load("artifacts/feature_columns.pkl")

model = HousePriceModel(input_size=len(feature_columns))

state_dict = torch.load("artifacts/house_price_model.pth", map_location="cpu")
model.load_state_dict(state_dict)

model.eval()


class HouseRequest(BaseModel):
    overall_qual: int = Field(..., ge=1, le=10)
    overall_cond: int = Field(..., ge=1, le=10)
    gr_liv_area: float = Field(..., gt=0)
    garage_cars: float = Field(..., ge=0)
    total_bsmt_sf: float = Field(..., ge=0)
    first_flr_sf: float = Field(..., gt=0)
    year_built: int = Field(..., ge=1800, le=2100)
    full_bath: int = Field(..., ge=0)
    bedroom_abvgr: int = Field(..., ge=0)
    totrms_abvgrd: int = Field(..., ge=1)

@app.get("/", response_class=HTMLResponse)
def frontend():
    with open("static/index.html", "r", encoding="utf-8") as file:
        return file.read()
    
@app.get("/health")
def health_check():
    return{
        "status": "ok",
        "model_loaded": True,
        "features_count": len(feature_columns)
    }
@app.get("/features")
def get_features():
    return{
        "features": feature_columns
    }
@app.get("/metrics")
def get_metrics():
    with open("artifacts/metrics.json", "r", encoding="utf-8") as file:
        metrics = json.load(file)
    return metrics

@app.post("/predict")
def predict_price(data: HouseRequest):
    # Формируем входные данные в том же порядке,
    # в котором модель обучалась
    input_data = [[
        data.overall_qual,
        data.overall_cond,
        data.gr_liv_area,
        data.garage_cars,
        data.total_bsmt_sf,
        data.first_flr_sf,
        data.year_built,
        data.full_bath,
        data.bedroom_abvgr,
        data.totrms_abvgrd
    ]]

    # Масштабирование
    input_scaled = x_scaler.transform(input_data)

    # numpy -> tensor
    input_tensor = torch.tensor(input_scaled, dtype=torch.float32)

    # Предсказание
    with torch.no_grad():
        prediction_scaled = model(input_tensor).numpy()

    # Обратное масштабирование
    prediction = y_scaler.inverse_transform(prediction_scaled)

    price = float(prediction[0][0])

    return {
        "predicted_price": round(price, 2),
        "currency": "USD"
    }