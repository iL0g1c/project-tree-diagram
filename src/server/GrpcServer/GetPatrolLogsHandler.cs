using Google.Protobuf.WellKnownTypes;
using Npgsql;

class GetPatrolLogsHandler
{
    public readonly string connectionString;
    public GetPatrolLogsHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }
    public async Task<List<Dictionary<string, object>>> GetPatrolLogs(Int64 guild_id, DateTime minimum_date, Int64 discord_id)
    {
        try
        {
            var patrols = new List<Dictionary<string, object>>();

            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            if (discord_id != 0)
            {
                string query = await File.ReadAllTextAsync("queries/get_patrol_logs.sql");

                using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guild_id", guild_id);
                cmd.Parameters.AddWithValue("@minimum_date", minimum_date);
                cmd.Parameters.AddWithValue("@discord_id", discord_id);

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
            }
            else
            {
                string query = await File.ReadAllTextAsync("queries/get_patrol_logs_by_date.sql");

                using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guild_id", guild_id);
                cmd.Parameters.AddWithValue("@date", minimum_date);

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
            }

            return patrols;

        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1051, ex, "Error during GetPatrolLogs");
            return new List<Dictionary<string, object>>();
        }
    }
}