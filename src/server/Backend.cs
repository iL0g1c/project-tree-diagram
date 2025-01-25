using Grpc.Core;

public class Backend
{

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting up...");
        StartGrpcServer();
        MapApiProcessor _mapApiProcessor = new MapApiProcessor();
        await _mapApiProcessor.Start();
        Task.Delay(-1).Wait();
    }

    private static void StartGrpcServer()
    {
        const int Port = 50051;
        var server = new Server
        {
            Services = { DatabaseService.BindService(new DatabaseServiceCoordinator()) },
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
        };
        
        server.Start();
        Console.WriteLine("Server listening on port " + Port);
    }

}