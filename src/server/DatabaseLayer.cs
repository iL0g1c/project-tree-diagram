using System;
using System.Diagnostics;
using dotenv.net;

public class DatabaseLayer
{
    public readonly string connectionString;
    private CallsignChangeDetection callsignChangeDetection;
    private PilotActivityLogger pilotActivityLogger;
    public DatabaseLayer()
    {
        DotEnv.Load();
        string? host = Environment.GetEnvironmentVariable("DB_HOST");
        string? username = Environment.GetEnvironmentVariable("DB_USER");
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        string? database = Environment.GetEnvironmentVariable("DB_NAME");

        connectionString = $"Host={host};Username={username};Password={password};Database={database}";

        callsignChangeDetection = new CallsignChangeDetection(connectionString);
        pilotActivityLogger = new PilotActivityLogger(connectionString);
    }

    public void ExecuteEventLoop(List<MapApiProcessor.User> users)
    {
        Debug.WriteLine("Executing event loop");
        callsignChangeDetection.ExecuteProcess(users);
        pilotActivityLogger.ExecuteProcess(users);
    }
}