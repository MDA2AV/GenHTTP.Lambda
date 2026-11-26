-- Which server of the installation follows a run, and until when its claim
-- holds: the same as V35 of the SQLite migrations, where it says more.

ALTER TABLE agent_runs ADD COLUMN follower text NULL;
ALTER TABLE agent_runs ADD COLUMN follower_until timestamptz NULL;
