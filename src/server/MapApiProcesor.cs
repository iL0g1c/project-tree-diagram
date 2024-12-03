using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

public class MapApiProcessor
{
    private readonly System.Timers.Timer _timer;
    private readonly HttpClient _httpClient;
    private bool _isRunning;
    private DatabaseLayer _databaseLayer;

    public MapApiProcessor()
    {
        _httpClient = new HttpClient();
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += async (sender, e) => await ExevuteEventLoop();
        _isRunning = false;
        _databaseLayer = new DatabaseLayer();
    }

    public void Start()
    {
        if (!_isRunning)
        {
            _timer.Start();
            _isRunning = true;
            Console.WriteLine("Started monitoring of GeoFS Map API");
        }
    }

    public void Stop()
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            Console.WriteLine("Stopped monitoring of GeoFS Map API");
        }
    }

    private async Task ExevuteEventLoop()
    {
        try
        {
            var payload = new
            {
                id = "",
                gid = (object?)null
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://mps.geo-fs.com/map", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {response.StatusCode}, Content: {responseContent}");
            Console.WriteLine(responseContent);
            _databaseLayer.ProcessUsers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}