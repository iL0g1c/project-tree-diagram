using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

class GetPatrolLogsHandler
{
    private readonly string connectionString;

    public GetPatrolLogsHandler()
    {
        connectionString = new DatabaseLayer().connectionString;
    }

    public async Task<List<Dictionary<string, object>>> GetPatrolLogs(long guild_id, DateTime minimum_date, long discord_id)
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

            return await FetchPatrolLogs(cmd);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1051, ex, "Error during GetPatrolLogs");
            return new List<Dictionary<string, object>>();
        }
    }

    private async Task<List<Dictionary<string, object>>> FetchPatrolLogs(NpgsqlCommand cmd)
    {
        var patrols = new List<Dictionary<string, object>>();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            patrols.Add(new Dictionary<string, object>
            {
                ["event_id"] = reader.GetInt64(0),
                ["discord_id"] = reader.GetInt64(1),
                ["start_time"] = reader.GetDateTime(2),
                ["end_time"] = reader.GetDateTime(3)
            });
        }

        return patrols;
    }
}
