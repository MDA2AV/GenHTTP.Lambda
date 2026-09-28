-- How the editor of a lambda opens for somebody who has not picked a view.
--
-- Full shows every section; Simple shows the app, how it is doing and where
-- to ask for a change, for somebody who had it built from a sentence. What a
-- person picks for themselves is kept in their browser and never here, so
-- this is only ever the default. Every lambda there is so far was made by
-- somebody who could choose the code, so they all stay Full.

ALTER TABLE lambdas ADD COLUMN editor_view TEXT NOT NULL DEFAULT 'Full';
