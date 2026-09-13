-- When the active version was put online, which is not when it was written.
--
-- The lifetime of a deployment was measured from the creation of the version it
-- runs, so a version saved yesterday and deployed today was already most of the
-- way through its day, and an older version put back online was overdue the
-- moment it started. Existing deployments are dated from the last change to the
-- lambda, which is the closest thing already recorded.

ALTER TABLE lambdas ADD COLUMN deployed TEXT NULL;

UPDATE lambdas SET deployed = modified WHERE active_version IS NOT NULL;

CREATE INDEX ix_lambdas_deployed ON lambdas (deployed);
