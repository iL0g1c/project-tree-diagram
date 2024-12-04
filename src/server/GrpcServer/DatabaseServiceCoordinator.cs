using Grpc.Core;

public class DatabaseServiceCoordinator : UserCallsignChangesService.UserCallsignChangesServiceBase
{
    private readonly new GetUserCallsignChangesHandler _getUserCallsignChangesHandler;

    public DatabaseServiceCoordinator()
    {
        _getUserCallsignChangesHandler = new GetUserCallsignChangesHandler();
    }

    public override async Task<UserCallsignChangesResponse> GetUserCallsignChanges(UserCallsignChangesRequest request, ServerCallContext context)
    {
        try {
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
            Console.WriteLine(e);
            return new UserCallsignChangesResponse();
        }
    }
}