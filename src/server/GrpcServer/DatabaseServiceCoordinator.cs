using Grpc.Core;

public class DatabaseServiceCoordinator : DatabaseService.DatabaseServiceBase
{
    private readonly InsertUserDiscordIdHandler _insertUserDiscordIdHandler;
    private readonly GetUserCallsignChangesHandler _getUserCallsignChangesHandler;
    private readonly UpdateUserForceCodeHandler _updateUserForceCodeHandler;
    private readonly InsertNewGuildHandler _insertNewGuildHandler;

    public DatabaseServiceCoordinator()
    {
        _insertUserDiscordIdHandler = new InsertUserDiscordIdHandler();
        _getUserCallsignChangesHandler = new GetUserCallsignChangesHandler();
        _updateUserForceCodeHandler = new UpdateUserForceCodeHandler();
        _insertNewGuildHandler = new InsertNewGuildHandler();
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

    public override async Task<UpdateUserForceCodeResponse> UpdateUserForceCode(UpdateUserForceCodeRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = _updateUserForceCodeHandler.UpdateUserForceCode(request.GeofsAccountId, request.DiscordId, request.ForceCode);
            var response = new UpdateUserForceCodeResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 3: {e.Message}");
            return new UpdateUserForceCodeResponse();
        }
    }

    public override async Task<InsertNewGuildResponse> InsertNewGuild(InsertNewGuildRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = _insertNewGuildHandler.InsertNewGuild(request.GuildId);
            var response = new InsertNewGuildResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 4: {e.Message}");
            return new InsertNewGuildResponse();
        }
    }
}