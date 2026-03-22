# AI-Powered Water Consumption Anomaly Detection System

Production-style starter project for detecting potential leaks and abnormal water usage from meter readings.

## About this Project

This portfolio project is built by Daniel Agbaji's using the following TechStack:
- Python and related packages/libraries
- C# / .NET services
- SQL Server, PostgreSQL, MySQL
- Utility / AMI / meter-reading analytics
- API and dashboard integration
- Data engineering across multi-tenant environments

It demonstrates:
- applied machine learning on real operational data
- feature engineering for time-series utility signals
- model training and persistence
- batch scoring into a SQL database
- real-time inference API for downstream portals/services
- integration path for .NET production systems

## Business problem

Utilities often detect leaks and abnormal consumption with static thresholds or manual review. This project builds an anomaly detection pipeline that learns normal usage patterns and assigns an anomaly score to each meter reading window.

Typical use cases:
- leak candidate detection
- unusual overnight flow detection
- abnormal daily/hourly consumption spikes
- proactive customer notifications
- operations dashboard prioritization

## Architecture

1. Ingest historical meter readings from CSV or SQL Server
2. Engineer rolling and behavioral features
3. Train an Isolation Forest anomaly model
4. Persist the trained model and feature metadata
5. Score new readings in batch mode
6. Save predictions to SQL Server
7. Expose real-time scoring endpoints through FastAPI
8. Optionally call the API from a .NET service or customer portal backend

## Repository structure

```text
ml_water_anomaly_detection/
├── README.md
├── requirements.txt
├── .env.example
├── data/
│   └── sample_meter_readings.csv
├── sql/
│   └── create_tables.sql
├── src/
│   ├── feature_engineering.py
│   ├── train.py
│   ├── score_batch.py
│   ├── common/
│   │   ├── config.py
│   │   └── db.py
│   └── api/
│       └── main.py
└── dotnet/
    ├── README.md
    └── WaterAnomalyApiClient.cs
```

## ML approach

This implementation uses `IsolationForest`, an unsupervised anomaly detection algorithm suitable when anomalies are rare and labeled leak data is limited. The model isolates unusual observations using random feature splits; shorter path lengths indicate potential outliers. FastAPI is used for serving predictions through an HTTP API, and its documentation shows straightforward support for SQL-backed applications. Microsoft also documents anomaly detection workflows in ML.NET, which gives a clean migration path if you later want a pure .NET implementation. citeturn590396search0turn590396search1turn590396search2

## Features engineered

The pipeline creates features typically useful for utility anomaly detection:
- hourly or interval consumption
- previous interval consumption
- delta from previous interval
- rolling 24-hour mean
- rolling 24-hour max
- rolling 7-day mean
- overnight usage flag
- weekend flag
- hour-of-day and day-of-week seasonality
- z-score-style deviation from rolling mean

## Demo setup

### 1) Create environment

```bash
python -m venv .venv
source .venv/bin/activate  # macOS/Linux
# .venv\Scripts\activate   # Windows
pip install -r requirements.txt
```

### 2) Train model from sample data

```bash
python -m src.train --input data/sample_meter_readings.csv --model-dir artifacts
```

### 3) Score sample data in batch

```bash
python -m src.score_batch --input data/sample_meter_readings.csv --model-dir artifacts --output artifacts/scored_output.csv
```

### 4) Run API

```bash
uvicorn src.api.main:app --reload --port 8000
```

Open docs at `http://localhost:8000/docs`

## Production SQL Server setup

1. Run `sql/create_tables.sql`
2. Copy `.env.example` to `.env`
3. Fill in SQL Server connection details
4. Use `src.score_batch` with `--write-sql true`

## Example API requests

### Health check

```bash
curl http://localhost:8000/health
```

### Predict one feature vector

```bash
curl -X POST http://localhost:8000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "meter_id": "961041632",
    "account_id": "AXTELL25",
    "reading_timestamp": "2026-03-22T01:00:00",
    "consumption_gallons": 42.8,
    "prev_consumption_gallons": 5.4,
    "rolling_24h_avg": 4.9,
    "rolling_24h_max": 8.1,
    "rolling_7d_avg": 4.2,
    "delta_from_prev": 37.4,
    "overnight_flag": 1,
    "weekend_flag": 1,
    "hour_of_day": 1,
    "day_of_week": 6,
    "deviation_from_24h_avg": 37.9
  }'
```

## Suggested portfolio bullets for LinkedIn

- Built an end-to-end machine learning system for water-usage anomaly detection using Python, scikit-learn, FastAPI, and SQL Server.
- Engineered rolling time-series features from interval meter reads to identify potential leaks and abnormal customer usage patterns.
- Developed batch and API inference workflows to support both offline analytics and real-time portal/service integration.
- Designed the solution to integrate with .NET utility platforms and multi-tenant database environments.

## Strong next upgrades

- add labeled leak events and compare supervised models
- schedule retraining with Airflow or a .NET Worker Service
- add SHAP-based explainability for investigations
- publish dashboard in Angular or Power BI
- add drift monitoring and retraining triggers

## Notes

This repo is intentionally practical and recruiter-friendly: it is easy to run locally, but the structure is also suitable for hardening into a real enterprise MVP.
