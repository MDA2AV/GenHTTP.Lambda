-- Why a version exists, and when each one was actually online.
--
-- A version recorded what the code was and nothing about why it changed. With
-- an agent writing most of them that is the half worth keeping: the code can
-- be read back, the request that produced it cannot. So a version carries the
-- request it answered and a line about what it changed, both optional, and
-- where it came from - the template it was seeded with, the API, or an agent.
--
-- Existing versions have no note, which is honest: nobody wrote one.

ALTER TABLE deployments ADD COLUMN prompt TEXT NULL;

ALTER TABLE deployments ADD COLUMN change TEXT NULL;

ALTER TABLE deployments ADD COLUMN origin TEXT NULL;

-- The history of what was online. The lambdas table knows which version is
-- live now and overwrites it on every deploy, so "what was online on Tuesday,
-- and why did it stop" had no answer. One row per stretch of being online,
-- closed when something replaces it, stops it or sweeps it.
--
-- Unlike the events it belongs to its lambda and goes with it: this is the
-- owner's history, not the platform's.

CREATE TABLE activations
(
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    lambda_id  INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    version    INTEGER NOT NULL,
    started    TEXT    NOT NULL,
    origin     TEXT    NULL,
    ended      TEXT    NULL,
    ended_by   TEXT    NULL
);

CREATE INDEX ix_activations_lambda_started ON activations (lambda_id, started);

-- What is online right now is the one stretch that can be recovered.

INSERT INTO activations (lambda_id, version, started)
SELECT id, active_version, COALESCE(deployed, modified)
FROM lambdas
WHERE active_version IS NOT NULL;
