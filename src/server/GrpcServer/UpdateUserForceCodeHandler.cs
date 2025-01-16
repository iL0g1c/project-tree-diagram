using Npgsql;

class UpdateUserForceCodeHandler
{
    public readonly string connectionString;
    public UpdateUserForceCodeHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public bool UpdateUserForceCode(Int64 geofs_account_id, Int64 discord_id, Int64 guild_id)
    {
        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = File.ReadAllText("queries/update_user_force_code.sql");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@geofs_account_id", geofs_account_id);
                    cmd.Parameters.AddWithValue("@discord_id", discord_id);
                    cmd.Parameters.AddWithValue("@guild_id", guild_id);

                    var result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        Console.WriteLine("No account found with the provided geofs_account_id.");
                        return false;
                    }
                    Console.WriteLine($"Force code updated for geofs_account_id: {result}");
                }

            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 6: {e.Message}");
            return false;
        }
    }
}