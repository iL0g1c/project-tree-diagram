-- File: build_database.sql

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
    end_time TIMESTAMP WITH TIME ZONE    NOT NULL,
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
    callsign_format VARCHAR(50)
);

-- File: check_for_existing_user.sql

SELECT geofs_account_id, callsign
FROM Account
WHERE geofs_account_id = ANY(@account_ids);

-- File: get_all_of_key.sql

SELECT guild_id, @key
FROM Forces;

-- File: get_configuration_keys.sql

SELECT *
FROM Forces
WHERE guild_id = @guild_id

-- File: get_force_users.sql

WITH SelectedForce AS (
    SELECT
        force_code
    FROM
        Forces
    WHERE
        guild_id = @guild_id
)
SELECT *
FROM Account
WHERE force_code = (SELECT force_code FROM SelectedForce)

-- File: get_user_callsign_changes.sql

SELECT
    detected_at,
    old_callsign,
    new_callsign
FROM
    callsign_change
WHERE
    geofs_account_id = @user_id
ORDER BY
    detected_at DESC;

-- File: insert_callsign_change_event.sql

INSERT INTO callsign_change (geofs_account_id, old_callsign, new_callsign, detected_at)
VALUES


-- File: insert_geofs_account_base.sql

INSERT INTO Account (geofs_account_id, callsign)
VALUES
--MULTI-ROW-PLACEHOLDER
ON CONFLICT (geofs_account_id)
DO UPDATE
    SET callsign = EXCLUDED.callsign
    WHERE Account.callsign <> EXCLUDED.callsign;


-- File: insert_new_guild.sql

INSERT INTO Forces (guild_id, force_code, developer_role_id, high_command_role_id, member_role_id, player_activity_channel_id, patrol_log_channel_id)
VALUES (@guild_id, null, null, null, null, null, null)

-- File: insert_user_discord_id.sql

UPDATE Account
SET discord_id = @discord_id
WHERE geofs_account_id = @geofs_account_id;

-- File: update_configuration_keys.sql

UPDATE Forces
SET @key = @value
WHERE guild_id = @guild_id
RETURNING guild_id;

-- File: update_offline_users.sql

WITH updated_accounts AS (
    UPDATE Account
    SET is_online = FALSE
    WHERE geofs_account_id <> ALL(@account_ids)
        AND is_online = TRUE
    RETURNING geofs_account_id, discord_id, force_code, callsign
),
inserted_events AS (
    INSERT INTO online_status_change (geofs_account_id, detected_at, is_online)
    SELECT geofs_account_id, @detected_at, FALSE
        FROM updated_accounts
    RETURNING geofs_account_id, is_online
)
SELECT
    ua.geofs_account_id,
    ua.discord_id,
    ua.force_code,
    ua.callsign,
    ie.is_online
FROM updated_accounts ua
JOIN inserted_events ie
    USING (geofs_account_id);

-- File: update_online_users.sql

WITH updated_accounts AS (
    UPDATE Account
    SET is_online = TRUE
    WHERE geofs_account_id = ANY(@account_ids)
        AND is_online = FALSE
    RETURNING geofs_account_id, discord_id, force_code, callsign
),
inserted_events AS (
    INSERT INTO online_status_change (geofs_account_id, detected_at, is_online)
    SELECT geofs_account_id, @detected_at, TRUE
        FROM updated_accounts
    RETURNING geofs_account_id, is_online
)
SELECT
    ua.geofs_account_id,
    ua.discord_id,
    ua.force_code,
    ua.callsign,
    ie.is_online
FROM updated_accounts ua
JOIN inserted_events ie
    USING (geofs_account_id);

-- File: update_patrol_events.sql

WITH inserted AS (
    INSERT INTO patrol_event (geofs_account_id, force_code, start_time, end_time)
    VALUES (
        @geofs_account_id,
        @force_code,
        (
            SELECT detected_at
            FROM online_status_change
            WHERE geofs_account_id = @geofs_account_id
            AND is_online = TRUE
            ORDER BY detected_at DESC
            LIMIT 1
        ),
        @end_time
    )
    RETURNING start_time, event_id
)
SELECT
    inserted.start_time,
    inserted.event_id,
    (SELECT patrol_log_channel_id 
       FROM forces 
      WHERE force_code = @force_code) AS patrol_log_channel_id,
    (SELECT COUNT(*)
       FROM patrol_event
      WHERE geofs_account_id = @geofs_account_id) AS patrol_count,
    (SELECT callsign_format
       FROM forces
      WHERE force_code = @force_code) AS callsign_format
FROM inserted;


-- File: update_user_force_code.sql

UPDATE Account
SET
    force_code = (
        SELECT force_code
        FROM Forces
        WHERE guild_id = @guild_id
    ),
    discord_id = @discord_id
WHERE geofs_account_id = @geofs_account_id
RETURNING geofs_account_id;

