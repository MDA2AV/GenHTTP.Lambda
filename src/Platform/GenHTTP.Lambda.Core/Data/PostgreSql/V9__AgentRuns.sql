-- The runs of the installation's agent, their events and the files that
-- travelled with them: the same as V34 of the SQLite migrations, where it
-- says more.

CREATE TABLE agent_runs
(
    id             text             NOT NULL PRIMARY KEY,
    kind           smallint         NOT NULL,
    lambda_id      bigint           NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    state          smallint         NOT NULL,
    ending         smallint         NULL,
    prompt         text             NOT NULL,
    options        text             NOT NULL,
    guarded        boolean          NOT NULL DEFAULT false,
    language       text             NULL,
    view           smallint         NOT NULL,
    deploy         boolean          NOT NULL DEFAULT false,
    feature        text             NULL,
    before_version integer          NULL,
    continues      text             NULL,
    continuation   smallint         NULL,
    reference      text             NULL,
    position       integer          NOT NULL DEFAULT 0,
    wait_reason    smallint         NULL,
    wait_until     timestamptz      NULL,
    time_limit     integer          NULL,
    step_limit     integer          NULL,
    message        text             NULL,
    detail         text             NULL,
    input_tokens   bigint           NULL,
    output_tokens  bigint           NULL,
    cost           double precision NULL,
    currency       text             NULL,
    followed       bigint           NOT NULL DEFAULT 0,
    ticket         text             NOT NULL,
    created        timestamptz      NOT NULL,
    started        timestamptz      NULL,
    ended          timestamptz      NULL
);

CREATE UNIQUE INDEX ix_agent_runs_ticket ON agent_runs (ticket);
CREATE INDEX ix_agent_runs_lambda ON agent_runs (lambda_id, created);
CREATE INDEX ix_agent_runs_state ON agent_runs (state);

CREATE TABLE agent_run_events
(
    run_id text        NOT NULL REFERENCES agent_runs (id) ON DELETE CASCADE,
    number integer     NOT NULL,
    at     timestamptz NOT NULL,
    kind   smallint    NOT NULL,
    key    text        NULL,
    data   text        NOT NULL,
    PRIMARY KEY (run_id, number)
);

CREATE TABLE agent_files
(
    id          text        NOT NULL PRIMARY KEY,
    direction   smallint    NOT NULL,
    run_id      text        NULL REFERENCES agent_runs (id) ON DELETE CASCADE,
    lambda_id   bigint      NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    name        text        NOT NULL,
    media_type  text        NOT NULL,
    size        bigint      NOT NULL,
    sha256      text        NOT NULL,
    description text        NULL,
    created     timestamptz NOT NULL
);

CREATE INDEX ix_agent_files_run ON agent_files (run_id);
CREATE INDEX ix_agent_files_lambda ON agent_files (lambda_id);

ALTER TABLE deployments ADD COLUMN agent_run text NULL;
ALTER TABLE deployments ADD COLUMN agent_options text NULL;

ALTER TABLE features ADD COLUMN agent_run text NULL;
ALTER TABLE features ADD COLUMN agent_options text NULL;
