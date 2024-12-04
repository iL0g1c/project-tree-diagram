using Grpc.Core;

public class DatabaseServiceCoordinator : DatabaseService.DatabaseServiceBase
{
    private readonly InsertUserDiscordIdHandler _insertUserDiscordIdHandler;
    private readonly GetUserCallsignChangesHandler _getUserCallsignChangesHandler;

    public DatabaseServiceCoordinator()
    {
        _insertUserDiscordIdHandler = new InsertUserDiscordIdHandler();
        _getUserCallsignChangesHandler = new GetUserCallsignChangesHandler();
    }

    public override async Task<InsertUserDiscordIdResponse> InsertUserDiscordId(InsertUserDiscordIdRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = _insertUserDiscordIdHandler.InsertUserDiscordId(request.GeofsAccountId, request.DiscordId);
            var response = new InsertUserDiscordIdResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 5: {e.Message}");
            return new InsertUserDiscordIdResponse();
        }
    }

    public override async Task<UserCallsignChangesResponse> GetUserCallsignChanges(UserCallsignChangesRequest request, ServerCallContext context)
    {
        try
        {
            var events = _getUserCallsignChangesHandler.GetCallsignChangesEvents(request.GeofsAccountId);
            var response = new UserCallsignChangesResponse();

            foreach (var evt in events)
            {
                response.Events.Add(new UserCallsignChangeEvent
                {
                    Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(evt.timestamp.ToUniversalTime()),
                    OldCallsign = evt.oldCallsign,
                    NewCallsign = evt.newCallsign
                });
            }
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 2: {e.Message}");
            return new UserCallsignChangesResponse();
        }
    }
}