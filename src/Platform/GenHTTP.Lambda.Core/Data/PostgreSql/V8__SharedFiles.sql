-- The files whoever runs the installation shares across it: the same as V33
-- of the SQLite migrations, where it says more.

CREATE TABLE shared_files
(
    name         text        NOT NULL PRIMARY KEY,
    bytes        integer     NOT NULL,
    hash         text        NOT NULL,
    updated      timestamptz NOT NULL,
    font_family  text,
    font_from    integer,
    font_to      integer,
    font_italic  boolean,
    font_letters text
);
