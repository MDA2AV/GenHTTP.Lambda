-- The indices the queries actually use, and not one that none does.
--
-- A feature's copy of the secrets is deleted by the feature alone when the
-- feature is merged or deleted, and the one index on secrets starts with the
-- lambda - so every such delete read the whole table. Partial, because the
-- lambda's own secrets have no feature and are never looked up this way.

CREATE INDEX ix_secrets_feature ON secrets (feature_id) WHERE feature_id IS NOT NULL;

-- When a lambda was put online is shown and compared in memory, and never
-- searched for, so its index was only ever written to - on every deployment.

DROP INDEX ix_lambdas_deployed;
