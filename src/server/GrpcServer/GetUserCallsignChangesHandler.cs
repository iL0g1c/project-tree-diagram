using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;

class GetUserCallsignChangesHandler
{
    public readonly string connectionString;

    public GetUserCallsignChangesHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }
    
    public List<(DateTime timestamp, string oldCallsign, string newCallsign)> GetCallsignChangesEvents(int user_id)
    {
        try{
            List<(DateTime, string, string)> results = new List<(DateTime, string, string)>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = File.ReadAllText("queries/get_user_callsign_changes.sql");

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", user_id);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime timestamp = reader.GetDateTime(0);
                            string oldCallsign = reader.GetString(1);
                            string newCallsign = reader.GetString(2);

                            results.Add((timestamp, oldCallsign, newCallsign));
                        }
                    }
                }
            }
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<(DateTime, string, string)>();
        }
        
    }
    
}