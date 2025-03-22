from __future__ import annotations

import argparse
import os
from pathlib import Path

import joblib
import pandas as pd

from src.common.config import settings
from src.common.db import get_sql_server_connection
from src.feature_engineering import FEATURE_COLUMNS, engineer_features


INSERT_SQL_TEMPLATE = """
INSERT INTO {table_name}
(
    meter_id,
    account_id,
    reading_timestamp,
    anomaly_score,
    is_anomaly,
    consumption_gallons,
    rolling_24h_avg,
    rolling_7d_avg,
    created_at
)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, SYSDATETIME())
"""


def write_predictions_to_sql(df: pd.DataFrame) -> None:
    insert_sql = INSERT_SQL_TEMPLATE.format(table_name=settings.prediction_table)
    conn = get_sql_server_connection()
    try:
        cursor = conn.cursor()
        rows = [
            (
                row.meter_id,
                row.account_id,
                row.reading_timestamp,
                float(row.anomaly_score),
                int(row.is_anomaly),
                float(row.consumption_gallons),
                float(row.rolling_24h_avg),
                float(row.rolling_7d_avg),
            )
            for row in df.itertuples(index=False)
        ]
        cursor.fast_executemany = True
        cursor.executemany(insert_sql, rows)
        conn.commit()
        print(f"Inserted {len(rows)} prediction rows into {settings.prediction_table}")
    finally:
        conn.close()


def main() -> None:
    parser = argparse.ArgumentParser(description="Batch score meter readings")
    parser.add_argument("--input", required=True, help="CSV input file")
    parser.add_argument("--model-dir", default="artifacts", help="Directory containing model artifacts")
    parser.add_argument("--output", default="artifacts/scored_output.csv", help="CSV output file")
    parser.add_argument("--write-sql", default="false", choices=["true", "false"], help="Write results to SQL Server")
    args = parser.parse_args()

    model_path = os.path.join(args.model_dir, "water_anomaly_model.joblib")
    if not os.path.exists(model_path):
        raise FileNotFoundError(f"Model file not found: {model_path}")

    df = pd.read_csv(args.input)
    engineered = engineer_features(df)
    X = engineered.model_frame[FEATURE_COLUMNS]

    model = joblib.load(model_path)
    scores = model.decision_function(X)
    preds = model.predict(X)

    output = engineered.full_frame.copy()
    output["anomaly_score"] = scores
    output["is_anomaly"] = (preds == -1).astype(int)

    Path(os.path.dirname(args.output) or ".").mkdir(parents=True, exist_ok=True)
    output.to_csv(args.output, index=False)
    print(f"Scored output saved to: {args.output}")

    if args.write_sql == "true":
        write_predictions_to_sql(output)


if __name__ == "__main__":
    main()
