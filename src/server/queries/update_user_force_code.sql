UPDATE Account
SET force_code = @force_code, discord_id = @discord_id
WHERE geofs_account_id = @geofs_account_id
RETURNING geofs_account_id;