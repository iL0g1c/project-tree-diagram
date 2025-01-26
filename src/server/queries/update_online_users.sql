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