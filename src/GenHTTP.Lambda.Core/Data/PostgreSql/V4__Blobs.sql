-- What the store of each lambda keeps: as V29 of the SQLite set
-- (Data/Migrations), whose comment says what the columns hold.

CREATE TABLE blobs
(
    lambda_id  bigint  NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    hash       text    NOT NULL,
    size       bigint  NOT NULL,
    chars      bigint  NULL,
    git_sha1   text    NOT NULL,
    git_sha256 text    NOT NULL,
    PRIMARY KEY (lambda_id, hash)
);

CREATE INDEX ix_blobs_git_sha1 ON blobs (lambda_id, git_sha1);
