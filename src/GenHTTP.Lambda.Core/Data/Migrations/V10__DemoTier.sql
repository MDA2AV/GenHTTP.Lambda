-- The lambdas the installation maintains itself are a tier now rather than a
-- flag beside the tier.
--
-- The flag only kept the sweeps away. A demo also has to be read only for
-- everybody holding its editor key, which is announced, and that is what a
-- tier decides everywhere else - so the examples become the Demo tier. They
-- are not in the new catalogue, so the seeder retires them on its next run.

UPDATE lambdas SET tier = 'Demo' WHERE is_example = 1;

DROP INDEX ix_lambdas_is_example;

ALTER TABLE lambdas DROP COLUMN is_example;
