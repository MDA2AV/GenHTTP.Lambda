-- What the operator can do about abuse: block a lambda, and the addresses it
-- was changed from.
--
-- A blocked lambda is offline and nobody may change it any more - its owner
-- through no door, and the operator only by lifting the block or deleting it.
-- blocked says since when, block_reason why, in the operator's words; neither
-- is ever shown to the owner but the fact of it.
--
-- editor_addresses are the addresses a lambda was changed from with its
-- editor key - created, saved, put online, its drafts and files - so that the
-- operator who blocks it can block where it came from as well. One row per
-- lambda and address, kept for a while after it was last seen and then
-- pruned, and never one of the installation's own networks: the build agent
-- reaches the platform from one.
--
-- address_blocks are the addresses refused before anything else is done for
-- them, for two weeks: an IPv4 address as it is, an IPv6 address with the /64
-- it belongs to. The lambda a block came from is kept by its public key
-- rather than by reference, since a block outlives the lambda it was made
-- for.

ALTER TABLE lambdas ADD COLUMN blocked TEXT NULL;
ALTER TABLE lambdas ADD COLUMN block_reason TEXT NULL;

CREATE TABLE editor_addresses
(
    lambda_id   INTEGER NOT NULL REFERENCES lambdas (id) ON DELETE CASCADE,
    address     TEXT    NOT NULL,
    first_seen  TEXT    NOT NULL,
    last_seen   TEXT    NOT NULL,
    PRIMARY KEY (lambda_id, address)
);

CREATE INDEX ix_editor_addresses_last_seen ON editor_addresses (last_seen);

CREATE TABLE address_blocks
(
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    address     TEXT    NOT NULL UNIQUE,
    reason      TEXT    NULL,
    public_key  TEXT    NULL,
    created     TEXT    NOT NULL
);
