UPDATE Forces
SET @key = @value
WHERE guild_id = @guild_id
RETURNING guild_id;