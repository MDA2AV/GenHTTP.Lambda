-- Which kinds of data a lambda keeps.
--
-- A version is the program: its code and its assets, saved, deployed and
-- rolled back together. Data is what the program keeps while it runs, and it
-- belongs to the lambda rather than to any version - every version reads and
-- writes the same. The workspace is the only kind so far; a database and
-- secrets are meant to follow, and each is something the owner switches on.
--
-- One row per lambda and kind, and only once the owner has chosen: a missing
-- row is the default of that kind, which for the workspace is on. So a new
-- kind needs no migration of its own to exist, only one to be remembered.

CREATE TABLE data_stores
(
    lambda_id  INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    kind       TEXT    NOT NULL,
    enabled    INTEGER NOT NULL,
    changed    TEXT    NOT NULL,
    PRIMARY KEY (lambda_id, kind)
);
