import os
import json

class ConfigManager:
    def __init__(self):
        config_path = os.path.join(os.path.dirname(__file__), "../../../data/config.json")
        if not os.path.exists(config_path):
            raise FileNotFoundError(f"Config file not found at {config_path}")
        
        with open(config_path) as f:
            self.config = json.load(f)

    def save_config(self):
        config_path = os.path.join(os.path.dirname(__file__), "../../../data/config.json")
        with open(config_path, "w") as f:
            json.dump(self.config, f)

    def update_key(self, key, value):
        if key in self.config:
            self.config[key] = value
            self.save_config()
            return True, None
        else:
            return False, f"The config key you tried to change doesn't exist."
        

    def create_key(self, key, value):
        if key not in self.config:
            self.config[key] = value
            self.save_config()
            return True, None
        else:
            return False, f"The config key you tried to create already exists."
        
    def destroy_key(self, key):
        if key in self.config:
            del self.config[key]
            self.save_config()
            return True, None
        else:
            return False, f"The config key you tried to delete doesn't exist."