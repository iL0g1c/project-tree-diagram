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
        var valid_users = new List<Dictionary<string, object?>>();

        string sql = "";
        // ===============================================================
        // 1) Update users going online
        // ===============================================================
        try {
            sql = File.ReadAllText("queries/update_online_users.sql");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
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
            sql = File.ReadAllText("queries/update_offline_users.sql");
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
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
        // 3) Package data for discord bot
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
    }
}