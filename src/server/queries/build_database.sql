CREATE TABLE Account (
    geofs_account_id BIGINT PRIMARY KEY,
    discord_id BIGINT,
    is_online BOOLEAN,
    callsign VARCHAR(50)
);

CREATE TABLE callsign_change (
    event_id BIGINT PRIMARY KEY DEFAULT nextval('event_id'),
    geofs_account_id BIGINT NOT NULL,
    old_callsign VARCHAR(50),
    new_callsign VARCHAR(50) NOT NULL,
    detected_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_account FOREIGN KEY (geofs_account_id)
    REFERENCES Account (geofs_account_id)
);