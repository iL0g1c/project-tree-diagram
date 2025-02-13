SELECT 
    pe.event_id,
    a.discord_id,
    pe.start_time,
    pe.end_time
FROM patrol_event pe
JOIN Account a ON pe.geofs_account_id = a.geofs_account_id
WHERE a.force_code = (SELECT force_code FROM Forces WHERE guild_id = @guild_id)
AND pe.start_time > @date
AND a.discord_id = @discord_id
ORDER BY pe.end_time DESC;