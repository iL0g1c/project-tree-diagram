WITH updated_accounts AS (
    UPDATE Account
    SET is_online = FALSE
    WHERE geofs_account_id <> ALL(@account_ids)
        AND is_online = TRUE
    RETURNING geofs_account_id
)
INSERT INTO online_status_change (geofs_account_id, is_online)
SELECT geofs_account_id, FALSE
FROM updated_accounts