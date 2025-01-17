# User Types
# 1 - Dev
# 2 - Secure HC
# 3 - HC
# 4 - FTO
# 5 - Member

def validateUser(user, user_type, config, is_config_change=False):
    match user_type:
        case 1:
            if config["developer_role_id"] == None:
                if not is_config_change:
                    return False, "You have not configured a developer role."
                else:
                    return True, "The developer role has been initialized for the first time."
            if any(role.id == config["developer_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
        case 2:
            if any(role.id in (config["high_command_role_id"], config["developer_role_id"]) for role in user.roles):
                return True, None
            else:
                if config["high_command_role_id"] == None:
                    return False, "You have not configured a high command role."
                if config["developer_role_id"] == None:
                    return False, "You have not configured a developer role."
        case 3:
            if config["high_command_role_id"] == None:
                return False, "You have not configured a high command role."
            if any(role.id == config["high_command_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
        case 4:
            if config["member_role_id"] == None:
                return False, "You have not configured a member role."
            if any(role.id == config["member_role_id"] for role in user.roles):
                return True, None
            else:
                return False, None
    raise ValueError("Invalid user type")