-- What a column holds of a few known values is a small number now rather
-- than a name: the tier, the view the editor opens in and the state of a
-- request to connect of a lambda, where a version, a deployment or a feature
-- came from, why a lambda went offline, and what an event was. The numbers are
-- the members of the enums in Data/Entities, each of which keeps its number
-- for good; what the API, MCP and the pages call them is spelled out beside
-- them (LambdaTierNames and the like), so nothing a caller sees changes.
--
-- A name no member has is read as the least it could mean: a tier as the
-- sandbox (Free and Premium, the names before V25, as the sandbox and
-- production), a view as the full one, the rest as not recorded - and an
-- event as one of no kind the platform counts (0).
--
-- The same as V30 of the SQLite migrations, where it says more.

ALTER TABLE lambdas ALTER COLUMN tier TYPE smallint USING CASE tier
    WHEN 'Sandbox' THEN 0
    WHEN 'Production' THEN 1
    WHEN 'System' THEN 2
    WHEN 'Demo' THEN 3
    WHEN 'Free' THEN 0
    WHEN 'Premium' THEN 1
    ELSE 0
END;

ALTER TABLE lambdas ALTER COLUMN editor_view DROP DEFAULT;

ALTER TABLE lambdas ALTER COLUMN editor_view TYPE smallint USING CASE editor_view
    WHEN 'Full' THEN 0
    WHEN 'Simple' THEN 1
    ELSE 0
END;

ALTER TABLE lambdas ALTER COLUMN editor_view SET DEFAULT 0;

ALTER TABLE lambdas ALTER COLUMN connectivity TYPE smallint USING CASE connectivity
    WHEN 'Requested' THEN 0
    WHEN 'Allowed' THEN 1
    WHEN 'Declined' THEN 2
END;

ALTER TABLE deployments ALTER COLUMN origin TYPE smallint USING CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE activations ALTER COLUMN origin TYPE smallint USING CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE activations ALTER COLUMN ended_by TYPE smallint USING CASE ended_by
    WHEN 'replaced' THEN 1
    WHEN 'stopped' THEN 2
    WHEN 'expired' THEN 3
    WHEN 'admin' THEN 4
END;

ALTER TABLE features ALTER COLUMN origin TYPE smallint USING CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE events ALTER COLUMN kind TYPE smallint USING CASE kind
    WHEN 'created' THEN 1
    WHEN 'saved' THEN 2
    WHEN 'deployed' THEN 3
    WHEN 'undeployed' THEN 4
    WHEN 'deleted' THEN 5
    ELSE 0
END;
