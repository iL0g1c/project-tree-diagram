using System;
using Npgsql;
using dotenv.net;
using System.Diagnostics;

public class DatabaseLayer
{
    private readonly string connectionString;
    public DatabaseLayer()
    {
        DotEnv.Load();
        string? host = Environment.GetEnvironmentVariable("DB_HOST");
        string? username = Environment.GetEnvironmentVariable("DB_USER");
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        string? database = Environment.GetEnvironmentVariable("DB_NAME");

        connectionString = $"Host={host};Username={username};Password={password};Database={database}";
    }

    public void ProcessUsers(List<MapApiProcessor.User> users)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            
            string sql = File.ReadAllText("queries/insert_geofs_account.sql");

            foreach (MapApiProcessor.User user in users)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@geofs_account_id", user.acid);
                    cmd.Parameters.AddWithValue("@callsign", user.callsign);
                    
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Inserted users into database");
        }
    }
}