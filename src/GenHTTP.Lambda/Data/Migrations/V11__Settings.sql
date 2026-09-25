-- What the operator switched on or off from the administration panel.
--
-- One row per setting, and only once it has been changed: a missing row is
-- the default, so a new setting needs no migration of its own.

CREATE TABLE settings
(
    key    TEXT NOT NULL PRIMARY KEY,
    value  TEXT NOT NULL
);
