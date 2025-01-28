SELECT
    account.discord_id,
    account.callsign,
    forces.callsign_format
FROM account
JOIN forces ON account.force_code = forces.force_code
WHERE forces.guild_id = @guild_id
    AND account.is_online = TRUE;