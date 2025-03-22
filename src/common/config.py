from __future__ import annotations

import os
from dataclasses import dataclass
from dotenv import load_dotenv

load_dotenv()


@dataclass(frozen=True)
class Settings:
    sql_server: str = os.getenv("SQL_SERVER", "localhost")
    sql_database: str = os.getenv("SQL_DATABASE", "UtilityAnalytics")
    sql_username: str = os.getenv("SQL_USERNAME", "sa")
    sql_password: str = os.getenv("SQL_PASSWORD", "")
    sql_driver: str = os.getenv("SQL_DRIVER", "ODBC Driver 18 for SQL Server")
    sql_trust_server_certificate: str = os.getenv("SQL_TRUST_SERVER_CERTIFICATE", "yes")
    model_dir: str = os.getenv("MODEL_DIR", "artifacts")
    prediction_table: str = os.getenv("PREDICTION_TABLE", "dbo.ml_water_anomaly_predictions")


settings = Settings()
