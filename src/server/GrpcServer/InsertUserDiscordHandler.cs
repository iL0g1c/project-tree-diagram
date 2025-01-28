using Npgsql;

class InsertUserDiscordIdHandler
{
    public readonly string connectionString;

    public InsertUserDiscordIdHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<bool> InsertUserDiscordId(Int64 geofs_account_id, Int64 discord_id)
    {
        try
        {
            int rows_affected = 0;
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = await File.ReadAllTextAsync("queries/insert_user_discord_id.sql");

            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@geofs_account_id", geofs_account_id);
            cmd.Parameters.AddWithValue("@discord_id", discord_id);
            rows_affected = await cmd.ExecuteNonQueryAsync();

            if (rows_affected == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1016, ex, "Error during InsertUserDiscordId");
            return false;
        }
    }
}