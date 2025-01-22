using Npgsql;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class PilotActivityLogger
{
    private readonly string _connectionString;
    public PilotActivityLogger(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }
    public async Task ExecuteProcess(List<MapApiProcessor.User> users)
    {
        List<long> account_ids = users.Select(user => (long)user.acid).ToList();
        var result_list = new List<Dictionary<string, object?>>();
        var users_going_offline = new List<Dictionary<string, object?>>();
        var patrol_events = new List<Dictionary<string, object?>>();
        var valid_users = new List<Dictionary<string, object?>>();

        string sql_insert_patrol_event = "";
        // ===============================================================
        // 1) Update users going online
        // ===============================================================
        try {
            sql_insert_patrol_event = File.ReadAllText("queries/update_online_users.sql");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql_insert_patrol_event, connection))
                {
                    command.Parameters.AddWithValue("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, account_ids.ToArray());
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row_dictionary = new Dictionary<string, object?>();

                            Int64 geofs_account_id = reader.GetInt64(reader.GetOrdinal("geofs_account_id"));
                            row_dictionary.Add("geofs_account_id", geofs_account_id);

                            Int64? discord_id = reader.IsDBNull(reader.GetOrdinal("discord_id")) ? null : (Int64?)reader.GetInt64(reader.GetOrdinal("discord_id"));
                            row_dictionary.Add("discord_id", discord_id);

                            string callsign = reader.GetString(reader.GetOrdinal("callsign"));
                            row_dictionary.Add("callsign", callsign);

                            string? force_code = reader.IsDBNull(reader.GetOrdinal("force_code")) ? null : reader.GetString(reader.GetOrdinal("force_code"));
                            row_dictionary.Add("force_code", force_code);

                            bool is_online = reader.GetBoolean(reader.GetOrdinal("is_online"));
                            row_dictionary.Add("is_online", is_online);

                            result_list.Add(row_dictionary);
                        }
                        Console.WriteLine($"Detected {result_list.Count} users going online.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 21 | Failed to update users going online: {ex.Message}");
            return;
        }

        // ===============================================================
        // 2) Update users going offline
        // ===============================================================
        try {
            sql_insert_patrol_event = File.ReadAllText("queries/update_offline_users.sql");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql_insert_patrol_event, connection))
                {
                    command.Parameters.AddWithValue("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, account_ids.ToArray());
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row_dictionary = new Dictionary<string, object?>();

                            Int64 geofs_account_id = reader.GetInt64(reader.GetOrdinal("geofs_account_id"));
                            row_dictionary.Add("geofs_account_id", geofs_account_id);

                            Int64? discord_id = reader.IsDBNull(reader.GetOrdinal("discord_id")) ? null : (Int64?)reader.GetInt64(reader.GetOrdinal("discord_id"));
                            row_dictionary.Add("discord_id", discord_id);

                            string callsign = reader.GetString(reader.GetOrdinal("callsign"));
                            row_dictionary.Add("callsign", callsign);

                            string? force_code = reader.IsDBNull(reader.GetOrdinal("force_code")) ? null : reader.GetString(reader.GetOrdinal("force_code"));
                            row_dictionary.Add("force_code", force_code);

                            bool is_online = reader.GetBoolean(reader.GetOrdinal("is_online"));
                            row_dictionary.Add("is_online", is_online);

                            result_list.Add(row_dictionary);
                            users_going_offline.Add(row_dictionary);
                        }
                        Console.WriteLine($"Detected {result_list.Count} users going offline.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 22 | Failed to update users going offline: {ex.Message}");
            return;
        }
        // ===============================================================
        // 3) Package activty updates for discord bot
        // ===============================================================
        try {
            Dictionary<Int64, object> callsign_formats = new Dictionary<Int64, object>();
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query_template = File.ReadAllText("queries/get_all_of_key.sql");
                string query = query_template.Replace("@key", "callsign_format");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Int64 id = reader.GetInt64(0);
                            string? username = reader.IsDBNull(1) ? null : reader.GetString(1);
                            if (username == null)
                            {
                                continue;
                            }
                            callsign_formats.Add(id, username);
                        }
                    }
                }
            }
            foreach (var user in result_list)
            {
                if (user["force_code"] == null)
                {
                    continue;
                }
                foreach (Int64 guild_id in callsign_formats.Keys.ToList())
                {
                    string? callsign_format = callsign_formats[guild_id].ToString();
                    if (callsign_format == null || user["callsign"] == null)
                    {
                        continue;
                    }
                    string regex_pattern = callsign_format.Replace("[", "\\[").Replace("]", "\\]").Replace("X", ".");
                    Regex regex = new Regex(".*" + regex_pattern + ".*", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(user["callsign"].ToString()))
                    {
                        valid_users.Add(user);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 23 | Failed to package data for Discord bot: {ex.Message}");
            return;
        }

        // ===============================================================
        // 4) Send activity updates to discord bot
        // ===============================================================
        try {
            if (valid_users.Count > 0)
            {
                var httpClient = new HttpClient();
                string json = JsonSerializer.Serialize(result_list);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("http://localhost:5001/player-activity-change", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error Code: 19 | Failed to send activity updates to Discord bot: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 20 | Failed to send activity updates to Discord bot: {ex.Message}");
            return;
        }
        // ===============================================================
        // 6) Send patrol events to database and package events for Discord bot
        // ===============================================================
        try {
            // Get total number of patrols for the force.
            var sql_get_patrol_count = File.ReadAllText("queries/get_patrol_count.sql");
            var sql_get_guild_id_from_force_code = File.ReadAllText("queries/get_guild_id_from_force_code.sql");
            sql_insert_patrol_event = File.ReadAllText("queries/insert_patrol_event.sql");
            
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var patrol_event_package = new Dictionary<string, object?>();
                foreach (var user in users_going_offline)
                {
                    if (user["force_code"] == null)
                    {
                        continue;
                    }

                    using (NpgsqlCommand command = new NpgsqlCommand(sql_get_guild_id_from_force_code, connection))
                    {
                        command.Parameters.AddWithValue("@force_code", user["force_code"]);
                        patrol_event_package.Add("guild_id", await command.ExecuteScalarAsync());
                    }

                    // Get total number of patrols for a certain geofs account.
                    var end_time = DateTime.UtcNow;
                    using (NpgsqlCommand command = new NpgsqlCommand(sql_get_patrol_count, connection))
                    {
                        command.Parameters.AddWithValue("@geofs_account_id", user["geofs_account_id"] ?? DBNull.Value);
                        patrol_event_package.Add("patrol_count", await command.ExecuteScalarAsync());
                    }

                    using (NpgsqlCommand command = new NpgsqlCommand(sql_insert_patrol_event, connection))
                    {
                        command.Parameters.AddWithValue("@geofs_account_id", user["geofs_account_id"] ?? DBNull.Value);
                        command.Parameters.AddWithValue("@force_code", user["force_code"] ?? DBNull.Value);
                        command.Parameters.AddWithValue("@end_time", end_time);
                        
                        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                
                                patrol_event_package.Add("start_time", reader.GetDateTime(reader.GetOrdinal("start_time")));
                                patrol_event_package.Add("event_id", reader.GetInt64(reader.GetOrdinal("event_id")));
                            }
                        }
                    }

                    patrol_event_package.Add("discord_id", user["discord_id"]);
                    patrol_event_package.Add("end_time", end_time);
                    patrol_event_package.Add("duration", (DateTime)patrol_event_package["end_time"] - (DateTime)patrol_event_package["start_time"]);
                    patrol_events.Add(patrol_event_package);
                }
            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 24 | Failed to send patrol events to database and package data for discord bot: {ex.Message}");
            return;
        }
        // ===============================================================
        // 7) Send patrol events for Discord bot
        // ===============================================================
        try {
            if (patrol_events.Count > 0)
            {
                var httpClient = new HttpClient();
                string json = JsonSerializer.Serialize(patrol_events);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("http://localhost:5001/patrol-event", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error Code: 25 | Failed to send patrol events to Discord bot: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Code: 25 | Failed to send patrol events to Discord bot: {ex.Message}");
            return;
        }
    }
}