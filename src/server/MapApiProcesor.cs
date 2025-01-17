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
        _timer.Elapsed += async (sender, e) => await GetOnlineUsers();
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

    private async Task GetOnlineUsers()
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
                        .Where(user => user.ValueKind != JsonValueKind.Null)
                        .Where(user =>
                            user.TryGetProperty("cs", out JsonElement csProp) &&
                            csProp.GetString() != "Foo" &&
                            user.TryGetProperty("acid", out JsonElement acidProp) &&
                            acidProp.ValueKind != JsonValueKind.Null)
                        .Select(user =>
                        {
                            var acid = user.GetProperty("acid").GetInt32();
                            var callsign = user.GetProperty("cs").GetString() ?? string.Empty;
                            // Debug.WriteLine($"Processing User - ACID: {acid}, Callsign: {callsign}");
                            return new User
                            {
                                acid = acid,
                                callsign = callsign
                            };
                        })
                        .ToList();
                    
                    _databaseLayer.ExecuteEventLoop(filteredUsers);
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 1: {e.Message}");
            Console.WriteLine(e.StackTrace);
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