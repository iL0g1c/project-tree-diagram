using Npgsql;

class DeleteCallsignFilterHandler
{
    public readonly string connectionString;
    
    public DeleteCallsignFilterHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<bool> DeleteCallsignFilter(Int64 guild_id, string callsign_filer)
    {
        try
        {
            var delete_callsign_filter = await File.ReadAllTextAsync("queries/delete_callsign_filter.sql");
            
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(delete_callsign_filter, connection);
            command.Parameters.AddWithValue("@guild_id", guild_id);
            command.Parameters.AddWithValue("@callsign_filter", callsign_filer);

            int rows_affected = await command.ExecuteNonQueryAsync();
            if (rows_affected != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1047, ex, "Error during DeleteCallsignFilter");
            return false;
        }
    }
}