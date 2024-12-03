

public class Backend
{

    public static void Main(string[] args)
    {
        MapApiProcessor _mapApiProcessor = new MapApiProcessor();
        Console.WriteLine("Type 'start' to start, 'stop' to stop, and 'quit' to exit.");

        while (true)
        {
            string? input = Console.ReadLine()?.ToLower();

            switch (input)
            {
                case "start":
                    _mapApiProcessor.Start();
                    break;
                case "stop":
                    _mapApiProcessor.Stop();
                    break;
                case "quit":
                    return;
                default:
                    Console.WriteLine("Invalid input. Use 'start', 'stop', or 'quit'.");
                    break;
            }
        }
    }

}