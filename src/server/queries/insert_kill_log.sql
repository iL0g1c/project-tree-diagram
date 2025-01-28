WITH account_info AS (
    SELECT a.geofs_account_id, f.force_code
    FROM Account a
    JOIN Forces f ON f.guild_id = @guild_id
    WHERE a.discord_id = @discord_id
),
inserted AS (
    INSERT INTO kill_event (geofs_account_id, guild_id, force_code, detected_at)
    SELECT geofs_account_id, @guild_id, force_code, NOW()
    FROM account_info
    RETURNING event_id, geofs_account_id, guild_id, force_code, detected_at
)
SELECT
    inserted.event_id, 
    inserted.detected_at,
    (
      SELECT COUNT(*)
      FROM kill_event
      WHERE kill_event.geofs_account_id = inserted.geofs_account_id
        AND kill_event.guild_id = inserted.guild_id
        AND kill_event.force_code = inserted.force_code
    ) AS kill_count
FROM inserted;