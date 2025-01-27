using Npgsql;

class GetPatrolLogsByDateHandler
{
    public readonly string connectionString;
    public GetPatrolLogsByDateHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<List<Dictionary<string, object>>> GetPatrolLogsByDate(Int64 guild_id, DateTime date)
    {
        try
        {
            var patrols = new List<Dictionary<string, object>>();

            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/get_patrol_logs_by_date.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            cmd.Parameters.AddWithValue("@date", date);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var patrol = new Dictionary<string, object>();
                patrol["event_id"] = reader.GetInt64(0);
                patrol["discord_id"] = reader.GetInt64(1);
                patrol["start_time"] = reader.GetDateTime(2);
                patrol["end_time"] = reader.GetDateTime(3);
                patrols.Add(patrol);
            }
            return patrols;

        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1036, ex, "Error during GetPatrolLogsByDate");
            return new List<Dictionary<string, object>>();
        }
    }
}