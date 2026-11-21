CREATE TABLE lambdas
(
    id             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    public_key     TEXT    NOT NULL,
    private_key    TEXT    NOT NULL,
    tier           TEXT    NOT NULL,
    active_version INTEGER NULL,
    created        TEXT    NOT NULL,
    modified       TEXT    NOT NULL
);

CREATE UNIQUE INDEX ux_lambdas_public_key ON lambdas (public_key);

CREATE UNIQUE INDEX ux_lambdas_private_key ON lambdas (private_key);

CREATE INDEX ix_lambdas_modified ON lambdas (modified);

CREATE TABLE deployments
(
    id        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    lambda_id INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    version   INTEGER NOT NULL,
    created   TEXT    NOT NULL
);

CREATE UNIQUE INDEX ux_deployments_lambda_version ON deployments (lambda_id, version);
