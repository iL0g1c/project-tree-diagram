import os
import grpc
from proto import database_service_pb2_grpc
from proto import database_service_pb2
import utils.handleProtobufUnpacking as handleProtobufUnpacking

class ConfigManager:
    def __init__(self):
        self.host = "localhost:50051"

    def get_config(self, guild_id):
        with grpc.insecure_channel(self.host) as channel:
            stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
            request = database_service_pb2.GetConfigurationKeysRequest(guild_id=guild_id)
            keys = stub.GetConfigurationKeys(request)

        keys = handleProtobufUnpacking.unpack(keys)
        return keys

    def update_key(self, guild_id, key, value):
        with grpc.insecure_channel(self.host) as channel:
            stub = database_service_pb2_grpc.DatabaseServiceStub(channel)
            request = database_service_pb2.UpdateConfigurationKeysRequest(guild_id=guild_id, key=key, value=value)
            response = stub.UpdateConfigurationKeys(request)
        if response.success:
            return True