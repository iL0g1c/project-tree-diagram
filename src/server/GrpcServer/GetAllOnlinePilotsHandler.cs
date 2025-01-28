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
                string callsign_format = reader.GetString(2);
                string regex_pattern = callsign_format.Replace("[", "\\[").Replace("]", "\\]").Replace("X", ".");
                Regex regex = new Regex(".*" + regex_pattern + ".*", RegexOptions.IgnoreCase);
                if (regex.IsMatch(callsign))
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