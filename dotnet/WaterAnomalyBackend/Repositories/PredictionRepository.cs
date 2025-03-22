using Dapper;
using Microsoft.Data.SqlClient;
using WaterAnomalyBackend.Models;

namespace WaterAnomalyBackend.Services;

public interface IPredictionRepository
{
    Task<IEnumerable<AnomalyPrediction>> GetByMeterAsync(string meterId, int page = 1, int pageSize = 50);
    Task<IEnumerable<AnomalyPrediction>> GetAnomaliesAsync(int page = 1, int pageSize = 50);
    Task InsertAsync(AnomalyPrediction prediction);
}

public class PredictionRepository : IPredictionRepository
{
    private readonly string _connectionString;

    public PredictionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServer")!;
    }

    public async Task<IEnumerable<AnomalyPrediction>> GetByMeterAsync(string meterId, int page = 1, int pageSize = 50)
    {
        const string sql = """
            SELECT
                prediction_id       AS PredictionId,
                meter_id            AS MeterId,
                account_id          AS AccountId,
                reading_timestamp   AS ReadingTimestamp,
                anomaly_score       AS AnomalyScore,
                is_anomaly          AS IsAnomaly,
                consumption_gallons AS ConsumptionGallons,
                rolling_24h_avg     AS Rolling24hAvg,
                rolling_7d_avg      AS Rolling7dAvg,
                created_at          AS CreatedAt
            FROM dbo.ml_water_anomaly_predictions
            WHERE meter_id = @MeterId
            ORDER BY reading_timestamp DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<AnomalyPrediction>(sql, new
        {
            MeterId = meterId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });
    }

    public async Task<IEnumerable<AnomalyPrediction>> GetAnomaliesAsync(int page = 1, int pageSize = 50)
    {
        const string sql = """
            SELECT
                prediction_id       AS PredictionId,
                meter_id            AS MeterId,
                account_id          AS AccountId,
                reading_timestamp   AS ReadingTimestamp,
                anomaly_score       AS AnomalyScore,
                is_anomaly          AS IsAnomaly,
                consumption_gallons AS ConsumptionGallons,
                rolling_24h_avg     AS Rolling24hAvg,
                rolling_7d_avg      AS Rolling7dAvg,
                created_at          AS CreatedAt
            FROM dbo.ml_water_anomaly_predictions
            WHERE is_anomaly = 1
            ORDER BY reading_timestamp DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<AnomalyPrediction>(sql, new
        {
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });
    }

    public async Task InsertAsync(AnomalyPrediction prediction)
    {
        const string sql = """
            INSERT INTO dbo.ml_water_anomaly_predictions
                (meter_id, account_id, reading_timestamp, anomaly_score, is_anomaly,
                 consumption_gallons, rolling_24h_avg, rolling_7d_avg)
            VALUES
                (@MeterId, @AccountId, @ReadingTimestamp, @AnomalyScore, @IsAnomaly,
                 @ConsumptionGallons, @Rolling24hAvg, @Rolling7dAvg)
            """;

        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, prediction);
    }
}
