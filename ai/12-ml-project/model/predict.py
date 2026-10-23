import pickle
import sys
from pathlib import Path

import numpy as np
import pandas as pd
import torch

from train import HousePriceNet

HERE = Path(__file__).resolve().parent

LIMITS = {
    "MedInc": (0.5, 15.0),
    "HouseAge": (1, 52),
    "AveRooms": (1.0, 20.0),
    "AveBedrms": (0.5, 5.0),
    "Population": (3, 35000),
    "AveOccup": (0.5, 20.0),
    "Latitude": (32.5, 42.0),
    "Longitude": (-124.5, -114.0),
}


class Predictor:
    def __init__(self):
        checkpoint = torch.load(HERE / "model.pth", weights_only=False)

        self.features = checkpoint["features"]
        self.metrics = checkpoint["metrics"]
        self.model = HousePriceNet(checkpoint["n_features"])
        self.model.load_state_dict(checkpoint["state_dict"])
        self.model.eval()

        with open(HERE / "scaler.pkl", "rb") as f:
            self.scaler = pickle.load(f)

    def validate(self, values):
        if len(values) != len(self.features):
            raise ValueError(f"Ожидается {len(self.features)} признаков, получено {len(values)}")

        errors = []
        for name, value in zip(self.features, values):
            if not isinstance(value, (int, float)) or value != value:
                errors.append(f"{name}: не число")
                continue

            low, high = LIMITS[name]
            if not low <= value <= high:
                errors.append(f"{name}: ожидается от {low} до {high}, получено {value}")

        if errors:
            raise ValueError("; ".join(errors))

    def predict(self, values):
        self.validate(values)

        frame = pd.DataFrame([list(map(float, values))], columns=self.features)
        scaled = self.scaler.transform(frame)

        with torch.no_grad():
            output = float(self.model(torch.tensor(scaled, dtype=torch.float32))[0][0])

        value = max(output, 0.0)

        return {
            "prediction": round(value, 4),
            "price_usd": round(value * 100_000, 2),
            "model_test_r2": self.metrics["test_r2"],
            "model_test_mae_usd": round(self.metrics["test_mae"] * 100_000, 2),
        }


if __name__ == "__main__":
    predictor = Predictor()

    example = [8.3, 41, 6.98, 1.02, 322, 2.55, 37.88, -122.23]
    args = [float(a) for a in sys.argv[1:]] or example

    print("Признаки:", dict(zip(predictor.features, args)))
    result = predictor.predict(args)
    print(f"\nПредсказанная цена: ${result['price_usd']:,.0f}")
    print(f"Качество модели на тесте: R2 = {result['model_test_r2']}, "
          f"средняя ошибка ${result['model_test_mae_usd']:,.0f}")
