
using Npgsql;
class CallsignChangeDetection
{
    private readonly string _connectionString;
    public CallsignChangeDetection(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }

    public void ExecuteProcess(List<MapApiProcessor.User> users)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            string checkExistingUser = File.ReadAllText("queries/check_for_existing_user.sql");
            string insertCallsignChangeEvent = File.ReadAllText("queries/insert_callsign_change_event.sql");
            string insertGeofsAccount = File.ReadAllText("queries/insert_geofs_account.sql");

            foreach (MapApiProcessor.User user in users)
            {
                using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkExistingUser, connection))
                {
                    checkCmd.Parameters.AddWithValue("@geofs_account_id", user.acid);
                    object? currentCallsignObj = checkCmd.ExecuteScalar();
                    string? currentCallsign = currentCallsignObj as string;

                    if (currentCallsign != null && currentCallsign != user.callsign)
                    {
                        using (NpgsqlCommand insertEventCmd = new NpgsqlCommand(insertCallsignChangeEvent, connection))
                        {
                            insertEventCmd.Parameters.AddWithValue("@geofs_account_id", user.acid);
                            insertEventCmd.Parameters.AddWithValue("@old_callsign", currentCallsign);
                            insertEventCmd.Parameters.AddWithValue("@new_callsign", user.callsign);
                            insertEventCmd.ExecuteNonQuery();
                        }
                    }
                }

                using (NpgsqlCommand insertGeofsAccountCmd = new NpgsqlCommand(insertGeofsAccount, connection))
                {
                    insertGeofsAccountCmd.Parameters.AddWithValue("@geofs_account_id", user.acid);
                    insertGeofsAccountCmd.Parameters.AddWithValue("@callsign", user.callsign);
                    insertGeofsAccountCmd.ExecuteNonQuery();
                }
            }
        }
    }
}