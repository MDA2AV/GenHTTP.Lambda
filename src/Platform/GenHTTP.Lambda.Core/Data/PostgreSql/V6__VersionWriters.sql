-- Who wrote a version or what a draft holds, where the platform knows it: the
-- same as V31 of the SQLite migrations, where it says more.

ALTER TABLE deployments ADD COLUMN writer smallint NULL;
ALTER TABLE deployments ADD COLUMN agent_model smallint NULL;
ALTER TABLE deployments ADD COLUMN agent_effort smallint NULL;

ALTER TABLE features ADD COLUMN writer smallint NULL;
ALTER TABLE features ADD COLUMN agent_model smallint NULL;
ALTER TABLE features ADD COLUMN agent_effort smallint NULL;
