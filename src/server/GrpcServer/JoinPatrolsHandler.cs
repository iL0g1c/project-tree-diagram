using Npgsql;

class JoinPatrolsHandler
{
    public readonly string connectionString;
    
    public JoinPatrolsHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<Int64> JoinPatrols(Int64 first_event_id, Int64 second_event_id, Int64 guild_id)
    {
        try
        {
            var join_patrols = await File.ReadAllTextAsync("queries/join_patrols.sql");
            
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(join_patrols, connection);
            command.Parameters.AddWithValue("@first_event_id", first_event_id);
            command.Parameters.AddWithValue("@second_event_id", second_event_id);
            command.Parameters.AddWithValue("@guild_id", guild_id);

            int rows_affected = await command.ExecuteNonQueryAsync();
            if (rows_affected != 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1049, ex, "Error during JoinPatrols");
            return 2;
        }
    }
}