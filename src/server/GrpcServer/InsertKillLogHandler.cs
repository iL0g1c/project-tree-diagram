using System;
using Npgsql;

class InsertKillLogHandler
{
    public readonly string connectionString;
    public InsertKillLogHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<(Int64, DateTime, Int64)> InsertKillLog(Int64 discord_id, Int64 guild_id)
    {
        try
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/insert_kill_log.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            cmd.Parameters.AddWithValue("@discord_id", discord_id);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Int64 event_id = reader.GetInt64(0);
                DateTime timestamp = reader.GetDateTime(1);
                timestamp = TimeZoneInfo.ConvertTimeToUtc(timestamp);
                Int64 kill_count = reader.GetInt64(2) + 1;
                Console.WriteLine($"Timestamp: {timestamp}");
                
                return (event_id, timestamp, kill_count);
            }
            else
            {
                return (-1, DateTime.MinValue, -1);
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1040, ex, "Error during InsertKillLog");
            return (-1, DateTime.MinValue, -1);
        }
    }
}