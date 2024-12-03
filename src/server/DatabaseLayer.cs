using System;
using Npgsql;
using dotenv.net;
using System.ComponentModel;

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

        string connectionString = $"Host={host};Username={username};Password={password};Database={database}";

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        Console.WriteLine("Connected to PostgreSQL!");
    }

    public void ProcessUsers()
    {

    }
}