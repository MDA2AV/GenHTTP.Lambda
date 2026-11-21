-- The analytics of the lambdas, in a database of their own (analytics.db).
--
-- A star: the lambdas whose visits are counted, and a table of facts per
-- statistic - the totals of a day, and one for each thing a visit is counted
-- by - each row the counts of one lambda on one day (in UTC), and of one value
-- where there is one. The day is kept as its year, month and day, which lead
-- the key after the lambda, so a lambda's days are read as a range of the key
-- and old ones are removed the same way.
--
-- Written by a background job every few minutes, adding to the rows of the
-- day, never while a request is served. Kept apart from the platform's
-- database so that writing a batch of counts never holds up what the editor
-- and the API write there, and so that it can be backed up, moved or thrown
-- away on its own: nothing else depends on it.
--
-- hits are every request answered, views the requests that were somebody
-- opening a page.

CREATE TABLE lambdas (
    id INTEGER PRIMARY KEY,
    created TEXT NOT NULL
);

CREATE TABLE days (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    visitors INTEGER NOT NULL DEFAULT 0,
    failed INTEGER NOT NULL DEFAULT 0,
    rejected INTEGER NOT NULL DEFAULT 0,
    bytes INTEGER NOT NULL DEFAULT 0,
    millis REAL NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day)
) WITHOUT ROWID;

CREATE TABLE pages (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE referrers (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE countries (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE devices (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE browsers (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE systems (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

CREATE TABLE crawlers (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value TEXT NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;
