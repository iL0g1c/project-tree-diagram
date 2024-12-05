using System;
using dotenv.net;
using System.Diagnostics;

public class DatabaseLayer
{
    public readonly string connectionString;
    private CallsignChangeDetection callsignChangeDetection;
    public DatabaseLayer()
    {
        DotEnv.Load();
        string? host = Environment.GetEnvironmentVariable("DB_HOST");
        string? username = Environment.GetEnvironmentVariable("DB_USER");
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        string? database = Environment.GetEnvironmentVariable("DB_NAME");

        connectionString = $"Host={host};Username={username};Password={password};Database={database}";

        callsignChangeDetection = new CallsignChangeDetection(connectionString);
    }

    public void ExecuteEventLoop(List<MapApiProcessor.User> users)
    {
        callsignChangeDetection.ExecuteProcess(users);
    }
}