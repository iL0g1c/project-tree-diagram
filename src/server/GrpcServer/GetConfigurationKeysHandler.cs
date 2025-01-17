using Npgsql;

class GetConfigurationKeysHandler
{
    public readonly string connectionString;
    public GetConfigurationKeysHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public Dictionary<object,object> GetConfigurationKeys(Int64 guild_id)
    {
        try
        {
            Dictionary<object,object> results = new Dictionary<object,object>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = File.ReadAllText("queries/get_configuration_keys.sql");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guild_id);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string key = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                results[key] = value;
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Configuration Keys: {results}");
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 4: {e.Message}");
            return new Dictionary<object,object>();
        }
    }
}