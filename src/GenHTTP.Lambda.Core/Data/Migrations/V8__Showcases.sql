-- What the owner of a lambda says about it, to people who have never seen it.
--
-- A lambda is a link, and a link says nothing about what is behind it. The
-- showcase page lists the lambdas whose owners chose to be listed, each with
-- a title, a few sentences and a picture - so it is opt in, one entry per
-- lambda, and it belongs to the lambda: it follows it to a new address and
-- goes when it is deleted.
--
-- The picture is kept in the row rather than on disk. It is small and bounded,
-- it is one per lambda, and a backup of the database is then a backup of the
-- whole showcase rather than half of it. The listing never selects it.

CREATE TABLE showcases
(
    lambda_id    INTEGER NOT NULL PRIMARY KEY REFERENCES lambdas (id) ON DELETE CASCADE,
    title        TEXT    NOT NULL,
    description  TEXT    NOT NULL,
    image        BLOB    NOT NULL,
    image_type   TEXT    NOT NULL,
    created      TEXT    NOT NULL,
    updated      TEXT    NOT NULL
);
