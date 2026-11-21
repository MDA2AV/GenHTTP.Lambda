-- The analytics of the lambdas, in PostgreSQL: the schema analytics.
--
-- The same star as in SQLite (see Services/Analytics/Migrations), written once
-- as it stands after the migrations there, since an installation starts on
-- PostgreSQL with none of their history. Counts are bigint, so a lambda's year
-- of bytes fits. Migrations from here on are numbered on their own, and every
-- change is made in both sets.

CREATE TABLE lambdas (
    id      bigint NOT NULL PRIMARY KEY,
    created text   NOT NULL
);

CREATE TABLE days (
    lambda    bigint           NOT NULL,
    year      integer          NOT NULL,
    month     integer          NOT NULL,
    day       integer          NOT NULL,
    hits      bigint           NOT NULL DEFAULT 0,
    views     bigint           NOT NULL DEFAULT 0,
    bot_views bigint           NOT NULL DEFAULT 0,
    visitors  bigint           NOT NULL DEFAULT 0,
    failed    bigint           NOT NULL DEFAULT 0,
    rejected  bigint           NOT NULL DEFAULT 0,
    bytes     bigint           NOT NULL DEFAULT 0,
    millis    double precision NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day)
);

CREATE TABLE pages (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE referrers (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE countries (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE devices (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE browsers (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE systems (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);

CREATE TABLE crawlers (
    lambda    bigint  NOT NULL,
    year      integer NOT NULL,
    month     integer NOT NULL,
    day       integer NOT NULL,
    value     text    NOT NULL,
    hits      bigint  NOT NULL DEFAULT 0,
    views     bigint  NOT NULL DEFAULT 0,
    bot_views bigint  NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
);
