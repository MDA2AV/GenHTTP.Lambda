-- Who wrote a version or what a draft holds, where the platform knows it,
-- beside the door it came through: the build agent of the installation, on
-- the model and effort of the job it was given (VersionWriter, AgentModel and
-- AgentEffort, each stored as its number). The server knows it from the
-- ticket it issued for the job, which every call of the job carries; an agent
-- of somebody's own leaves all three empty, and so does everything saved
-- before this migration.
--
-- A draft keeps who wrote its last save; the version it is merged into takes
-- that over, whoever merges it.

ALTER TABLE deployments ADD COLUMN writer INTEGER NULL;
ALTER TABLE deployments ADD COLUMN agent_model INTEGER NULL;
ALTER TABLE deployments ADD COLUMN agent_effort INTEGER NULL;

ALTER TABLE features ADD COLUMN writer INTEGER NULL;
ALTER TABLE features ADD COLUMN agent_model INTEGER NULL;
ALTER TABLE features ADD COLUMN agent_effort INTEGER NULL;
