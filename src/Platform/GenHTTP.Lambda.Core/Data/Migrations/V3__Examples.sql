-- Lambdas the installation maintains itself, so visitors have something that
-- already runs to look at before they write anything.
--
-- They differ from everything else in this table only in what the maintenance
-- job is allowed to do to them: an example nobody has touched for a month is
-- still wanted, and one deployed yesterday should still be answering today.
-- The flag is what keeps both sweeps off them.

ALTER TABLE lambdas ADD COLUMN is_example INTEGER NOT NULL DEFAULT 0;

CREATE INDEX ix_lambdas_is_example ON lambdas (is_example);
