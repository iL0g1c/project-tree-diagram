# User Types
# 1 - Dev
# 2 - Secure HC
# 3 - HC
# 4 - FTO
# 5 - Member

def validateUser(user, user_type, config):
    match user_type:
        case 1:
            if "developer_role" not in config:
                config["developer_role"] = None
                return False, "You have not configured a developer role."
            if user.id == config["developer_role"]:
                return True, None
            else:
                return False, None
        case 2:
            if "high_command_role" not in config:
                config["high_command_role"] = None
                return False, "You have not configured a high command role."
            if "developer_role" not in config:
                config["developer_role"] = None
                return False, "You have not configured a developer role."
            if user.id in (config["high_command_role"], config["developer_role"]):
                return True, None
            else:
                return False, None
        case 3:
            if "high_command_role" not in config:
                config["high_command_role"] = None
                return False, "You have not configured a high command role."
            if user.id == config["high_command_role"]:
                return True, None
            else:
                return False, None
        case 4:
            if "fto_role" not in config:
                config["fto_role"] = None
                return False, "You have not configured a FTO role."
            if user.id in (config["fto_role"], config["nco_role"]):
                return True, None
            else:
                return False, None
        case 5:
            if "member_role" not in config:
                config["member_role"] = None
                return False, "You have not configured a member role."
            if user.id == config["member_role"]:
                return True, None
            else:
                return False, None
    raise ValueError("Invalid user type")