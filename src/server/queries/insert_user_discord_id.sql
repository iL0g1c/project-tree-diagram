UPDATE Account
SET discord_id = @discord_id
WHERE geofs_account_id = @geofs_account_id;