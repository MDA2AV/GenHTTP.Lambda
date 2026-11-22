-- The tickets the jobs of the build agent are handed, by which what a job
-- saves over MCP is known to be the build agent's, on the model and effort
-- the job runs on (AgentModel, AgentEffort, each stored as its number). Kept
-- here rather than in the memory of the server that issued them, so a ticket
-- outlives a restart and is known to whichever server the job's calls reach.
--
-- Only the SHA-256 of a ticket is kept, as a hex string: what the table holds
-- cannot be presented as one. A ticket lasts a day, and the expired ones are
-- removed by a background job.

CREATE TABLE agent_tickets
(
    hash    TEXT    NOT NULL PRIMARY KEY,
    model   INTEGER NOT NULL,
    effort  INTEGER NOT NULL,
    expires TEXT    NOT NULL
);

CREATE INDEX ix_agent_tickets_expires ON agent_tickets (expires);
