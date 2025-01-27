from proto import database_service_pb2
import utils.handleProtobufUnpacking as handleProtobufUnpacking
import utils.GrpcClient as GrpcClient

class ConfigManager:
    def __init__(self):
        self.host = "localhost:50051"
        self.grpc_client = GrpcClient.GrpcClient()

    def get_config(self, guild_id):
        request = database_service_pb2.GetConfigurationKeysRequest(guild_id=guild_id)
        response = self.grpc_client.call_method("DatabaseService", "GetConfigurationKeys", request)
        keys = handleProtobufUnpacking.unpack(response.keys)
        return keys

    def update_key(self, guild_id, key, value):
        request = database_service_pb2.UpdateConfigurationKeysRequest(
            guild_id=guild_id,
            key=key, value=value
        )
        response = self.grpc_client.call_method("DatabaseService", "UpdateConfigurationKeys", request)
        if response.success:
            return True
        
    def get_all_of_key(self, key_name):
        request = database_service_pb2.GetAllOfKeyRequest(key=key_name)
        response = self.grpc_client.call_method("DatabaseService", "GetAllOfKey", request)
        keys = handleProtobufUnpacking.unpack(response.keys)
        return keys