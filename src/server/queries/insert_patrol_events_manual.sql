WITH force_check AS (
    SELECT f.force_code
    FROM forces f
    JOIN account a ON f.force_code = a.force_code
    WHERE a.discord_id = @discord_id
),
account_force_code AS (
    SELECT force_code, geofs_account_id
    FROM account
    WHERE discord_id = @discord_id
),
inserted AS (
    INSERT INTO patrol_event (geofs_account_id, start_time, end_time, force_code)
    SELECT
        afc.geofs_account_id,
        (
            SELECT detected_at
            FROM online_status_change
            WHERE geofs_account_id = afc.geofs_account_id
            AND is_online = TRUE
            ORDER BY detected_at DESC
            LIMIT 1
        ),
        @end_time,
        afc.force_code
    FROM account_force_code afc
    WHERE afc.force_code = (SELECT force_code FROM force_check)
    RETURNING event_id
)
SELECT
    inserted.event_id,
    (SELECT patrol_log_channel_id 
       FROM forces 
      WHERE force_code = (SELECT force_code FROM force_check)) AS patrol_log_channel_id,
    (SELECT COUNT(*)
       FROM patrol_event
      WHERE geofs_account_id = @geofs_account_id) AS patrol_count
FROM inserted;
