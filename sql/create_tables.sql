/*
    SQL Server objects for the anomaly detection project.
    Run in the target utility analytics database.
*/

IF OBJECT_ID('dbo.meter_readings_raw', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.meter_readings_raw
    (
        reading_id            BIGINT IDENTITY(1,1) PRIMARY KEY,
        utility_id            INT            NOT NULL,
        account_id            VARCHAR(50)    NOT NULL,
        meter_id              VARCHAR(50)    NOT NULL,
        reading_timestamp     DATETIME2(0)   NOT NULL,
        consumption_gallons   DECIMAL(18,4)  NOT NULL,
        created_at            DATETIME2(0)   NOT NULL DEFAULT SYSDATETIME()
    );
END;
GO

IF OBJECT_ID('dbo.ml_water_anomaly_predictions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ml_water_anomaly_predictions
    (
        prediction_id         BIGINT IDENTITY(1,1) PRIMARY KEY,
        meter_id              VARCHAR(50)    NOT NULL,
        account_id            VARCHAR(50)    NOT NULL,
        reading_timestamp     DATETIME2(0)   NOT NULL,
        anomaly_score         FLOAT          NOT NULL,
        is_anomaly            BIT            NOT NULL,
        consumption_gallons   DECIMAL(18,4)  NOT NULL,
        rolling_24h_avg       DECIMAL(18,4)  NOT NULL,
        rolling_7d_avg        DECIMAL(18,4)  NOT NULL,
        created_at            DATETIME2(0)   NOT NULL DEFAULT SYSDATETIME()
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ml_water_anomaly_predictions_meter_time'
      AND object_id = OBJECT_ID('dbo.ml_water_anomaly_predictions')
)
BEGIN
    CREATE INDEX IX_ml_water_anomaly_predictions_meter_time
        ON dbo.ml_water_anomaly_predictions (meter_id, reading_timestamp DESC);
END;
GO
