-- The site has no enterprise page any more.
--
-- The operator could switch the header's link to the page with the prices of
-- the enterprise edition on and off. The page is gone, and with it the
-- switch; the row it was kept in, where it was ever changed, goes too, so
-- the settings hold nothing that no longer means anything.

DELETE FROM settings WHERE key = 'enterprise-page';
