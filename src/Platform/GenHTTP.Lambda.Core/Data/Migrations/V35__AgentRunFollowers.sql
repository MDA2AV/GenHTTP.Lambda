-- Which server of the installation follows a run, and until when its claim
-- holds: several servers may run one installation, and exactly one of them
-- takes what the agent system tells of a run into the records. A server
-- renews its claim while it follows; one that went away is taken over once
-- its claim has lapsed. A run nobody claimed - one of a release before - is
-- free to take.

ALTER TABLE agent_runs ADD COLUMN follower TEXT NULL;
ALTER TABLE agent_runs ADD COLUMN follower_until TEXT NULL;
