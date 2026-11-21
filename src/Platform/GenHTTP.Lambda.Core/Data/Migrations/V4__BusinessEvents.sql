-- What has happened on the platform, rather than what is true right now.
--
-- The tables beside this one hold state: which lambdas exist, which version
-- each runs. That answers "how many are deployed" and cannot answer "how many
-- were deployed last Tuesday", because a redeploy overwrites the timestamp and
-- a delete takes the row with it. This is the append-only half.
--
-- There is deliberately no foreign key to lambdas. An event has to outlive the
-- thing it happened to, and a deletion is the one event that matters most.

CREATE TABLE events
(
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    kind       TEXT    NOT NULL,
    lambda_id  INTEGER NULL,
    public_key TEXT    NULL,
    occurred   TEXT    NOT NULL
);

CREATE INDEX ix_events_occurred ON events (occurred);

CREATE INDEX ix_events_kind_occurred ON events (kind, occurred);

-- The history that is already recorded, recovered rather than started from
-- zero: every lambda knows when it was made, every stored version knows when
-- it was written, and a lambda that is online knows when it went online. Only
-- redeployments and deletions are unrecoverable, and those begin from now.
--
-- Examples are left out throughout. The installation seeds its own on every
-- boot, and counting those as activity would drown the figures they are meant
-- to show.

INSERT INTO events (kind, lambda_id, public_key, occurred)
SELECT 'created', id, public_key, created
FROM lambdas
WHERE COALESCE(is_example, 0) = 0;

INSERT INTO events (kind, lambda_id, public_key, occurred)
SELECT 'saved', d.lambda_id, l.public_key, d.created
FROM deployments d
JOIN lambdas l ON l.id = d.lambda_id
WHERE COALESCE(l.is_example, 0) = 0;

INSERT INTO events (kind, lambda_id, public_key, occurred)
SELECT 'deployed', id, public_key, deployed
FROM lambdas
WHERE deployed IS NOT NULL AND COALESCE(is_example, 0) = 0;
