-- Features: a change being worked on beside the lambda, without touching it.
--
-- Versions are immutable, which is what makes them worth having - every one
-- can be read back and put online again exactly as it was. What that leaves
-- out is somewhere to work: an agent fixing something saved a version for
-- every attempt, and every attempt it deployed replaced what visitors got.
--
-- A feature branches off a version and is edited in place for as long as it
-- takes. It has its own copy of the lambda's data to try things on, and can
-- be put online at an address of its own, /features/{key}/, which leaves the
-- lambda alone. Once it does what was asked it is merged: its files become the
-- next version, and the feature goes. Only a feature based on the newest
-- version can be merged, so a feature cannot quietly undo a version saved
-- after it branched off; bringing that version's changes in is up to whoever
-- works on the feature, who then says so by moving its base.
--
-- The files, the copy of the data and what the preview serves live on disk
-- beside the versions, like the versions themselves.

CREATE TABLE features
(
    id             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    lambda_id      INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    key            TEXT    NOT NULL,
    name           TEXT    NOT NULL,
    specification  TEXT    NULL,
    change         TEXT    NULL,
    base_version   INTEGER NOT NULL,
    origin         TEXT    NULL,
    created        TEXT    NOT NULL,
    modified       TEXT    NOT NULL,
    preview        INTEGER NOT NULL DEFAULT 0,
    previewed      TEXT    NULL,
    revision       INTEGER NOT NULL DEFAULT 1,
    preview_of     INTEGER NULL
);

CREATE UNIQUE INDEX ix_features_key ON features (key);

CREATE INDEX ix_features_lambda ON features (lambda_id);
