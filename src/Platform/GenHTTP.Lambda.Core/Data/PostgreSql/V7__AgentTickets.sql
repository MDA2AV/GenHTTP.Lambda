-- The tickets the jobs of the build agent are handed: the same as V32 of the
-- SQLite migrations, where it says more.

CREATE TABLE agent_tickets
(
    hash    text        NOT NULL PRIMARY KEY,
    model   smallint    NOT NULL,
    effort  smallint    NOT NULL,
    expires timestamptz NOT NULL
);

CREATE INDEX ix_agent_tickets_expires ON agent_tickets (expires);
