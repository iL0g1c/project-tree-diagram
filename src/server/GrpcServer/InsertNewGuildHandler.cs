using System.Threading.Tasks;
using Npgsql;

class InsertNewGuildHandler
{
    public readonly string connectionString;
    public InsertNewGuildHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<bool> InsertNewGuild(Int64 guild_id)
    {
        try
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/insert_new_guild.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            await cmd.ExecuteNonQueryAsync();
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 5: {e.Message}");
            return false;
        }
    }
}