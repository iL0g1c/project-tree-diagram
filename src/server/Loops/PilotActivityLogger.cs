using Npgsql;

class PilotActivityLogger
{
    private readonly string _connectionString;
    public PilotActivityLogger(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }
    public void ExecuteProcess(List<MapApiProcessor.User> users)
    {
        // update all users that are in list to online
        // update all users that are not in list to offline
        // insert a offline-online record
        List<long> account_ids = users.Select(user => (long)user.acid).ToList();
        
        string sql = File.ReadAllText("queries/update_online_users.sql");
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, account_ids.ToArray());
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Detected {rowsAffected} users going online.");
            }
        }
        sql = File.ReadAllText("queries/update_offline_users.sql");
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, account_ids.ToArray());
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Detected {rowsAffected} users going offline.");
            }
        }


    }
}