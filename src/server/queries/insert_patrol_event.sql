WITH OnlineStatusChange AS (
    SELECT *
    FROM online_status_change
    WHERE is_online = true
    ORDER BY detected_at DESC
    LIMIT 1
)
INSERT INTO patrol_event (geofs_account_id, force_code, start_time, end_time)
VALUES (@geofs_account_id, @force_code, (SELECT detected_at FROM OnlineStatusChange), @end_time)
RETURNING event_id, start_time;