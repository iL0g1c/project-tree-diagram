using Npgsql;

class InsertNewGuildHandler
{
    public readonly string connectionString;
    public InsertNewGuildHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public bool InsertNewGuild(Int64 guild_id)
    {
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = File.ReadAllText("queries/insert_new_guild.sql");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guild_id);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 5: {e.Message}");
            return false;
        }
    }
}