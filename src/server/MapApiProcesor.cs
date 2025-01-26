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
    private static readonly HttpClient _httpClient = new HttpClient();
    private bool _isRunning;
    private DatabaseLayer _databaseLayer;

    public MapApiProcessor()
    {
        _isRunning = false;
        _databaseLayer = new DatabaseLayer();

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Tree Diagram Backends");
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
            catch (Exception ex)
            {
                ErrorHandler.LogError(1001, ex, "Error during the processing of Map API data.");
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
        catch (Exception ex)
        {
            ErrorHandler.LogError(1002, ex, "Error during the retrieval of online users from GeoFS Map API.");
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