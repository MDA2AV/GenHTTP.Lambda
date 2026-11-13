-- Secrets: the API keys, passwords and tokens a lambda needs and its code
-- must not contain.
--
-- A secret is data, not program: it belongs to the lambda, is shared by every
-- version and is left alone by deploys, rollbacks and merges. A feature works
-- on a copy of the lambda's, which is what feature_id marks - null for the
-- lambda's own.
--
-- Values are never stored as they were given. Each is sealed with AES-GCM
-- under a key made of two halves: one the installation holds outside this
-- database (LAMBDA_SECRETS_KEY, or the key file in the data directory), and
-- one each lambda holds here, secret_salt. A copy of this file alone opens
-- nothing, and a value moved to another lambda or another name no longer
-- opens either. So a backup or a move to another server takes this database
-- and the installation's key, and the secrets travel with them.

ALTER TABLE lambdas ADD COLUMN secret_salt BLOB NULL;

CREATE TABLE secrets
(
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    lambda_id   INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    feature_id  INTEGER NULL REFERENCES features (id) ON DELETE CASCADE,
    name        TEXT    NOT NULL,
    value       BLOB    NOT NULL,
    created     TEXT    NOT NULL,
    changed     TEXT    NOT NULL
);

-- one value per name, for the lambda and for each of its features
CREATE UNIQUE INDEX ix_secrets_name ON secrets (lambda_id, IFNULL(feature_id, 0), name);
