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

    public async Task<bool> UpdateConfigurationKeys(Int64 guild_id, string key, string value)
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
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string queryTemplate = await File.ReadAllTextAsync("queries/update_configuration_keys.sql");
            string query = queryTemplate.Replace("@key", key);

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guild_id", guild_id);
            cmd.Parameters.AddWithValue("@value", typedValue ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();

            Console.WriteLine($"Configuration key updated for guild_id: {result}");
            
            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1017, ex, "Error during UpdateConfigurationKeys");
            return false;
        }
    }
}