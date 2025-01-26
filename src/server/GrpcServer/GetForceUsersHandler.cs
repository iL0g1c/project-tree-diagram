using Npgsql;

class GetForceUsersHandler
{
    public readonly string connectionString;
    public GetForceUsersHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public List<(Int64, Int64)> GetForceUsers(Int64 guild_id)
    {
        try
        {
            List<(Int64,Int64)> results = new List<(Int64,Int64)>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = File.ReadAllText("queries/get_force_users.sql");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guild_id);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Int64 geofs_account_id = reader.GetInt64(0);
                            Int64 discord_id = reader.GetInt64(1);

                            results.Add((discord_id, geofs_account_id));
                        }
                    }
                }
            }
            Console.WriteLine($"Force Users: {results}");
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 24: {e.Message}");
            return new List<(Int64, Int64)>();
        }
    }
}