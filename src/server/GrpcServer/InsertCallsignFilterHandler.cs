using Npgsql;

class InsertCallsignFilterHandler
{
    public readonly string connectionString;
    public InsertCallsignFilterHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<bool> InsertCallsignFilter(Int64 guild_id, string callsign_filter)
    {
        try
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/insert_callsign_filter.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            cmd.Parameters.AddWithValue("@callsign_filter", callsign_filter);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1045, ex, "Error during InsertCallsignFilter");
            return false;
        }
    }
}