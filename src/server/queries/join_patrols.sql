WITH force_code_cte AS (
	SELECT force_code
	FROM forces
	WHERE guild_id = @guild_id
),
excluded_patrols AS (
	SELECT event_id
	FROM patrol_event
	WHERE event_id > @first_event_id AND event_id < @second_event_id
    AND force_code = (SELECT force_code FROM force_code_cte)
),
valid_patrols AS (
	SELECT *
	FROM patrol_event
	WHERE event_id IN (@first_event_id, @second_event_id)
		AND force_code = (SELECT * FROM force_code_cte)
		AND NOT EXISTS (
			SELECT 1
			FROM excluded_patrols
	)
)
UPDATE patrol_event
SET end_time = (
	SELECT end_time
	FROM valid_patrols
	WHERE event_id = @second_event_id
)
WHERE event_id = @first_event_id
AND EXISTS (
	SELECT 1
	FROM valid_patrols
);

WITH force_code_cte AS (
	SELECT force_code
	FROM forces
	WHERE guild_id = @guild_id
),
excluded_patrols AS (
	SELECT event_id
	FROM patrol_event
	WHERE event_id > @first_event_id AND event_id < @second_event_id
    AND force_code = (SELECT force_code FROM force_code_cte)
),
valid_patrols AS (
	SELECT *
	FROM patrol_event
	WHERE event_id IN (@first_event_id, @second_event_id)
		AND force_code = (SELECT * FROM force_code_cte)
		AND NOT EXISTS (
			SELECT 1
			FROM excluded_patrols
	)
)
DELETE FROM patrol_event
WHERE event_id = @second_event_id
AND EXISTS (
	SELECT 1
	FROM valid_patrols
);