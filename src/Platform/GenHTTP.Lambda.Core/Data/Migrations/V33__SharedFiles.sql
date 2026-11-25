-- The files whoever runs the installation shares across it - fonts and a logo
-- for its pages, a style guide for the agents that build on it, a style sheet
-- for their apps - a row each by its name.
--
-- The content is a file of the files integration, kept under the SHA-256 of
-- what it holds (no file is kept in a database). What a font draws is read
-- from it as it is uploaded, and kept for the pages to be told with.

CREATE TABLE shared_files
(
    name         TEXT    NOT NULL PRIMARY KEY,
    bytes        INTEGER NOT NULL,
    hash         TEXT    NOT NULL,
    updated      TEXT    NOT NULL,
    font_family  TEXT,
    font_from    INTEGER,
    font_to      INTEGER,
    font_italic  INTEGER,
    font_letters TEXT
);
