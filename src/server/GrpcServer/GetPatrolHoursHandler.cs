using Google.Protobuf.WellKnownTypes;
using Npgsql;

class GetPatrolHoursHandler
{
    private readonly string connectionString;

    public GetPatrolHoursHandler()
    {
        connectionString = new DatabaseLayer().connectionString;
    }

    public async Task<double> GetPatrolHours(long guild_id, DateTime minimum_date, long discord_id)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query = discord_id != 0
                ? await File.ReadAllTextAsync("queries/get_patrol_logs.sql")
                : await File.ReadAllTextAsync("queries/get_patrol_logs_by_date.sql");

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            cmd.Parameters.AddWithValue("@date", minimum_date);
            if (discord_id != 0) cmd.Parameters.AddWithValue("@discord_id", discord_id);

            return await CalculatePatrolHours(cmd);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1036, ex, "Error during GetPatrolLogsByDate");
            return 0;
        }
    }

    private async Task<double> CalculatePatrolHours(NpgsqlCommand cmd)
    {
        double patrol_hours = 0;

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DateTime start_time = reader.GetDateTime(2);
            DateTime end_time = reader.GetDateTime(3);
            patrol_hours += (end_time - start_time).TotalHours;
        }

        return Math.Round(patrol_hours, 2);
    }
}
