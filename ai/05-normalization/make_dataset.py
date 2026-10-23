import numpy as np
import pandas as pd

RNG = np.random.default_rng(42)
ROWS = 1905

NEIGHBOURHOODS = {
    "Downtown Dubai": 1.45,
    "Dubai Marina": 1.30,
    "Palm Jumeirah": 1.70,
    "Business Bay": 1.10,
    "Jumeirah Village Circle": 0.80,
    "Dubai Hills Estate": 1.20,
    "Arabian Ranches": 1.05,
    "International City": 0.55,
    "Al Furjan": 0.85,
    "Dubai Silicon Oasis": 0.70,
}

QUALITY = {"Low": 0.75, "Medium": 1.00, "High": 1.25, "Ultra": 1.60}

AMENITIES = [
    "balcony", "shared_pool", "security", "concierge", "covered_parking",
    "built_in_wardrobes", "walk_in_closet", "kitchen_appliances", "maid_room",
    "study", "central_ac", "childrens_play_area", "childrens_pool", "shared_gym",
    "shared_spa", "view_of_water", "view_of_landmark", "private_garden",
    "private_pool", "private_gym", "private_jacuzzi", "barbecue_area",
    "maid_service", "vastu_compliant", "networked", "pets_allowed",
    "unfurnished", "lobby_in_building",
]

PREMIUM = {"private_pool", "private_gym", "private_jacuzzi", "view_of_water",
           "view_of_landmark", "concierge", "private_garden", "shared_spa"}


def build():
    neighborhood = RNG.choice(list(NEIGHBOURHOODS), size=ROWS,
                              p=np.array([12, 14, 7, 13, 15, 10, 8, 9, 7, 5]) / 100)
    quality = RNG.choice(list(QUALITY), size=ROWS, p=[0.18, 0.44, 0.28, 0.10])

    bedrooms = RNG.choice([0, 1, 2, 3, 4, 5], size=ROWS,
                          p=[0.07, 0.30, 0.31, 0.19, 0.09, 0.04])
    bathrooms = np.clip(bedrooms + RNG.choice([0, 1, 2], size=ROWS, p=[0.45, 0.45, 0.10]), 1, 7)

    base_size = 420 + bedrooms * 560
    size = (base_size * RNG.normal(1.0, 0.16, ROWS)).clip(330, 9000).round().astype(int)

    df = pd.DataFrame({
        "id": np.arange(1, ROWS + 1),
        "neighborhood": neighborhood,
        "latitude": (25.08 + RNG.normal(0, 0.06, ROWS)).round(6),
        "longitude": (55.16 + RNG.normal(0, 0.08, ROWS)).round(6),
        "size_in_sqft": size,
        "no_of_bedrooms": bedrooms,
        "no_of_bathrooms": bathrooms,
        "quality": quality,
    })

    amenity_rate = np.where(df["quality"].map(QUALITY) > 1.1, 0.55, 0.32)
    premium_bonus = np.zeros(ROWS)

    for name in AMENITIES:
        rate = amenity_rate * (0.55 if name in PREMIUM else 1.0)
        flag = RNG.random(ROWS) < rate
        df[name] = flag
        if name in PREMIUM:
            premium_bonus += flag * RNG.uniform(0.02, 0.05)

    rate_per_sqft = (
        980
        * df["neighborhood"].map(NEIGHBOURHOODS).to_numpy()
        * df["quality"].map(QUALITY).to_numpy()
        * (1 + premium_bonus)
        * RNG.normal(1.0, 0.09, ROWS)
    )

    price = (df["size_in_sqft"] * rate_per_sqft + df["no_of_bathrooms"] * 18_000)
    df["price"] = price.clip(290_000, None).round().astype(int)
    df["price_per_sqft"] = (df["price"] / df["size_in_sqft"]).round(2)

    order = ["id", "neighborhood", "latitude", "longitude", "price", "size_in_sqft",
             "price_per_sqft", "no_of_bedrooms", "no_of_bathrooms", "quality"] + AMENITIES

    return df[order]


if __name__ == "__main__":
    data = build()
    data.to_csv("properties_data.csv", index=False)
    print(f"строк: {len(data)}, колонок: {len(data.columns)}")
    print(f"числовых: {data.select_dtypes('number').shape[1]}, "
          f"булевых: {data.select_dtypes('bool').shape[1]}, "
          f"категориальных: {data.select_dtypes("str").shape[1]}")
    print(f"пропусков: {int(data.isnull().sum().sum())}")
    print(data[["price", "size_in_sqft", "price_per_sqft"]].describe().round(1))
