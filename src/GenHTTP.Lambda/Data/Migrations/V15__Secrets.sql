-- Secrets: API tokens, credentials and the like, a kind of data of a lambda.
--
-- Like every kind of data they belong to the lambda, not to a version, and a
-- feature works on a copy of them. That copy is why a row may name a feature:
-- the lambda's own secrets have no feature, and each feature has the rows it
-- was given when it began.
--
-- A value is never stored as it was written. It is encrypted (AES-256-GCM)
-- under a key made of two things that are kept apart: a secret of the
-- installation, which is not in this database, and the salt of the lambda
-- below, which is. A copy of this file alone reads nothing; the two together
-- - which is what moving to another server or restoring a backup takes - read
-- everything. The name of a secret is not secret, and is authenticated with
-- the value, so a value cannot be moved under another name.
--
-- Rows go with their lambda, and with their feature.

ALTER TABLE lambdas ADD COLUMN secret_salt BLOB NULL;

CREATE TABLE secrets
(
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    lambda_id   INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    feature_id  INTEGER NULL REFERENCES features (id) ON DELETE CASCADE,
    name        TEXT    NOT NULL,
    value       BLOB    NOT NULL,
    created     TEXT    NOT NULL,
    updated     TEXT    NOT NULL
);

-- one of a name for the lambda, and one for each of its features; a null does
-- not equal a null to SQLite, so the lambda's own rows are indexed on their own
CREATE UNIQUE INDEX ix_secrets_lambda ON secrets (lambda_id, name) WHERE feature_id IS NULL;

CREATE UNIQUE INDEX ix_secrets_feature ON secrets (feature_id, name) WHERE feature_id IS NOT NULL;
