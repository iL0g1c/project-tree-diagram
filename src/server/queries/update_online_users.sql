WITH updated_accounts AS (
    UPDATE Account
    SET is_online = TRUE
    WHERE geofs_account_id = ANY(@account_ids)
        AND is_online = FALSE
    RETURNING geofs_account_id
)
INSERT INTO online_status_change (geofs_account_id, is_online)
SELECT geofs_account_id, TRUE
FROM updated_accounts