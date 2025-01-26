WITH SelectedForce AS (
    SELECT
        force_code
    FROM
        Forces
    WHERE
        guild_id = @guild_id
)
SELECT *
FROM Account
WHERE force_code = (SELECT force_code FROM SelectedForce)