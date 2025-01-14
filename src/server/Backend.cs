using Grpc.Core;

public class Backend
{

    public static void Main(string[] args)
    {
        StartGrpcServer();
        MapApiProcessor _mapApiProcessor = new MapApiProcessor();
        _mapApiProcessor.Start();
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