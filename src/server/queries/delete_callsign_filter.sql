DELETE FROM callsign_filter
WHERE callsign_filter = @callsign_filter
AND force_code = (
    SELECT force_code
    FROM forces
    WHERE guild_id = @guild_id
)