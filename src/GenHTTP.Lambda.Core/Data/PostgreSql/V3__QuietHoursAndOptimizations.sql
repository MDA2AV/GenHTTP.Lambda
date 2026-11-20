-- How busy the installation and each lambda are by the hour of the day, and
-- what keeping their databases in shape last did: as V28 of the SQLite set
-- (Data/Migrations), whose comment says what the columns hold.

CREATE TABLE quiet_hours
(
    lambda_id bigint      NOT NULL PRIMARY KEY,
    hours     text        NOT NULL,
    covered   integer     NOT NULL DEFAULT 0,
    since     timestamptz NOT NULL,
    updated   timestamptz NOT NULL
);

CREATE TABLE database_optimizations
(
    lambda_id   bigint      NOT NULL PRIMARY KEY,
    optimized   timestamptz NOT NULL,
    millis      bigint      NOT NULL,
    size_before bigint      NOT NULL,
    size_after  bigint      NOT NULL,
    steps       text        NOT NULL,
    skipped     text        NULL,
    failure     text        NULL
);
