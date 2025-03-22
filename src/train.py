from __future__ import annotations

import argparse
import json
import os
from pathlib import Path

import joblib
import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import RobustScaler

from src.feature_engineering import FEATURE_COLUMNS, engineer_features


def build_pipeline(random_state: int = 42) -> Pipeline:
    return Pipeline(
        steps=[
            ("scaler", RobustScaler()),
            (
                "model",
                IsolationForest(
                    n_estimators=300,
                    contamination=0.02,
                    random_state=random_state,
                    n_jobs=-1,
                ),
            ),
        ]
    )


def main() -> None:
    parser = argparse.ArgumentParser(description="Train water anomaly detection model")
    parser.add_argument("--input", required=True, help="CSV input file")
    parser.add_argument("--model-dir", default="artifacts", help="Directory for model artifacts")
    args = parser.parse_args()

    df = pd.read_csv(args.input)
    engineered = engineer_features(df)
    X = engineered.model_frame[FEATURE_COLUMNS]

    pipeline = build_pipeline()
    pipeline.fit(X)

    Path(args.model_dir).mkdir(parents=True, exist_ok=True)
    model_path = os.path.join(args.model_dir, "water_anomaly_model.joblib")
    meta_path = os.path.join(args.model_dir, "feature_metadata.json")

    joblib.dump(pipeline, model_path)
    with open(meta_path, "w", encoding="utf-8") as f:
        json.dump({"feature_columns": FEATURE_COLUMNS}, f, indent=2)

    scores = pipeline.decision_function(X)
    preds = pipeline.predict(X)
    output = engineered.full_frame.copy()
    output["anomaly_score"] = scores
    output["is_anomaly"] = (preds == -1).astype(int)
    output.to_csv(os.path.join(args.model_dir, "training_scored_output.csv"), index=False)

    print(f"Model saved to: {model_path}")
    print(f"Metadata saved to: {meta_path}")
    print("Training complete.")


if __name__ == "__main__":
    main()
