WITH ForceData AS (
    SELECT force_code
    FROM forces
    WHERE guild_id = @guild_id
),
AccountForce AS (
    SELECT a.force_code
    FROM patrol_event p
    INNER JOIN account a
        ON p.geofs_account_id = a.geofs_account_id
    WHERE p.event_id = @event_id
)
DELETE FROM patrol_event
WHERE event_id = @event_id
    AND EXISTS (
        SELECT 1
        FROM ForceData fd, AccountForce af
        WHERE fd.force_code = af.force_code
    );