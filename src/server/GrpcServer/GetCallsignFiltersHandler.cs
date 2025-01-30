using Npgsql;

class GetCallsignFiltersHandler
{
    public readonly string connectionString;
    public GetCallsignFiltersHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<List<string>> GetCallsignFilters(Int64 guild_id)
    {
        try
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/get_callsign_filters.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);

            var callsign_filters = new List<string>();
            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                callsign_filters.Add(reader.GetString(0));
            }
            return callsign_filters;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1039, ex, "Error during GetCallsignFilters");
            return new List<string>();
        }
    }
}