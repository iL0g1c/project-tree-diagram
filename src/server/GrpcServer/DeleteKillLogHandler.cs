using Npgsql;

class DeleteKillLogHandler
{
    public readonly string connectionString;
    
    public DeleteKillLogHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<Int64> DeleteKillLog(Int64 event_id, Int64 guild_id)
    {
        try
        {
            var delete_kill_log = await File.ReadAllTextAsync("queries/delete_kill_log.sql");
            
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(delete_kill_log, connection);
            command.Parameters.AddWithValue("@event_id", event_id);
            command.Parameters.AddWithValue("@guild_id", guild_id);

            int rows_affected = await command.ExecuteNonQueryAsync();
            if (rows_affected != 0)
            {
                return (Int64) 0;
            }
            else
            {
                return (Int64) 1;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1042, ex, "Error during DeleteKillLog");
            return (Int64) 0;
        }
    }
}