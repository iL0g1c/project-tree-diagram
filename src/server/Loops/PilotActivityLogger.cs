using Npgsql;
using System.Text;
using System.Text.Json;

class PilotActivityLogger
{
    private readonly string _connectionString;
    public PilotActivityLogger(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }
    public async Task ExecuteProcess(List<MapApiProcessor.User> users)
    {
        NpgsqlConnection connection = null!;
        try {
            List<long> account_ids = users.Select(user => (long)user.acid).ToList();
            var result_list = new List<Dictionary<string, object?>>();
            string sql = "";
            // ===============================================================
            // 1) Update users going online
            // ===============================================================
            try {
                sql = File.ReadAllText("queries/update_online_users.sql");
                connection = new NpgsqlConnection(_connectionString);
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
                connection = new NpgsqlConnection(_connectionString);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error Code: 22 | Failed to update users going offline: {ex.Message}");
                return;
            }

            // ===============================================================
            // 3) Send activity updates to discord bot
            // ===============================================================
            try {
                if (result_list.Count > 0)
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
        finally
        {
            await connection.CloseAsync();
        }
    }
}