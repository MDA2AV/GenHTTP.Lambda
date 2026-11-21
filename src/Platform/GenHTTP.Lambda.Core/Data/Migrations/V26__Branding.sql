-- The pictures the operator gave the installation to present itself with,
-- a row each by what it is for: its logo, for now.
--
-- Its name is a setting (branding.name). A picture is kept in the row
-- rather than on disk, as the showcase's are: it is small and bounded, and a
-- backup of the database is then a backup of how the installation looks.

CREATE TABLE branding_pictures
(
    purpose  TEXT NOT NULL PRIMARY KEY,
    type     TEXT NOT NULL,
    content  BLOB NOT NULL,
    updated  TEXT NOT NULL
);
