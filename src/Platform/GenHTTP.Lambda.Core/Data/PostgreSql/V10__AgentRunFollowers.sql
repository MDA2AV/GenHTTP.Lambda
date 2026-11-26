-- Which server of the installation follows a run, and until when its claim
-- holds: the same as V35 of the SQLite migrations, where it says more.

ALTER TABLE agent_runs ADD COLUMN follower text NULL;
ALTER TABLE agent_runs ADD COLUMN follower_until timestamptz NULL;

-- One run of a lambda at a time, held by the records.
CREATE UNIQUE INDEX agent_runs_one_at_a_time ON agent_runs (lambda_id) WHERE state <> 4 AND lambda_id IS NOT NULL;
