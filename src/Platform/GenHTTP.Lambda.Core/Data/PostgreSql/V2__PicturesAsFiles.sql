-- The pictures the platform keeps are files of the files integration rather
-- than rows: as V27 of the SQLite set (Data/Migrations), whose comment says
-- why. The bytes are moved out by the platform as it starts.

ALTER TABLE showcases ADD COLUMN image_bytes integer NOT NULL DEFAULT 0;

UPDATE showcases SET image_bytes = octet_length(image);
