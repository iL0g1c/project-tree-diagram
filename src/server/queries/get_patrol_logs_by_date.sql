WITH ForceCodeCTE AS (
    SELECT force_code
    FROM Forces
    WHERE guild_id = @guild_id
),
AccountCTE AS (
    SELECT geofs_account_id, discord_id
    FROM Account
    WHERE force_code = (SELECT force_code FROM ForceCodeCTE)
)
SELECT 
    pe.event_id,
    a.discord_id,
    pe.start_time,
    pe.end_time
FROM patrol_event pe
JOIN AccountCTE a ON pe.geofs_account_id = a.geofs_account_id
WHERE pe.start_time > @date
ORDER BY pe.end_time DESC;