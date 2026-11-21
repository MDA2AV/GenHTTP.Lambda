-- What the store of each lambda keeps: the content of the files of its
-- versions, its features and their previews, once each, below
-- code/{lambda}/blobs/ and named by the SHA-256 of what it holds (hash).
--
-- A row says what the content is without reading it: how large it is, its
-- length in characters where it is UTF-8 text without a NUL character
-- (chars, null for anything else), and the ids git gives it - the SHA-1 it
-- names a blob by today, and the SHA-256 of its object format to come - so
-- a push that brings a file the lambda keeps already is not read, and a
-- commit is laid out without hashing what it holds. Which content a version
-- holds is said by its manifest on disk; a row of content no manifest names
-- any more goes with the content.

CREATE TABLE blobs
(
    lambda_id  INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    hash       TEXT    NOT NULL,
    size       INTEGER NOT NULL,
    chars      INTEGER NULL,
    git_sha1   TEXT    NOT NULL,
    git_sha256 TEXT    NOT NULL,
    PRIMARY KEY (lambda_id, hash)
);

CREATE INDEX ix_blobs_git_sha1 ON blobs (lambda_id, git_sha1);
