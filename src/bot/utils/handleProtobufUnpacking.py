from google.protobuf.any_pb2 import Any
from google.protobuf.wrappers_pb2 import StringValue, Int64Value, BoolValue
from google.protobuf.timestamp_pb2 import Timestamp
from google.protobuf.empty_pb2 import Empty
def unpack(keys):
    unpacked_keys = {}
    for key, any_value in keys.items():
        if any_value.Is(Empty.DESCRIPTOR):
            unpacked_keys[key] = None
        elif any_value.Is(StringValue.DESCRIPTOR):
            string_value = StringValue()
            any_value.Unpack(string_value)
            unpacked_keys[key] = string_value.value
        elif any_value.Is(Int64Value.DESCRIPTOR):
            int64_value = Int64Value()
            any_value.Unpack(int64_value)
            unpacked_keys[key] = int64_value.value
        elif any_value.Is(BoolValue.DESCRIPTOR):
            bool_value = BoolValue()
            any_value.Unpack(bool_value)
            unpacked_keys[key] = bool_value.value
        else:
            continue
    return unpacked_keys