using Npgsql;
using System.Text;
using System.Text.Json;
class CallsignChangeDetection
{
    private readonly string _connectionString;
    public CallsignChangeDetection(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }

    private static string EscapeForSql(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? "";
        
        // Replace single quotes with two single quotes
        return value.Replace("'", "''");
    }

    public async Task ExecuteProcess(List<MapApiProcessor.User> users)
    {
        if (users == null || users.Count == 0)
        {
            return;
        }

        // Distinct Account IDs to look up
        var accountIds = users.Select(u => u.acid).Distinct().ToArray();

        // ===============================================================
        // 1) Load SQL queries from files
        // ===============================================================
        string checkExistingUser;
        string insertCallsignChangeBase;
        string upsertGeofsAccountBase;
        try {
            checkExistingUser = File.ReadAllText("queries/check_for_existing_user.sql");
            insertCallsignChangeBase = File.ReadAllText("queries/insert_callsign_change_event.sql");
            upsertGeofsAccountBase = File.ReadAllText("queries/insert_geofs_account_base.sql");
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1019, ex, "Failed to load SQL queries from files.");
            return;
        }

        NpgsqlConnection connection = null!;
        NpgsqlTransaction transaction = null!;

        try {
            // ===============================================================
            // 2) Open connection & begin transaction
            // ===============================================================
            try {
                connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                transaction = await connection.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1020, ex, "Failed to open connection or begin transaction.");
                return;
            }

            // ===============================================================
            // 3) Bulk-select existing callsigns (reuse prepared command)
            // ===============================================================
            var existingCallsigns = new Dictionary<long, string>();
            try {
                using var checkCmd = new NpgsqlCommand(checkExistingUser, connection, transaction);
                checkCmd.Parameters.Add(new NpgsqlParameter("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint));
                await checkCmd.PrepareAsync();
                checkCmd.Parameters["@account_ids"].Value = accountIds;

                existingCallsigns = new Dictionary<long, string>();
                using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        long existingId = reader.GetInt64(0);
                        string callsign = reader.GetString(1);
                        existingCallsigns.Add(existingId, callsign);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1021, ex, "Failed to bulk-select existing callsigns.");
                return;
            }

            // ===============================================================
            // 4) Process in memory, gather changes & upserts
            // ===============================================================
            var callsignChanges = new List<(long acid, string old_callsign, string new_callsign)>();
            var upserts = new List<(long acid, string callsign)>();
            var upserts_raw = new List<(long acid, string callsign)>();
            try {
                callsignChanges = new List<(long acid, string old_callsign, string new_callsign)>();
                upserts_raw = new List<(long acid, string callsign)>();

                foreach (var user in users)
                {
                    // Check if callsign changed
                    if (existingCallsigns.TryGetValue(user.acid, out var currentCallsign) && currentCallsign != user.callsign)
                    {
                        callsignChanges.Add((user.acid, currentCallsign, user.callsign));
                    }

                    // Attempt an upsert for every user entry
                    upserts_raw.Add((user.acid, user.callsign));
                }

                upserts = upserts_raw.GroupBy(x => x.acid).Select(g => g.Last()).ToList();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1022, ex, "Failed to process in memory, gather changes & upserts.");
                return;
            }

            // ===============================================================
            // 5a) Batch insert callsign changes (if any)
            // ===============================================================
            try {
                if (callsignChanges.Count > 0)
                {
                    string utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    string insertValues = string.Join(", ",
                        callsignChanges.Select(c =>
                            $"({c.acid}, '{EscapeForSql(c.old_callsign)}', '{EscapeForSql(c.new_callsign)}', '{utcNow}')")

                    );
                    string insertCallsignChangeSql = $"{insertCallsignChangeBase}\n{insertValues};";
                    using var insertChangeCmd = new NpgsqlCommand(insertCallsignChangeSql, connection, transaction);
                    await insertChangeCmd.ExecuteNonQueryAsync();
                }
                Console.WriteLine($"Processed {callsignChanges.Count} callsign changes");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1023, ex, "Failed to batch insert callsign changes.");
                return;
            }

            // ===============================================================
            // 5b) Batch upsert accounts
            // ===============================================================
            try {
                if (upserts.Count > 0)
                {
                    string upsertValues = string.Join(", ",
                        upserts.Select(u =>
                            $"({u.acid}, '{EscapeForSql(u.callsign)}')"
                        )
                    );

                    string finalUpsertSql = upsertGeofsAccountBase.Replace(
                        "--MULTI-ROW-PLACEHOLDER", 
                        upsertValues
                    );

                    using var upsertCmd = new NpgsqlCommand(finalUpsertSql, connection, transaction);
                    await upsertCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1024, ex, "Failed to batch upsert accounts.");
                return;
            }

            // ===============================================================
            // 6) Send new callsign changes to discord bot
            // ===============================================================
            try {
                if (callsignChanges.Count > 0)
                {
                    var httpClient = new HttpClient();
                    List<object> data = new List<object>();
                    foreach (var user in callsignChanges)
                    {
                        var user_data = new
                        {
                            acid = user.acid,
                            old_callsign = user.old_callsign,
                            new_callsign = user.new_callsign
                        };
                        data.Add(user_data);
                    }
                    string json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("http://localhost:5001/callsign-changes", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to send callsign changes to Discord bot: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1025, ex, "Failed to send new callsign changes to Discord bot.");
                return;
            }

            // ===============================================================
            // 7) Commit transaction and done
            // ===============================================================
            try {
                await transaction.CommitAsync();
                    
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(1026, ex, "Failed to commit transaction.");
                return;
            }
        }
        finally
        {
            transaction?.Dispose();
            connection?.Dispose();
        }
    }
}