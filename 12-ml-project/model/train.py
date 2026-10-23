import json
import pickle
from pathlib import Path

import matplotlib
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from sklearn.metrics import mean_absolute_error, r2_score
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from torch.utils.data import DataLoader, TensorDataset

matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
DATA = ROOT / "dataset" / "data.csv"

TARGET = "MedHouseVal"
EPOCHS = 40
BATCH = 256
LR = 0.005
SEED = 42


class HousePriceNet(nn.Module):
    def __init__(self, n_features):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(n_features, 64),
            nn.ReLU(),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.Linear(32, 16),
            nn.ReLU(),
            nn.Linear(16, 1),
        )

    def forward(self, x):
        return self.net(x)


def load():
    df = pd.read_csv(DATA)
    df = df[df[TARGET] < 5.0]

    X = df.drop(columns=[TARGET])
    y = df[TARGET]

    return X, y


def main():
    torch.manual_seed(SEED)
    np.random.seed(SEED)

    X, y = load()
    print(f"Данные: {X.shape[0]} строк, {X.shape[1]} признаков")

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=SEED)

    scaler = StandardScaler()
    X_train_scaled = scaler.fit_transform(X_train)
    X_test_scaled = scaler.transform(X_test)

    X_train_t = torch.tensor(X_train_scaled, dtype=torch.float32)
    X_test_t = torch.tensor(X_test_scaled, dtype=torch.float32)
    y_train_t = torch.tensor(y_train.values, dtype=torch.float32).reshape(-1, 1)
    y_test_t = torch.tensor(y_test.values, dtype=torch.float32).reshape(-1, 1)

    model = HousePriceNet(X.shape[1])
    loss_fn = nn.MSELoss()
    optimizer = torch.optim.Adam(model.parameters(), lr=LR)
    loader = DataLoader(TensorDataset(X_train_t, y_train_t), batch_size=BATCH, shuffle=True)

    history = {"train": [], "test": []}

    for epoch in range(1, EPOCHS + 1):
        model.train()
        total = 0.0

        for X_batch, y_batch in loader:
            prediction = model(X_batch)
            loss = loss_fn(prediction, y_batch)

            optimizer.zero_grad()
            loss.backward()
            optimizer.step()

            total += loss.item() * len(X_batch)

        train_loss = total / len(X_train_t)

        model.eval()
        with torch.no_grad():
            test_loss = loss_fn(model(X_test_t), y_test_t).item()

        history["train"].append(train_loss)
        history["test"].append(test_loss)

        if epoch % 5 == 0 or epoch == 1:
            print(f"epoch {epoch:>3}  train_loss={train_loss:.4f}  test_loss={test_loss:.4f}")

    model.eval()
    with torch.no_grad():
        pred_train = model(X_train_t).numpy().ravel()
        pred_test = model(X_test_t).numpy().ravel()

    metrics = {
        "train_mae": round(float(mean_absolute_error(y_train, pred_train)), 4),
        "test_mae": round(float(mean_absolute_error(y_test, pred_test)), 4),
        "train_r2": round(float(r2_score(y_train, pred_train)), 4),
        "test_r2": round(float(r2_score(y_test, pred_test)), 4),
    }

    print("\nМетрики:")
    for name, value in metrics.items():
        print(f"  {name:<10} {value}")
    print(f"\nОшибка в долларах: примерно ${metrics['test_mae'] * 100_000:,.0f}")

    torch.save({
        "state_dict": model.state_dict(),
        "n_features": X.shape[1],
        "features": list(X.columns),
        "target": TARGET,
        "metrics": metrics,
    }, HERE / "model.pth")

    with open(HERE / "scaler.pkl", "wb") as f:
        pickle.dump(scaler, f)

    with open(HERE / "metrics.json", "w", encoding="utf-8") as f:
        json.dump({"metrics": metrics, "history": history}, f, ensure_ascii=False, indent=2)

    plt.figure(figsize=(8, 4.5))
    plt.plot(history["train"], label="train loss")
    plt.plot(history["test"], label="test loss")
    plt.xlabel("Эпоха")
    plt.ylabel("MSE")
    plt.title("Обучение сети: падение ошибки")
    plt.legend()
    plt.grid(alpha=0.3)
    plt.tight_layout()
    plt.savefig(HERE / "loss.png", dpi=110)

    print(f"\nСохранено: model.pth, scaler.pkl, metrics.json, loss.png")


if __name__ == "__main__":
    main()
