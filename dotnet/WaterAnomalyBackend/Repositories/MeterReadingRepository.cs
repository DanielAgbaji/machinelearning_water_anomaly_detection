using Dapper;
using Microsoft.Data.SqlClient;
using WaterAnomalyBackend.Models;

namespace WaterAnomalyBackend.Services;

public interface IMeterReadingRepository
{
    Task<IEnumerable<MeterReading>> GetRecentAsync(string meterId, int hours = 168);
    Task<long> InsertAsync(MeterReading reading);
}

public class MeterReadingRepository : IMeterReadingRepository
{
    private readonly string _connectionString;

    public MeterReadingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServer")!;
    }

    public async Task<IEnumerable<MeterReading>> GetRecentAsync(string meterId, int hours = 168)
    {
        const string sql = """
            SELECT TOP 1000
                reading_id      AS ReadingId,
                utility_id      AS UtilityId,
                account_id      AS AccountId,
                meter_id        AS MeterId,
                reading_timestamp AS ReadingTimestamp,
                consumption_gallons AS ConsumptionGallons,
                created_at      AS CreatedAt
            FROM dbo.meter_readings_raw
            WHERE meter_id = @MeterId
              AND reading_timestamp >= DATEADD(HOUR, -@Hours, SYSDATETIME())
            ORDER BY reading_timestamp DESC
            """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<MeterReading>(sql, new { MeterId = meterId, Hours = hours });
    }

    public async Task<long> InsertAsync(MeterReading reading)
    {
        const string sql = """
            INSERT INTO dbo.meter_readings_raw
                (utility_id, account_id, meter_id, reading_timestamp, consumption_gallons)
            VALUES
                (@UtilityId, @AccountId, @MeterId, @ReadingTimestamp, @ConsumptionGallons);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        using var conn = new SqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<long>(sql, reading);
    }
}
