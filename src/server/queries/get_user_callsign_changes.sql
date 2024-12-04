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