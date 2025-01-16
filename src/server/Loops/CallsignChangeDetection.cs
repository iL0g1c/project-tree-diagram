
using Npgsql;
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

    public void ExecuteProcess(List<MapApiProcessor.User> users)
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
        string checkExistingUser = File.ReadAllText("queries/check_for_existing_user.sql");
        string insertCallsignChangeBase = File.ReadAllText("queries/insert_callsign_change_event.sql");
        string upsertGeofsAccountBase = File.ReadAllText("queries/insert_geofs_account_base.sql");

        // ===============================================================
        // 2) Open connection & begin transaction
        // ===============================================================
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // ===============================================================
        // 3) Bulk-select existing callsigns (reuse prepared command)
        // ===============================================================
        using var checkCmd = new NpgsqlCommand(checkExistingUser, connection, transaction);
        checkCmd.Parameters.Add(new NpgsqlParameter("@account_ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer));
        checkCmd.Prepare();
        checkCmd.Parameters["@account_ids"].Value = accountIds;

        var existingCallsigns = new Dictionary<long, string>();
        using (var reader = checkCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                long existingId = reader.GetInt64(0);
                string callsign = reader.GetString(1);
                existingCallsigns.Add(existingId, callsign);
            }
        }

        // ===============================================================
        // 4) Process in memory, gather changes & upserts
        // ===============================================================
        var callsignChanges = new List<(long acid, string old_callsign, string new_callsign)>();
        var upserts = new List<(long acid, string callsign)>();

        foreach (var user in users)
        {
            // Check if callsign changed
            if (existingCallsigns.TryGetValue(user.acid, out var currentCallsign) && currentCallsign != user.callsign)
            {
                callsignChanges.Add((user.acid, currentCallsign, user.callsign));
            }

            // Attempt an upsert for every user entry
            upserts.Add((user.acid, user.callsign));
        }

        // ===============================================================
        // 5a) Batch insert callsign changes (if any)
        // ===============================================================
        if (callsignChanges.Count > 0)
        {
            string insertValues = string.Join(", ",
                callsignChanges.Select(c =>
                    $"({c.acid}, '{EscapeForSql(c.old_callsign)}', '{EscapeForSql(c.new_callsign)}', NOW())")

            );
            string insertCallsignChangeSql = $"{insertCallsignChangeBase}\n{insertValues};";
            using var insertChangeCmd = new NpgsqlCommand(insertCallsignChangeSql, connection, transaction);
            insertChangeCmd.ExecuteNonQuery();
        }

        // ===============================================================
        // 5b) Batch upsert accounts
        // ===============================================================
        if (upserts.Count > 0)
        {
            string upsertValues = string.Join(", ",
                upserts.Select(u =>
                    $"({u.acid}, '{EscapeForSql(u.callsign)}', TRUE)"
                )
            );

            string finalUpsertSql = upsertGeofsAccountBase.Replace(
                "--MULTI-ROW-PLACEHOLDER", 
                upsertValues
            );

            using var upsertCmd = new NpgsqlCommand(finalUpsertSql, connection, transaction);
            upsertCmd.ExecuteNonQuery();
        }

        // ===============================================================
        // 6) Commit transaction and done
        // ===============================================================
        transaction.Commit();
        Console.WriteLine($"Processed {users.Count} users. Inserted {callsignChanges.Count} callsign changes.");
    }
}