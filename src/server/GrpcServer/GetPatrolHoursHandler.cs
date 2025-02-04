using Google.Protobuf.WellKnownTypes;
using Npgsql;

class GetPatrolHoursHandler
{
    public readonly string connectionString;
    public GetPatrolHoursHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<double> GetPatrolHours(Int64 guild_id, DateTime minimum_date, Int64 discord_id)
    {
        try
        {
            double patrol_hours = 0;

            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            if (discord_id != 0)
            {
                string query = await File.ReadAllTextAsync("queries/get_patrol_logs.sql");

                using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guild_id", guild_id);
                cmd.Parameters.AddWithValue("@date", minimum_date);
                cmd.Parameters.AddWithValue("@discord_id", discord_id);

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    object start_time = reader.GetDateTime(2);
                    object end_time = reader.GetDateTime(3);
                    Timestamp parsed_start_time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)start_time);
                    Timestamp parsed_end_time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)end_time);
                    patrol_hours += Math.Round((parsed_end_time - parsed_start_time).Seconds / 3600.0, 2);
                }
            }
            else
            {
                string query = await File.ReadAllTextAsync("queries/get_patrol_logs_by_date.sql");

                using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@guild_id", guild_id);
                cmd.Parameters.AddWithValue("@date", minimum_date);

                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    object start_time = reader.GetDateTime(2);
                    object end_time = reader.GetDateTime(3);
                    Timestamp parsed_start_time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)start_time);
                    Timestamp parsed_end_time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)end_time);
                    patrol_hours += (parsed_end_time - parsed_start_time).Seconds / 3600.0;
                }
            }

            return patrol_hours;

        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1036, ex, "Error during GetPatrolLogsByDate");
            return (Int64)0;
        }
    }
}