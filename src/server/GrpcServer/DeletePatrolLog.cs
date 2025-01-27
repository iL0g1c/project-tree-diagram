using Npgsql;

class DeletePatrolLogHandler
{
    public readonly string connectionString;

    public DeletePatrolLogHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<Int64> DeletePatrolLog(Int64 event_id, Int64 guild_id)
    {
        var delete_patrol_log = await File.ReadAllTextAsync("queries/delete_patrol_log.sql");
        
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new NpgsqlCommand(delete_patrol_log, connection);
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
}