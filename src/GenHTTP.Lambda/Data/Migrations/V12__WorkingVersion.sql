-- The newest version can be worked on.
--
-- Every save used to be a new version, so an agent fixing a typo, then a
-- compiler error, then the typo it made fixing that, left three versions for
-- one thing the user asked for - and a history nobody could read. The newest
-- version may now be changed where it is, as often as it takes; the ones
-- before it stay as they were, to go back to.
--
-- A revision counts the saves of one version, the first included, and says
-- which of them is online: the version online can be changed without anything
-- visitors see changing until it is deployed again, so "version 6 is online"
-- no longer says which version 6.

ALTER TABLE deployments ADD COLUMN revision INTEGER NOT NULL DEFAULT 1;

ALTER TABLE deployments ADD COLUMN modified TEXT NULL;

ALTER TABLE lambdas ADD COLUMN active_revision INTEGER NULL;

UPDATE lambdas SET active_revision = 1 WHERE active_version IS NOT NULL;

ALTER TABLE activations ADD COLUMN revision INTEGER NULL;
