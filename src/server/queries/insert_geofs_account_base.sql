INSERT INTO Account (geofs_account_id, callsign, is_online)
VALUES
--MULTI-ROW-PLACEHOLDER
ON CONFLICT (geofs_account_id)
DO UPDATE
SET callsign = EXCLUDED.callsign,
    is_online = TRUE
WHERE Account.is_online = FALSE
      OR Account.callsign <> EXCLUDED.callsign;
