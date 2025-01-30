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
    private readonly GetAllOfKeyHandler _getAllOfKeyHandler;
    private readonly GetForceUsersHandler _getForceUsersHandler;
    private readonly InsertPatrolLogHandler _insertPatrolLogHandler;
    private readonly DeletePatrolLogHandler _deletePatrolLogHandler;
    private readonly GetPatrolLogsByDateHandler _getPatrolLogsByDateHandler;
    private readonly GetAllOnlinePilotsHandler _getAllOnlinePilotsHandler;
    private readonly InsertKillLogHandler _insertKillLogHandler;
    private readonly DeleteKillLogHandler _deleteKillLogHandler;
    private readonly GetCallsignFiltersHandler _getCallsignFiltersHandler;
    private readonly InsertCallsignFilterHandler _insertCallsignFilterHandler;
    private readonly DeleteCallsignFilterHandler _deleteCallsignFilterHandler;

    public DatabaseServiceCoordinator()
    {
        _insertUserDiscordIdHandler = new InsertUserDiscordIdHandler();
        _getUserCallsignChangesHandler = new GetUserCallsignChangesHandler();
        _updateUserForceCodeHandler = new UpdateUserForceCodeHandler();
        _insertNewGuildHandler = new InsertNewGuildHandler();
        _getConfigurationKeysHandler = new GetConfigurationKeysHandler();
        _updateConfigurationKeysHandler = new UpdateConfigurationKeysHandler();
        _getAllOfKeyHandler = new GetAllOfKeyHandler();
        _getForceUsersHandler = new GetForceUsersHandler();
        _insertPatrolLogHandler = new InsertPatrolLogHandler();
        _deletePatrolLogHandler = new DeletePatrolLogHandler();
        _getPatrolLogsByDateHandler = new GetPatrolLogsByDateHandler();
        _getAllOnlinePilotsHandler = new GetAllOnlinePilotsHandler();
        _insertKillLogHandler = new InsertKillLogHandler();
        _deleteKillLogHandler = new DeleteKillLogHandler();
        _getCallsignFiltersHandler = new GetCallsignFiltersHandler();
        _insertCallsignFilterHandler = new InsertCallsignFilterHandler();
        _deleteCallsignFilterHandler = new DeleteCallsignFilterHandler();
    }

    public override async Task<InsertUserDiscordIdResponse> InsertUserDiscordId(InsertUserDiscordIdRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _insertUserDiscordIdHandler.InsertUserDiscordId(request.GeofsAccountId, request.DiscordId);
            var response = new InsertUserDiscordIdResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1003, ex, "Error during InsertUserDiscordId");
            return new InsertUserDiscordIdResponse();
        }
    }

    public override async Task<UserCallsignChangesResponse> GetUserCallsignChanges(UserCallsignChangesRequest request, ServerCallContext context)
    {
        try
        {
            var events = await _getUserCallsignChangesHandler.GetCallsignChangesEvents(request.GeofsAccountId);
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
        catch (Exception ex)
        {
            ErrorHandler.LogError(1004, ex, "Error during GetUserCallsignChanges");
            return new UserCallsignChangesResponse();
        }
    }

    public override async Task<UpdateUserForceCodeResponse> UpdateUserForceCode(UpdateUserForceCodeRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _updateUserForceCodeHandler.UpdateUserForceCode(request.GeofsAccountId, request.DiscordId, request.GuildId);
            var response = new UpdateUserForceCodeResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1005, ex, "Error during UpdateUserForceCode");
            return new UpdateUserForceCodeResponse();
        }
    }

    public override async Task<InsertNewGuildResponse> InsertNewGuild(InsertNewGuildRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _insertNewGuildHandler.InsertNewGuild(request.GuildId);
            var response = new InsertNewGuildResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1006, ex, "Error during InsertNewGuild");
            return new InsertNewGuildResponse();
        }
    }

    public override async Task<GetConfigurationKeysResponse> GetConfigurationKeys(GetConfigurationKeysRequest request, ServerCallContext context)
    {
        try
        {
            var keys = await _getConfigurationKeysHandler.GetConfigurationKeys(request.GuildId);
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
        catch (Exception ex)
        {
            ErrorHandler.LogError(1007, ex, "Error during GetConfigurationKeys");
            return new GetConfigurationKeysResponse();
        }
    }
    public override async Task<UpdateConfigurationKeysResponse> UpdateConfigurationKeys(UpdateConfigurationKeysRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _updateConfigurationKeysHandler.UpdateConfigurationKeys(request.GuildId, request.Key, request.Value);
            var response = new UpdateConfigurationKeysResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1008, ex, "Error during UpdateConfigurationKeys");
            return new UpdateConfigurationKeysResponse();
        }
    }
    public override async Task<GetAllOfKeyResponse> GetAllOfKey(GetAllOfKeyRequest request, ServerCallContext context)
    {
        try
        {
            var keys = await _getAllOfKeyHandler.GetAllOfKey(request.Key);
            var response = new GetAllOfKeyResponse();

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

                response.Keys.Add(key.Key, value);
            }
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1009, ex, "Error during GetAllOfKey");
            return new GetAllOfKeyResponse();
        }
    }

    public override async Task<GetForceUsersResponse> GetForceUsers(GetForceUsersRequest request, ServerCallContext context)
    {
        try
        {
            var users = await _getForceUsersHandler.GetForceUsers(request.GuildId);
            var response = new GetForceUsersResponse();
            foreach (var user in users)
            {
                response.Users.Add(new UserDict
                {
                    DiscordId = user.Item1,
                    GeofsAccountId = user.Item2
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1010, ex, "Error during GetForceUsers");
            return new GetForceUsersResponse();
        }
    }

    public override async Task<InsertPatrolLogResponse> InsertPatrolLog(InsertPatrolLogRequest request, ServerCallContext context)
    {
        try
        {
            var patrolEventPackage = await _insertPatrolLogHandler.InsertPatrolLog(request.DiscordId, request.GuildId, request.StartDatetime.ToDateTime(), request.EndDatetime.ToDateTime());
            var response = new InsertPatrolLogResponse();
            foreach (var key in patrolEventPackage)
            {
                Any value;
                if (key.Value is long intValue)
                {
                    value = Any.Pack(new Int64Value { Value = intValue });
                }
                else if (key.Value == null)
                {
                    value = Any.Pack(new Empty());
                } else {
                    continue;
                }

                response.PatrolReport.Add(key.Key, value);
            }
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1034, ex, "Error during InsertPatrolLog");
            return new InsertPatrolLogResponse();
        }
    }

    public override async Task<DeletePatrolLogResponse> DeletePatrolLog(DeletePatrolLogRequest request, ServerCallContext context)
    {
        try
        {
            var response_code = await _deletePatrolLogHandler.DeletePatrolLog(request.EventId, request.GuildId);
            var response = new DeletePatrolLogResponse();
            response.ResponseCode = response_code;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1035, ex, "Error during DeletePatrolLog");
            return new DeletePatrolLogResponse();
        }
    }

    public override async Task<GetPatrolLogsByDateResponse> GetPatrolLogsByDate(GetPatrolLogsByDateRequest request, ServerCallContext context)
    {
        try
        {
            var patrol_reports = await _getPatrolLogsByDateHandler.GetPatrolLogsByDate(request.GuildId, request.TimeFrameStart.ToDateTime());
            var response = new GetPatrolLogsByDateResponse();
            foreach (var patrol in patrol_reports)
            {
                response.PatrolReports.Add(new PatrolReport
                {
                    EventId = (Int64) patrol["event_id"],
                    DiscordId = (Int64) patrol["discord_id"],
                    StartDatetime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)patrol["start_time"]),
                    EndDatetime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)patrol["end_time"])
                });
            }
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1037, ex, "Error during GetPatrolLogsByDate");
            return new GetPatrolLogsByDateResponse();
        }
    }

    public override async Task<GetAllOnlinePilotsResponse> GetAllOnlinePilots(GetAllOnlinePilotsRequest request, ServerCallContext context)
    {
        try
        {
            var online_users = await _getAllOnlinePilotsHandler.GetAllOnlinePilots(request.GuildId);
            var response = new GetAllOnlinePilotsResponse();
            response.DiscordIds.AddRange(online_users);
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1039, ex, "Error during GetAllOnlinePilots");
            return new GetAllOnlinePilotsResponse();
        }
    }

    public override async Task<InsertKillLogResponse> InsertKillLog(InsertKillLogRequest request, ServerCallContext context)
    {
        try
        {
            var killLog = await _insertKillLogHandler.InsertKillLog(request.DiscordId, request.GuildId);
            var response = new InsertKillLogResponse();
            if (killLog.Item1 != -1)
            {
                response.EventId = killLog.Item1;
                response.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime((DateTime)killLog.Item2);
                response.KillCount = killLog.Item3;
            }
            else
            {
                response.EventId = killLog.Item1;
                response.Timestamp = Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime());
                response.KillCount = killLog.Item3;
            }
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1041, ex, "Error during InsertKillLog");
            return new InsertKillLogResponse();
        }
    }

    public override async Task<DeleteKillLogResponse> DeleteKillLog(DeleteKillLogRequest request, ServerCallContext context)
    {
        try
        {
            var response_code = await _deleteKillLogHandler.DeleteKillLog(request.EventId, request.GuildId);
            var response = new DeleteKillLogResponse();
            response.ResponseCode = response_code;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1043, ex, "Error during DeleteKillLog");
            return new DeleteKillLogResponse();
        }
    }

    public override async Task<GetCallsignFiltersResponse> GetCallsignFilters(GetCallsignFiltersRequest request, ServerCallContext context)
    {
        try
        {
            var callsign_filters = await _getCallsignFiltersHandler.GetCallsignFilters(request.GuildId);
            var response = new GetCallsignFiltersResponse();
            response.Filters.AddRange(callsign_filters);
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1044, ex, "Error during GetCallsignFilters");
            return new GetCallsignFiltersResponse();
        }
    }
    public override async Task<InsertCallsignFilterResponse> InsertCallsignFilter(InsertCallsignFilterRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _insertCallsignFilterHandler.InsertCallsignFilter(request.GuildId, request.CallsignFilter);
            var response = new InsertCallsignFilterResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1046, ex, "Error during InsertCallsignFilter");
            return new InsertCallsignFilterResponse();
        }
    }

    public override async Task<DeleteCallsignFilterResponse> DeleteCallsignFilter(DeleteCallsignFilterRequest request, ServerCallContext context)
    {
        try
        {
            var isSuccessful = await _deleteCallsignFilterHandler.DeleteCallsignFilter(request.GuildId, request.CallsignFilter);
            var response = new DeleteCallsignFilterResponse();
            response.Success = isSuccessful;
            return response;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(1047, ex, "Error during DeleteCallsignFilter");
            return new DeleteCallsignFilterResponse();
        }
    }
}