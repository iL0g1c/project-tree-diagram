using Npgsql;
using System.Globalization;

class UpdateConfigurationKeysHandler
{
    public readonly string connectionString;
    public UpdateConfigurationKeysHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public bool UpdateConfigurationKeys(Int64 guild_id, string key, string value)
    {
        try
        {
            object? typedValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                typedValue = DBNull.Value;
            }
            else if (bool.TryParse(value, out bool boolResult))
            {
                typedValue = boolResult;
            }
            else if (Int64.TryParse(value, out Int64 intResult))
            {
                typedValue = intResult;
            }
            else
            {
                typedValue = value;
            }
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string queryTemplate = File.ReadAllText("queries/update_configuration_keys.sql");
                string query = queryTemplate.Replace("@key", key);

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guild_id);
                    cmd.Parameters.AddWithValue("@value", typedValue ?? DBNull.Value);

                    var result = cmd.ExecuteScalar();

                    Console.WriteLine($"Configuration key updated for guild_id: {result}");
                }

            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 7: {e.Message}");
            return false;
        }
    }
}