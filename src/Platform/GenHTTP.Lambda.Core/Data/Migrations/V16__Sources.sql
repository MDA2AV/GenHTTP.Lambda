-- The lambdas whose owners published their source code.
--
-- Publishing is opt in, one row per lambda, and the row belongs to the lambda:
-- it follows it to a new key and goes when it is deleted. What is published is
-- the program - every version's code, assets, documentation and tests, as the
-- export packs them - and never the data, which no row here points at.
--
-- A row outlives being taken down, marked unpublished, so that switching it off
-- and on again does not throw away the stars people gave it. Nothing but the
-- owner's choice and the count is kept here: the source itself is read from the
-- versions, and packed once per version into a cache on disk.

CREATE TABLE sources
(
    lambda_id     INTEGER NOT NULL PRIMARY KEY REFERENCES lambdas (id) ON DELETE CASCADE,
    published     INTEGER NOT NULL,
    license       TEXT    NOT NULL,
    author        TEXT    NULL,
    stars         INTEGER NOT NULL DEFAULT 0,
    about         TEXT    NULL,
    about_version INTEGER NULL,
    published_at  TEXT    NOT NULL,
    updated       TEXT    NOT NULL
);

CREATE INDEX ix_sources_published ON sources (published);
