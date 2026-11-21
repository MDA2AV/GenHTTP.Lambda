-- When somebody last asked this lambda for something.
--
-- A lambda was taken offline a day after it was deployed and removed a month
-- after it was last touched, and "touched" meant edited. Both were wrong for
-- the same reason: they measured attention from its author rather than use by
-- anybody. A deployment that worked went offline overnight, and one that was
-- finished and popular was still being counted down to deletion because
-- nobody had any reason to open the editor again.
--
-- What decides now is use, and this is where use is recorded. Requests are
-- counted in memory - per request database writes for a counter would be
-- absurd - and the maintenance pass writes the last of them here, so the
-- figure survives a restart to within one pass of it.
--
-- Existing rows are dated from their last change, which is the closest thing
-- already recorded and errs towards keeping things.

ALTER TABLE lambdas ADD COLUMN last_seen TEXT NULL;

UPDATE lambdas SET last_seen = modified;

CREATE INDEX ix_lambdas_last_seen ON lambdas (last_seen);
