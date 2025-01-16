SELECT geofs_account_id, callsign
FROM Account
WHERE geofs_account_id = ANY(@account_ids);