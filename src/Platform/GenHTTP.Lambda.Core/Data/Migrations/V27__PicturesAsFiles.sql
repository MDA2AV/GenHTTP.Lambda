-- The pictures the platform keeps - the logo of the branding, the pictures of
-- the showcase - are files of the files integration rather than rows (decided
-- by the owner: no pictures in a database).
--
-- A showcase entry says how large its picture is, which was read off its
-- bytes before. The bytes themselves are moved out by the platform as it
-- starts, which a migration cannot do, and the columns that held them are
-- left empty - to be dropped once every installation has moved its pictures.

ALTER TABLE showcases ADD COLUMN image_bytes INTEGER NOT NULL DEFAULT 0;

UPDATE showcases SET image_bytes = length(image);
