using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

public class DatabaseServiceCoordinator : DatabaseService.DatabaseServiceBase
{
    private readonly InsertUserDiscordIdHandler _insertUserDiscordIdHandler;
    private readonly GetUserCallsignChangesHandler _getUserCallsignChangesHandler;
    private readonly UpdateUserForceCodeHandler _updateUserForceCodeHandler;
    private readonly InsertNewGuildHandler _insertNewGuildHandler;
    private readonly GetConfigurationKeysHandler _getConfigurationKeysHandler;
    private readonly UpdateConfigurationKeysHandler _updateConfigurationKeysHandler;

    public DatabaseServiceCoordinator()
    {
        _insertUserDiscordIdHandler = new InsertUserDiscordIdHandler();
        _getUserCallsignChangesHandler = new GetUserCallsignChangesHandler();
        _updateUserForceCodeHandler = new UpdateUserForceCodeHandler();
        _insertNewGuildHandler = new InsertNewGuildHandler();
        _getConfigurationKeysHandler = new GetConfigurationKeysHandler();
        _updateConfigurationKeysHandler = new UpdateConfigurationKeysHandler();
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

    public override async Task<GetConfigurationKeysResponse> GetConfigurationKeys(GetConfigurationKeysRequest request, ServerCallContext context)
    {
        try
        {
            var keys = _getConfigurationKeysHandler.GetConfigurationKeys(request.GuildId);
            var response = new GetConfigurationKeysResponse();

            foreach (var key in keys)
            {
                Any value;
                if (key.Value is string stringValue)
                {
                    value = Any.Pack(new StringValue { Value = stringValue });
                }
                else if (key.Value is long intValue)
                {
                    value = Any.Pack(new Int64Value { Value = intValue });
                }
                else if (key.Value is bool boolValue)
                {
                    value = Any.Pack(new BoolValue { Value = boolValue });
                }
                else if (key.Value == null)
                {
                    value = Any.Pack(new Empty());
                } else {
                    continue;
                }

                response.Keys.Add((string) key.Key, value);
            }
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 8: {e.Message}");
            return new GetConfigurationKeysResponse();
        }
    }
    public override async Task<UpdateConfigurationKeysResponse> UpdateConfigurationKeys(UpdateConfigurationKeysRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = _updateConfigurationKeysHandler.UpdateConfigurationKeys(request.GuildId, request.Key, request.Value);
            var response = new UpdateConfigurationKeysResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Code 7: {e.Message}");
            return new UpdateConfigurationKeysResponse();
        }
    }
}