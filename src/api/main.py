from __future__ import annotations

import json
import os
from functools import lru_cache
from typing import Any

import joblib
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from src.common.config import settings
from src.feature_engineering import FEATURE_COLUMNS

app = FastAPI(title="Water Anomaly Detection API", version="1.0.0")


class PredictionRequest(BaseModel):
    meter_id: str
    account_id: str
    reading_timestamp: str
    consumption_gallons: float = Field(ge=0)
    prev_consumption_gallons: float
    rolling_24h_avg: float
    rolling_24h_max: float
    rolling_7d_avg: float
    delta_from_prev: float
    overnight_flag: int
    weekend_flag: int
    hour_of_day: int
    day_of_week: int
    deviation_from_24h_avg: float


class PredictionResponse(BaseModel):
    meter_id: str
    account_id: str
    reading_timestamp: str
    anomaly_score: float
    is_anomaly: int
    model_name: str


@lru_cache(maxsize=1)
def get_model() -> Any:
    model_path = os.path.join(settings.model_dir, "water_anomaly_model.joblib")
    if not os.path.exists(model_path):
        raise FileNotFoundError(
            f"Model not found at {model_path}. Train a model first using src.train."
        )
    return joblib.load(model_path)


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@app.get("/model-info")
def model_info() -> dict[str, Any]:
    meta_path = os.path.join(settings.model_dir, "feature_metadata.json")
    if not os.path.exists(meta_path):
        raise HTTPException(status_code=404, detail="Feature metadata not found")
    with open(meta_path, "r", encoding="utf-8") as f:
        metadata = json.load(f)
    return metadata


@app.post("/predict", response_model=PredictionResponse)
def predict(request: PredictionRequest) -> PredictionResponse:
    model = get_model()
    ordered_values = [[getattr(request, col) for col in FEATURE_COLUMNS]]
    anomaly_score = float(model.decision_function(ordered_values)[0])
    pred = int(model.predict(ordered_values)[0])
    is_anomaly = 1 if pred == -1 else 0

    return PredictionResponse(
        meter_id=request.meter_id,
        account_id=request.account_id,
        reading_timestamp=request.reading_timestamp,
        anomaly_score=anomaly_score,
        is_anomaly=is_anomaly,
        model_name="IsolationForest",
    )
