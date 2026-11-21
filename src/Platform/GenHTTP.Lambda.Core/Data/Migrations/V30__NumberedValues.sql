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
-- SQLite cannot change the type of a column, so each gets a new one, filled
-- from the old, which is dropped and whose name it takes. The events are
-- appended to and never changed, so they are converted here once, and their
-- index is made again over the new column.

ALTER TABLE lambdas ADD COLUMN tier_number INTEGER NOT NULL DEFAULT 0;

UPDATE lambdas SET tier_number = CASE tier
    WHEN 'Sandbox' THEN 0
    WHEN 'Production' THEN 1
    WHEN 'System' THEN 2
    WHEN 'Demo' THEN 3
    WHEN 'Free' THEN 0
    WHEN 'Premium' THEN 1
    ELSE 0
END;

ALTER TABLE lambdas DROP COLUMN tier;

ALTER TABLE lambdas RENAME COLUMN tier_number TO tier;

ALTER TABLE lambdas ADD COLUMN editor_view_number INTEGER NOT NULL DEFAULT 0;

UPDATE lambdas SET editor_view_number = CASE editor_view
    WHEN 'Full' THEN 0
    WHEN 'Simple' THEN 1
    ELSE 0
END;

ALTER TABLE lambdas DROP COLUMN editor_view;

ALTER TABLE lambdas RENAME COLUMN editor_view_number TO editor_view;

ALTER TABLE lambdas ADD COLUMN connectivity_number INTEGER NULL;

UPDATE lambdas SET connectivity_number = CASE connectivity
    WHEN 'Requested' THEN 0
    WHEN 'Allowed' THEN 1
    WHEN 'Declined' THEN 2
END;

ALTER TABLE lambdas DROP COLUMN connectivity;

ALTER TABLE lambdas RENAME COLUMN connectivity_number TO connectivity;

ALTER TABLE deployments ADD COLUMN origin_number INTEGER NULL;

UPDATE deployments SET origin_number = CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE deployments DROP COLUMN origin;

ALTER TABLE deployments RENAME COLUMN origin_number TO origin;

ALTER TABLE activations ADD COLUMN origin_number INTEGER NULL;

UPDATE activations SET origin_number = CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE activations DROP COLUMN origin;

ALTER TABLE activations RENAME COLUMN origin_number TO origin;

ALTER TABLE activations ADD COLUMN ended_by_number INTEGER NULL;

UPDATE activations SET ended_by_number = CASE ended_by
    WHEN 'replaced' THEN 1
    WHEN 'stopped' THEN 2
    WHEN 'expired' THEN 3
    WHEN 'admin' THEN 4
END;

ALTER TABLE activations DROP COLUMN ended_by;

ALTER TABLE activations RENAME COLUMN ended_by_number TO ended_by;

ALTER TABLE features ADD COLUMN origin_number INTEGER NULL;

UPDATE features SET origin_number = CASE origin
    WHEN 'template' THEN 1
    WHEN 'api' THEN 2
    WHEN 'agent' THEN 3
    WHEN 'git' THEN 4
    WHEN 'admin' THEN 5
    WHEN 'system' THEN 6
END;

ALTER TABLE features DROP COLUMN origin;

ALTER TABLE features RENAME COLUMN origin_number TO origin;

DROP INDEX ix_events_kind_occurred;

ALTER TABLE events ADD COLUMN kind_number INTEGER NOT NULL DEFAULT 0;

UPDATE events SET kind_number = CASE kind
    WHEN 'created' THEN 1
    WHEN 'saved' THEN 2
    WHEN 'deployed' THEN 3
    WHEN 'undeployed' THEN 4
    WHEN 'deleted' THEN 5
    ELSE 0
END;

ALTER TABLE events DROP COLUMN kind;

ALTER TABLE events RENAME COLUMN kind_number TO kind;

CREATE INDEX ix_events_kind_occurred ON events (kind, occurred);
