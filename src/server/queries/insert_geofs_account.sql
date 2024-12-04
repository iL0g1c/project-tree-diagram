INSERT INTO Account (geofs_account_id, discord_id, is_online, callsign)
VALUES (@geofs_account_id, NULL, TRUE, @callsign)
ON CONFLICT (geofs_account_id) DO UPDATE
SET is_online = TRUE,
    callsign = EXCLUDED.callsign
WHERE Account.is_online = FALSE OR Account.callsign <> EXCLUDED.callsign;