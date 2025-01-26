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

    public async Task<Dictionary<Int64, object?>> GetAllOfKey(string key)
    {
        try
        {
            var results = new Dictionary<Int64, object?>();
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query_template = await File.ReadAllTextAsync("queries/get_all_of_key.sql");
            string query = query_template.Replace("@key", key);

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Int64 id = reader.GetInt64(0);
                object? value = reader.GetValue(1) ?? null;
                results.Add(id, value);
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