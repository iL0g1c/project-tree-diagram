import os
import json
def load_config():
    config_path = os.path.join(os.path.dirname(__file__), "../../../data/config.json")
    if not os.path.exists(config_path):
        raise FileNotFoundError(f"Config file not found at {config_path}")
    
    with open(config_path) as f:
        return json.load(f)