using Npgsql;

class ChatloggerDetection
{
    private readonly string _connectionString;
    public ChatloggerDetection(string ConnectionString)
    {
        _connectionString = ConnectionString;
    }

    public void ExecuteProcess(List<MapApiProcessor.User> users)
    {
        // Before proceeding, detection of users going offline is needed.
    }
}