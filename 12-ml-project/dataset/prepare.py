from pathlib import Path

from sklearn.datasets import fetch_california_housing

HERE = Path(__file__).resolve().parent


def main():
    data = fetch_california_housing(as_frame=True)
    df = data.frame
    df.to_csv(HERE / "data.csv", index=False)

    print(f"строк: {len(df)}, колонок: {len(df.columns)}")
    print(f"пропусков: {int(df.isnull().sum().sum())}")
    print(f"сохранено: {HERE / 'data.csv'}")


if __name__ == "__main__":
    main()
