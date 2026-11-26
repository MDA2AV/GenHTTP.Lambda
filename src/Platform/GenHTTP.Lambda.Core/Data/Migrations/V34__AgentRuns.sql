-- The runs of the installation's agent, kept by the platform rather than by
-- the agent system: a page closed and opened again, or a platform restarted,
-- finds a run where it was, and what a page says came of it is read from
-- the platform's records. A run is one message - a build on /build, or a
-- message about an app from its owner (kind 0 and 1) - with the options of
-- the agent integration it runs on as a JSON object, its state (Starting 0,
-- Queued 1, Working 2, Waiting 3, Ended 4) and how it ended (AgentEnding of
-- the integration contract), each stored as its number.
--
-- The run's credential is kept as its SHA-256, as the tickets of V32 were:
-- what the table holds cannot be presented as one. Those tickets are still
-- read for the day they last, then removed.
--
-- Its events are what the platform saw it do at its doors, what the agent
-- said and did beyond the platform, what it changed - which is what undoing
-- it reads - and the files it handed back. agent_files holds what travelled
-- with a run either way: the attachments of a message (direction 0) and the
-- files the agent handed back (direction 1); their content is a file of the
-- files integration's. Everything goes with its lambda, and after a while
-- with its run.
--
-- What the agent wrote is marked with its run and its options, beside the
-- model and effort of V31, which stay for what was written before.

CREATE TABLE agent_runs
(
    id             TEXT    NOT NULL PRIMARY KEY,
    kind           INTEGER NOT NULL,
    lambda_id      INTEGER NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    state          INTEGER NOT NULL,
    ending         INTEGER NULL,
    prompt         TEXT    NOT NULL,
    options        TEXT    NOT NULL,
    guarded        INTEGER NOT NULL DEFAULT 0,
    language       TEXT    NULL,
    view           INTEGER NOT NULL,
    deploy         INTEGER NOT NULL DEFAULT 0,
    feature        TEXT    NULL,
    before_version INTEGER NULL,
    continues      TEXT    NULL,
    continuation   INTEGER NULL,
    reference      TEXT    NULL,
    position       INTEGER NOT NULL DEFAULT 0,
    wait_reason    INTEGER NULL,
    wait_until     TEXT    NULL,
    time_limit     INTEGER NULL,
    step_limit     INTEGER NULL,
    message        TEXT    NULL,
    detail         TEXT    NULL,
    input_tokens   INTEGER NULL,
    output_tokens  INTEGER NULL,
    cost           REAL    NULL,
    currency       TEXT    NULL,
    followed       INTEGER NOT NULL DEFAULT 0,
    ticket         TEXT    NOT NULL,
    created        TEXT    NOT NULL,
    started        TEXT    NULL,
    ended          TEXT    NULL
);

CREATE UNIQUE INDEX ix_agent_runs_ticket ON agent_runs (ticket);
CREATE INDEX ix_agent_runs_lambda ON agent_runs (lambda_id, created);
CREATE INDEX ix_agent_runs_state ON agent_runs (state);

CREATE TABLE agent_run_events
(
    run_id TEXT    NOT NULL REFERENCES agent_runs (id) ON DELETE CASCADE,
    number INTEGER NOT NULL,
    at     TEXT    NOT NULL,
    kind   INTEGER NOT NULL,
    key    TEXT    NULL,
    data   TEXT    NOT NULL,
    PRIMARY KEY (run_id, number)
);

CREATE TABLE agent_files
(
    id          TEXT    NOT NULL PRIMARY KEY,
    direction   INTEGER NOT NULL,
    run_id      TEXT    NULL REFERENCES agent_runs (id) ON DELETE CASCADE,
    lambda_id   INTEGER NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    name        TEXT    NOT NULL,
    media_type  TEXT    NOT NULL,
    size        INTEGER NOT NULL,
    sha256      TEXT    NOT NULL,
    description TEXT    NULL,
    created     TEXT    NOT NULL
);

CREATE INDEX ix_agent_files_run ON agent_files (run_id);
CREATE INDEX ix_agent_files_lambda ON agent_files (lambda_id);

ALTER TABLE deployments ADD COLUMN agent_run TEXT NULL;
ALTER TABLE deployments ADD COLUMN agent_options TEXT NULL;

ALTER TABLE features ADD COLUMN agent_run TEXT NULL;
ALTER TABLE features ADD COLUMN agent_options TEXT NULL;
