INSERT INTO Account (geofs_account_id, callsign)
VALUES
--MULTI-ROW-PLACEHOLDER
ON CONFLICT (geofs_account_id)
DO UPDATE
    SET callsign = EXCLUDED.callsign
    WHERE Account.callsign <> EXCLUDED.callsign;
