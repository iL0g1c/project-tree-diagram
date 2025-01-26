WITH inserted AS (
    INSERT INTO patrol_event (geofs_account_id, force_code, start_time, end_time)
    VALUES (
        @geofs_account_id,
        @force_code,
        (
            SELECT detected_at
            FROM online_status_change
            WHERE geofs_account_id = @geofs_account_id
            AND is_online = TRUE
            ORDER BY detected_at DESC
            LIMIT 1
        ),
        @end_time
    )
    RETURNING start_time, event_id
)
SELECT
    inserted.start_time,
    inserted.event_id,
    (SELECT patrol_log_channel_id 
       FROM forces 
      WHERE force_code = @force_code) AS patrol_log_channel_id,
    (SELECT COUNT(*)
       FROM patrol_event
      WHERE geofs_account_id = @geofs_account_id) AS patrol_count,
    (SELECT callsign_format
       FROM forces
      WHERE force_code = @force_code) AS callsign_format
FROM inserted;
