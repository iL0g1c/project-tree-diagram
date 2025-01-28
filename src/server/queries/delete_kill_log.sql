WITH ForceData AS (
    SELECT force_code
    FROM forces
    WHERE guild_id = @guild_id
),
AccountForce AS (
    SELECT a.force_code
    FROM kill_event k
    INNER JOIN account a
        ON k.geofs_account_id = a.geofs_account_id
    WHERE k.event_id = @event_id
)
DELETE FROM kill_event
WHERE event_id = @event_id
    AND EXISTS (
        SELECT 1
        FROM ForceData fd, AccountForce af
        WHERE fd.force_code = af.force_code
    );