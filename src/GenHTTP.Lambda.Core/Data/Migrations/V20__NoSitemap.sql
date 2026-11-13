-- The sitemap names no lambda any more.
--
-- The operator could list a lambda in the sitemap of the installation, which
-- named its address below /lambda/. The lambdas answer at hosts of their own
-- now, below the hosting domain, and a sitemap names pages of the host it is
-- served from - so there is nothing left for the switch to do.

ALTER TABLE lambdas DROP COLUMN in_sitemap;
