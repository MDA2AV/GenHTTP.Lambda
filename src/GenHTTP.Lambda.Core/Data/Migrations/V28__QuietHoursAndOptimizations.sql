-- How busy the installation and each lambda are by the hour of the day, and
-- what keeping their databases in shape last did.
--
-- quiet_hours holds a typical day: hours is 24 numbers, the requests of each
-- hour of the day in UTC, counted from midnight, as an average in which a
-- week ago counts half as much as today - so the quietest hours can be told
-- without keeping a row per hour that ever was. A row per lambda that was
-- visited, written hourly by a job from counters in memory, and the row of
-- lambda_id 0 for every request the server answered. covered marks, a bit
-- per hour of the day, which hours the server has counted at all, so an
-- hour it was never up for is not taken for a quiet one; it is kept on the
-- installation's row, the lambdas' hours being counted whenever it is.
--
-- database_optimizations holds the last time the database of a lambda was
-- optimized - lambda_id 0 for the platform's own records and analytics -
-- with what it took, the room before and after, and what was done or left
-- out, a line each.
--
-- Neither refers to lambdas, since the installation's rows are not one:
-- their rows go with a lambda where it is removed.

CREATE TABLE quiet_hours
(
    lambda_id INTEGER NOT NULL PRIMARY KEY,
    hours     TEXT    NOT NULL,
    covered   INTEGER NOT NULL DEFAULT 0,
    since     TEXT    NOT NULL,
    updated   TEXT    NOT NULL
);

CREATE TABLE database_optimizations
(
    lambda_id   INTEGER NOT NULL PRIMARY KEY,
    optimized   TEXT    NOT NULL,
    millis      INTEGER NOT NULL,
    size_before INTEGER NOT NULL,
    size_after  INTEGER NOT NULL,
    steps       TEXT    NOT NULL,
    skipped     TEXT    NULL,
    failure     TEXT    NULL
);
