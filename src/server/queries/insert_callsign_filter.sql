INSERT INTO callsign_filter (callsign_filter, force_code)
VALUES (
    @callsign_filter,
    (
        SELECT force_code
        FROM forces
        WHERE guild_id = @guild_id
    )
)