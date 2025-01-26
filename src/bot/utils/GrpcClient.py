import grpc
from proto import database_service_pb2_grpc

class GrpcClient:
    _instance = None

    def __new__(cls, host: str = "localhost:50051"):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance._initialize(host)
        return cls._instance
    
    def _initialize(self, host):
        self.host = host
        self.channel = grpc.insecure_channel(self.host)
        self.stubs = {
            "DatabaseService": database_service_pb2_grpc.DatabaseServiceStub(self.channel)
        }

    def get_stub(self, service_name: str):
        return self.stubs.get(service_name)
    
    def call_method(self, service_name: str, method_name: str, request):
        stub = self.get_stub(service_name)
        if not stub:
            raise ValueError(f"Service {service_name} not found")

        method = getattr(stub, method_name, None)
        if not method:
            raise ValueError(f"Method {method_name} not found in service {service_name}")
        
        return method(request)

    def close(self):
        self.channel.close()