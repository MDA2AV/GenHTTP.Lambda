-- The domain a lambda answers at besides its path below /lambda/.
--
-- Only honoured for a lambda in the premium tier, but kept regardless: a
-- lambda that drops out of the tier keeps what its owner configured, and is
-- reachable at it again the moment it is promoted back.
--
-- Stored as it is matched against the Host header - lower case, no port, no
-- trailing dot, internationalized names in their ASCII form - so the lookup is
-- a comparison and never a normalization. One lambda per domain; the index is
-- partial because almost every lambda has none.

ALTER TABLE lambdas ADD COLUMN domain TEXT NULL;

CREATE UNIQUE INDEX ux_lambdas_domain ON lambdas (domain) WHERE domain IS NOT NULL;
