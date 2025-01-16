UPDATE Account
SET
    force_code = (
        SELECT force_code
        FROM Forces
        WHERE guild_id = @guild_id
    ),
    discord_id = @discord_id
WHERE geofs_account_id = @geofs_account_id
RETURNING geofs_account_id;