import os
import json
import joblib
import ssl

import numpy as np
import pandas as pd

import torch
import torch.nn as nn
from torch.utils.data import TensorDataset, DataLoader

from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

from model import HousePriceModel

ssl._create_default_https_context = ssl._create_unverified_context

DATA_URL = "http://jse.amstat.org/v19n3/decock/AmesHousing.txt"

os.makedirs("artifacts", exist_ok=True)

df = pd.read_csv(DATA_URL, sep="\t")

print("Dataset shape:", df.shape)
print(df.head())

features = [
    "Overall Qual",
    "Overall Cond",
    "Gr Liv Area",
    "Garage Cars",
    "Total Bsmt SF",
    "1st Flr SF",
    "Year Built",
    "Full Bath",
    "Bedroom AbvGr",
    "TotRms AbvGrd"
]
target = "SalePrice"

df = df[features + [target]]

df = df.dropna()

X = df[features].values.astype(np.float32)
y = df[target].values.astype(np.float32).reshape(-1,1)
print("X shape:", X.shape)
print("y shape:", y.shape)

X_train,X_test,y_train,y_test = train_test_split(
    X,
    y,
    test_size=0.2,
    random_state=42
)
x_scaler = StandardScaler()
y_scaler = StandardScaler()

X_train_scaled = x_scaler.fit_transform(X_train)
X_test_scaled = x_scaler.transform(X_test)

y_train_scaled = y_scaler.fit_transform(y_train)
y_test_scaled = y_scaler.transform(y_test)

X_train_tensor = torch.tensor(X_train_scaled, dtype=torch.float32)
y_train_tensor = torch.tensor(y_train_scaled,dtype=torch.float32)

X_test_tensor = torch.tensor(X_test_scaled, dtype=torch.float32)

train_dataset = TensorDataset(X_train_tensor, y_train_tensor)

train_loader = DataLoader(
    train_dataset,
    batch_size = 32,
    shuffle = True
)

input_size = X_train_tensor.shape[1]
model = HousePriceModel(input_size=input_size)

criterion = nn.MSELoss()
optimizer = torch.optim.Adam(model.parameters(), lr=0.001)

epochs = 250

for epoch in range(epochs):
    model.train()

    total_loss = 0

    for batch_X, batch_y in train_loader:
        predictions = model(batch_X)

        loss = criterion(predictions,batch_y)

        optimizer.zero_grad()
        loss.backward()
        optimizer.step()

        total_loss += loss.item()

    if (epoch + 1) % 25 == 0:
        avg_loss = total_loss / len(train_loader)
        print(f"Epoch[{epoch + 1}/{epochs}], Loss: {avg_loss:.4f}")

model.eval()

with torch.no_grad():
    test_predictions_scaled = model(X_test_tensor).cpu().detach().numpy()

test_predictions = y_scaler.inverse_transform(test_predictions_scaled)
y_test_original = y_test

# ==========================================
# 1. РАСЧЕТ И ВЫВОД МЕТРИК
# ==========================================
mae = mean_absolute_error(y_test_original, test_predictions)
mse = mean_squared_error(y_test_original, test_predictions)
rmse = np.sqrt(mse)  # RMSE считается как корень из MSE
r2 = r2_score(y_test_original, test_predictions)

print("\n=== Evaluation Metrics ===")
print(f"MAE  (Mean Absolute Error): {mae:.2f}")
print(f"MSE  (Mean Squared Error) : {mse:.2f}")
print(f"RMSE (Root MSE)           : {rmse:.2f}")
print(f"R² Score (Accuracy)       : {r2:.4f}")

# Собираем метрики в словарь для сохранения
metrics = {
    "mae": float(mae),
    "mse": float(mse),
    "rmse": float(rmse),
    "r2": float(r2)
}

# ==========================================
# 2. СОХРАНЕНИЕ В ARTIFACTS
# ==========================================
print("\nSaving artifacts...")

# Сохраняем веса PyTorch модели
torch.save(model.state_dict(), "artifacts/house_price_model.pth")



joblib.dump(x_scaler, "artifacts/x_scaler.joblib")
joblib.dump(y_scaler, "artifacts/y_scaler.joblib")
joblib.dump(features, "artifacts/feature_columns.pkl")

# Сохраняем метрики в JSON файл
with open("artifacts/metrics.json", "w", encoding="utf-8") as f:
    json.dump(metrics, f, indent=4)

print("All artifacts saved successfully in 'artifacts/' directory!")
