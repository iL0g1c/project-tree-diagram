using Npgsql;

class InsertPatrolLogHandler
{
    public readonly string connectionString;
    
    public InsertPatrolLogHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<Dictionary<string, object?>> InsertPatrolLog(Int64 discord_id, Int64 guild_id, DateTime start_datetime, DateTime end_datetime)
    {
        try
        {
            var update_patrol_events = await File.ReadAllTextAsync("queries/update_patrol_events_manual.sql");
            var patrol_event_package = new Dictionary<string, object?>();

            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            var command = new NpgsqlCommand(update_patrol_events, connection);
            command.Parameters.AddWithValue("@discord_id", discord_id);
            command.Parameters.AddWithValue("@guild_id", guild_id);
            command.Parameters.AddWithValue("@start_time", start_datetime);
            command.Parameters.AddWithValue("@end_time", end_datetime);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                patrol_event_package["patrol_count"] = (Int64) reader["patrol_count"] + 1;
                patrol_event_package["event_id"] = reader.GetInt64(reader.GetOrdinal("event_id"));
                patrol_event_package["patrol_log_channel_id"] = reader["patrol_log_channel_id"];
                patrol_event_package["response_code"] = (Int64) 0;
                return patrol_event_package;
            }
            else
            {
                patrol_event_package["patrol_count"] = null;
                patrol_event_package["event_id"] = null;
                patrol_event_package["patrol_log_channel_id"] = null;
                patrol_event_package["response_code"] = (Int64) 1;
                return patrol_event_package;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1033, ex, "Error during InsertPatrolLog");
            return new Dictionary<string, object?>();
        }
    }
}