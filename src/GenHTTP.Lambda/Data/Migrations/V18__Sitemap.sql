-- Whether the sitemap of the installation names a lambda.
--
-- Only the operator lists one, so a search engine is pointed at what somebody
-- vouched for rather than at whatever was built. Off for every lambda there
-- is, and for every new one. The sitemap names the lambda's address below
-- /lambda/ while it is online, and leaves it out while it is not: an address
-- that answers with an error is no use to a crawler.

ALTER TABLE lambdas ADD COLUMN in_sitemap INTEGER NOT NULL DEFAULT 0;
