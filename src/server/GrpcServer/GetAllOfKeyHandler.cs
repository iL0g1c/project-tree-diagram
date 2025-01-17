using System.Numerics;
using Npgsql;

class GetAllOfKeyHandler
{
    public readonly string connectionString;
    public GetAllOfKeyHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public Dictionary<Int64, object?> GetAllOfKey(string key)
    {
        try
        {
            Dictionary<Int64, object?> results = new Dictionary<Int64, object?>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query_template = File.ReadAllText("queries/get_all_of_key.sql");
                string query = query_template.Replace("@key", key);

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Int64 id = reader.GetInt64(0);
                            object? value = reader.GetValue(1);
                            results.Add(id, value);
                        }
                    }
                }
            }
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 9: {e.Message}");
            return new Dictionary<Int64, object?>();
        }
    }
}