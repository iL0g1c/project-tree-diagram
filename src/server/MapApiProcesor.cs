using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Diagnostics;

public class MapApiProcessor
{
    private readonly System.Timers.Timer _timer;
    private readonly HttpClient _httpClient;
    private bool _isRunning;
    private DatabaseLayer _databaseLayer;

    public MapApiProcessor()
    {
        _httpClient = new HttpClient();
        _timer = new System.Timers.Timer(10000);
        _timer.Elapsed += async (sender, e) => await ExecuteEventLoop();
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

    private async Task ExecuteEventLoop()
    {
        try
        {
            object payload = new
            {
                id = string.Empty,
                gid = (object?)null
            };

            string jsonPayload = JsonSerializer.Serialize(payload);
            StringContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("https://mps.geo-fs.com/map", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                {
                    JsonElement root = document.RootElement;
                    var filteredUsers = root.GetProperty("users")
                        .EnumerateArray()
                        .Where(user =>
                            user.TryGetProperty("cs", out JsonElement csProp) &&
                            csProp.GetString() != "Foo" &&
                            user.TryGetProperty("acid", out JsonElement acidProp) &&
                            acidProp.ValueKind != JsonValueKind.Null)
                        .Select(user => new User
                        {
                            acid = user.GetProperty("acid").GetInt32(),
                            callsign = user.GetProperty("cs").GetString()
                        })
                        .ToList();
                    
                    _databaseLayer.ProcessUsers(filteredUsers);
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public class User
    {
        [JsonPropertyName("acid")]
        public int acid { get; set; } = 0;
        [JsonPropertyName("cs")]
        public string callsign { get; set; } = string.Empty;
    }
}