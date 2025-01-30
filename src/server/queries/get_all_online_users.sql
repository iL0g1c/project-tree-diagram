SELECT
    account.discord_id,
    account.callsign,
    ARRAY_AGG(callsign_filter.callsign_filter) AS callsign_filter
FROM account
JOIN forces ON account.force_code = forces.force_code
LEFT JOIN callsign_filter ON account.force_code= callsign_filter.force_code
WHERE forces.guild_id = @guild_id
    AND account.is_online = TRUE
GROUP BY account.discord_id, account.callsign;