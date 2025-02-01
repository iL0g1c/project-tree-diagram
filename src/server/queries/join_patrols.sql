WITH force_data AS (
    -- Step 1: Get the force_code for the given guild_id
    SELECT force_code 
    FROM Forces 
    WHERE guild_id = @guild_id
),
validate_events AS (
    -- Step 2: Confirm both events have the correct force_code
    SELECT pe1.event_id AS first_event_id, pe2.event_id AS second_event_id, pe2.end_time
    FROM patrol_event pe1
    JOIN patrol_event pe2 ON pe1.geofs_account_id = pe2.geofs_account_id
    JOIN force_data f ON pe1.force_code = f.force_code AND pe2.force_code = f.force_code
    WHERE pe1.event_id = @first_event_id
    AND pe2.event_id = @second_event_id
    AND pe2.event_id > pe1.event_id -- Step 3: Ensure second_event_id is greater
)
UPDATE patrol_event
SET end_time = (SELECT end_time FROM validate_events WHERE second_event_id = @second_event_id)
WHERE event_id = @first_event_id;

-- Step 5: Delete the row with second_event_id
DELETE FROM patrol_event
WHERE event_id = @second_event_id;
