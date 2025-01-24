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
    private readonly HttpClient _httpClient;
    private bool _isRunning;
    private DatabaseLayer _databaseLayer;

    public MapApiProcessor()
    {
        _httpClient = new HttpClient();
        _isRunning = false;
        _databaseLayer = new DatabaseLayer();
    }

    public async Task Start()
    {
        Console.WriteLine("Starting MapApiProcessor...");
        _isRunning = true;

        while (_isRunning)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                await ProcessUsers();

                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;

                Console.WriteLine($"Processed users in {stopwatch.Elapsed} seconds.");

                var remaining_delay = 10000 - elapsed;

                if (remaining_delay > 0)
                {
                    await Task.Delay((int)remaining_delay);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during processing: {e.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        Console.WriteLine("Stopped monitoring of GeoFS Map API");
    }

    private async Task ProcessUsers()
    {
        List<User> users = await GetOnlineUsers();
        await _databaseLayer.ExecuteEventLoop(users);
    }

    private async Task<List<User>> GetOnlineUsers()
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
                    return filteredUsers;
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 1: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
        return new List<User>();
    }

    public class User
    {
        [JsonPropertyName("acid")]
        public int acid { get; set; } = 0;
        [JsonPropertyName("cs")]
        public string callsign { get; set; } = string.Empty;
    }
}