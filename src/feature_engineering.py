from __future__ import annotations

from dataclasses import dataclass
from typing import Iterable

import numpy as np
import pandas as pd

RAW_REQUIRED_COLUMNS = [
    "meter_id",
    "account_id",
    "reading_timestamp",
    "consumption_gallons",
]

FEATURE_COLUMNS = [
    "consumption_gallons",
    "prev_consumption_gallons",
    "rolling_24h_avg",
    "rolling_24h_max",
    "rolling_7d_avg",
    "delta_from_prev",
    "overnight_flag",
    "weekend_flag",
    "hour_of_day",
    "day_of_week",
    "deviation_from_24h_avg",
]

IDENTITY_COLUMNS = [
    "meter_id",
    "account_id",
    "reading_timestamp",
]


@dataclass
class FeatureFrame:
    full_frame: pd.DataFrame
    model_frame: pd.DataFrame


def validate_raw_input(df: pd.DataFrame) -> None:
    missing = [col for col in RAW_REQUIRED_COLUMNS if col not in df.columns]
    if missing:
        raise ValueError(f"Missing required columns: {missing}")


def engineer_features(df: pd.DataFrame) -> FeatureFrame:
    validate_raw_input(df)

    data = df.copy()
    data["reading_timestamp"] = pd.to_datetime(data["reading_timestamp"])
    data = data.sort_values(["meter_id", "reading_timestamp"]).reset_index(drop=True)

    group = data.groupby("meter_id", group_keys=False)

    data["prev_consumption_gallons"] = group["consumption_gallons"].shift(1)
    data["delta_from_prev"] = data["consumption_gallons"] - data["prev_consumption_gallons"]

    # Use row counts as a practical proxy for hourly intervals in the demo dataset.
    data["rolling_24h_avg"] = group["consumption_gallons"].transform(
        lambda s: s.rolling(window=24, min_periods=1).mean()
    )
    data["rolling_24h_max"] = group["consumption_gallons"].transform(
        lambda s: s.rolling(window=24, min_periods=1).max()
    )
    data["rolling_7d_avg"] = group["consumption_gallons"].transform(
        lambda s: s.rolling(window=24 * 7, min_periods=1).mean()
    )

    data["hour_of_day"] = data["reading_timestamp"].dt.hour
    data["day_of_week"] = data["reading_timestamp"].dt.dayofweek
    data["overnight_flag"] = data["hour_of_day"].isin([0, 1, 2, 3, 4]).astype(int)
    data["weekend_flag"] = (data["day_of_week"] >= 5).astype(int)
    data["deviation_from_24h_avg"] = data["consumption_gallons"] - data["rolling_24h_avg"]

    data = data.replace([np.inf, -np.inf], np.nan)
    data[FEATURE_COLUMNS] = data[FEATURE_COLUMNS].fillna(0.0)

    model_frame = data[IDENTITY_COLUMNS + FEATURE_COLUMNS].copy()
    return FeatureFrame(full_frame=data, model_frame=model_frame)
