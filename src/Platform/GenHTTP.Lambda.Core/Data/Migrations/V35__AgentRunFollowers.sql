-- Which server of the installation follows a run, and until when its claim
-- holds: several servers may run one installation, and exactly one of them
-- takes what the agent system tells of a run into the records. A server
-- renews its claim while it follows; one that went away is taken over once
-- its claim has lapsed. A run nobody claimed - one of a release before - is
-- free to take.

ALTER TABLE agent_runs ADD COLUMN follower TEXT NULL;
ALTER TABLE agent_runs ADD COLUMN follower_until TEXT NULL;

-- One run of a lambda at a time, held by the records rather than by a check
-- before the insert: two servers, or a message sent twice, cannot start two.
CREATE UNIQUE INDEX agent_runs_one_at_a_time ON agent_runs (lambda_id) WHERE state <> 4 AND lambda_id IS NOT NULL;
