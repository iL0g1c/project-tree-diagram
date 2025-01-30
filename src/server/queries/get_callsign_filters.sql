WITH force_code AS (
    SELECT force_code
    FROM forces
    WHERE guild_id = @guild_id
)
SELECT callsign_filter
FROM callsign_filter
WHERE force_code IN (SELECT force_code FROM force_code)