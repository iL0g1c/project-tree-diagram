# User Types
# 1 - Dev
# 2 - Secure HC
# 3 - HC
# 4 - FTO
# 5 - Member

def validateUser(user, user_type, config):
    match user_type:
        case 1:
            if user.id == config["developer_role"]:
                return True
            else:
                return False
        case 2:
            if user.id in (config["high_command_role"], config["developer_role"]):
                return True
            else:
                return False
        case 3:
            if user.id == config["high_command_role"]:
                return True
            else:
                return False
        case 4:
            if user.id in (config["fto_role"], config["nco_role"]):
                return True
            else:
                return False
        case 5:
            if user.id == config["member_role"]:
                return True
            else:
                return False
    return False