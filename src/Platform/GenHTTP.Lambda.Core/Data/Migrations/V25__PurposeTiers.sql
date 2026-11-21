-- The tiers are named by what a lambda is for rather than by who pays.
--
-- Free and Premium mixed two questions: who pays, which is the installation's
-- business, and how much a lambda matters, which is the lambda's. A free
-- lambda becomes a sandbox and a premium one goes to production; what either
-- may do does not change. Nothing becomes a system lambda here - that is the
-- operator's to decide, lambda by lambda.

UPDATE lambdas SET tier = 'Sandbox' WHERE tier = 'Free';

UPDATE lambdas SET tier = 'Production' WHERE tier = 'Premium';

-- The limits the operator saved in the panel, under the tiers' new names -
-- the four retired allowances too, which the two of today are still made of
-- until they are saved.

UPDATE settings SET key = 'limits.sandbox.' || substr(key, length('limits.free.') + 1) WHERE key LIKE 'limits.free.%';

UPDATE settings SET key = 'limits.production.' || substr(key, length('limits.premium.') + 1) WHERE key LIKE 'limits.premium.%';

-- How many versions are kept and how many features may be open were the same
-- in every tier, so they are one setting each now. Where an operator saved
-- two, the larger is kept - the panel never let the free one be the larger -
-- so no lambda keeps fewer than it did.

INSERT INTO settings (key, value)
SELECT 'limits.versions', saved FROM
    (SELECT CAST(MAX(CAST(value AS INTEGER)) AS TEXT) AS saved FROM settings WHERE key IN ('limits.sandbox.versions', 'limits.production.versions'))
WHERE saved IS NOT NULL;

INSERT INTO settings (key, value)
SELECT 'limits.features', saved FROM
    (SELECT CAST(MAX(CAST(value AS INTEGER)) AS TEXT) AS saved FROM settings WHERE key IN ('limits.sandbox.features', 'limits.production.features'))
WHERE saved IS NOT NULL;

DELETE FROM settings
WHERE key IN ('limits.sandbox.versions', 'limits.production.versions', 'limits.sandbox.features', 'limits.production.features');

-- Connectivity was part of the premium tier whatever had been decided about
-- the lambda; now a decision about the lambda comes before what its tier
-- allows by default, and a request connects nothing until it is allowed. A
-- decline or a request from before a lambda went to production would take
-- away what it has, so it is forgotten - the purpose and the note stay.

UPDATE lambdas SET connectivity = NULL WHERE tier = 'Production' AND connectivity IN ('Declined', 'Requested');

-- When the tier of a lambda last changed. A lambda moved to a tier that takes
-- unused lambdas offline is given the whole time from that moment - it and
-- the previews of its features - rather than measured from a visit or an
-- edit long before, while nothing would have taken it offline.

ALTER TABLE lambdas ADD COLUMN tier_changed TEXT NULL;
