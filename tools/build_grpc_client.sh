source ../src/venv/bin/activate # Enter venv

# remove previous grpc client
rm ../src/bot/proto/database_service_pb2_grpc.py
rm ../src/bot/proto/database_service_pb2.py

# generate new grpc client
python3 -m grpc_tools.protoc --proto_path="$(pwd)/../src/proto_source" --python_out=../src/bot/proto/ --grpc_python_out=../src/bot/proto/ database_service.proto

# modify client for package implementation
sed -i 's|import database_service_pb2 as database__service__pb2|from proto import database_service_pb2 as database__service__pb2|' ../src/bot/proto/database_service_pb2_grpc.py

deactivate # leave venv
