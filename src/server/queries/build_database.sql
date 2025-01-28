CREATE TABLE Account (
    geofs_account_id BIGINT PRIMARY KEY,
    discord_id BIGINT,
    is_online BOOLEAN DEFAULT FALSE,
    callsign VARCHAR(50),
    force_code VARCHAR(10)
);

CREATE SEQUENCE event_id START WITH 1 INCREMENT BY 1;

CREATE TABLE callsign_change (
    event_id BIGINT PRIMARY KEY DEFAULT nextval('event_id'),
    geofs_account_id BIGINT NOT NULL,
    old_callsign VARCHAR(50),
    new_callsign VARCHAR(50) NOT NULL,
    detected_at TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT fk_account FOREIGN KEY (geofs_account_id)
        REFERENCES Account (geofs_account_id)
);

CREATE TABLE online_status_change (
    event_id BIGINT PRIMARY KEY DEFAULT nextval('event_id'),
    geofs_account_id BIGINT NOT NULL,
    detected_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_online BOOLEAN NOT NULL,
    CONSTRAINT fk_account FOREIGN KEY (geofs_account_id)
        REFERENCES Account (geofs_account_id)
);

CREATE TABLE patrol_event (
    event_id BIGINT PRIMARY KEY DEFAULT nextval('event_id'),
    geofs_account_id BIGINT NOT NULL,
    force_code VARCHAR(10) NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT fk_account FOREIGN KEY (geofs_account_id)
        REFERENCES Account (geofs_account_id),
    CONSTRAINT fk_force FOREIGN KEY (force_code)
        REFERENCES Forces (force_code)
);

CREATE TABLE Forces (
    guild_id BIGINT PRIMARY KEY,
    force_code VARCHAR(10) UNIQUE,
    developer_role_id BIGINT,
    high_command_role_id BIGINT,
    member_role_id BIGINT,
    player_activity_channel_id BIGINT,
    patrol_log_channel_id BIGINT,
    callsign_change_channel_id BIGINT,
    callsign_code_channel_id BIGINT,
    callsign_code_loop_enabled BOOLEAN,
    callsign_format VARCHAR(50),
    kill_log_channel_id BIGINT,
);

CREATE TABLE kill_event (
    event_id BIGINT PRIMARY KEY DEFAULT nextval('event_id'),
    geofs_account_id BIGINT NOT NULL,
    guild_id BIGINT NOT NULL,
    detected_at TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT fk_account FOREIGN KEY (geofs_account_id)
        REFERENCES Account (geofs_account_id),
    CONSTRAINT fk_force FOREIGN KEY (guild_id)
        REFERENCES Forces (guild_id)
)