using Npgsql;

class GetPatrolHoursHandler
{
    public readonly string connectionString;
    public GetPatrolHoursHandler()
    {
        var dbLayer = new DatabaseLayer();
        connectionString = dbLayer.connectionString;
    }

    public async Task<Int64> GetPatrolHours(Int64 guild_id, DateTime minimum_date, Int64 discord_id)
    {
        try
        {
            Int64 patrol_hours = 0;

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
                    var patrol = new Dictionary<string, object>();
                    DateTime start_time = reader.GetDateTime(2);
                    DateTime end_time = reader.GetDateTime(3);
                    patrol_hours += (Int64)(end_time - start_time).TotalMinutes;
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
                    DateTime start_time = reader.GetDateTime(2);
                    DateTime end_time = reader.GetDateTime(3);
                    patrol_hours += (Int64)(end_time - start_time).TotalMinutes;
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