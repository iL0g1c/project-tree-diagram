using Npgsql;
using System.Text.RegularExpressions;

class GetAllOnlinePilotsHandler
{
    public readonly string connectionString;
    public GetAllOnlinePilotsHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<List<Int64>> GetAllOnlinePilots(Int64 guild_id)
    {
        try
        {
            var online_users = new List<Int64>();

            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/get_all_online_users.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Int64 discord_id = reader.GetInt64(0);
                string callsign = reader.GetString(1);
                string[] callsign_filters = reader.IsDBNull(2) ? Array.Empty<string>() : reader.GetFieldValue<string[]>(2);
                
                bool matches = callsign_filters.Any(filter =>
                {
                    string regex_pattern = filter.Replace("[", "\\[").Replace("]", "\\]").Replace("X", ".");
                    Regex regex = new Regex(".*" + regex_pattern + ".*", RegexOptions.IgnoreCase);
                    return regex.IsMatch(callsign);
                });

                if (matches)
                {
                    online_users.Add(discord_id);
                }
            }
            return online_users;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1038, ex, "Error during GetAllOnlinePilots");
            return new List<Int64>();
        }
    }
}