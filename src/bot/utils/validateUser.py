# User Types
# 1 - Dev
# 2 - Secure HC
# 3 - HC
# 4 - FTO
# 5 - Member

def validateUser(user, user_type, config):
    match user_type:
        case 1:
            if "developer_role_id" not in config:
                return False, "You have not configured a developer role."
            if any(role.id == config["developer_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
        case 2:
            if "high_command_role_id" not in config:
                return False, "You have not configured a high command role."
            if "developer_role" not in config:
                return False, "You have not configured a developer role."
            if any(role.id in (config["high_command_role_id"], config["developer_role_id"]) for role in user.roles):
                return True, None
            else:
                return False, None
        case 3:
            if "high_command_role_id" not in config:
                return False, "You have not configured a high command role."
            if any(role.id == config["high_command_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
        case 4:
            if "member_role_id" not in config:
                return False, "You have not configured a member role."
            if any(role.id == config["member_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
    raise ValueError("Invalid user type")