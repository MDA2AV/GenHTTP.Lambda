-- Whether the owner switched analytics on, and since when.
--
-- Analytics count how a lambda is visited - by day, page, country, referrer
-- and device - and are opt-in: nothing is counted for a lambda until its
-- owner asks for it. The counts themselves are not kept here but in a
-- database of their own (analytics.db), written in batches by a background
-- job, so serving a request never writes to this one. What is kept here is
-- only the switch, because it is read with everything else that decides how a
-- request to the lambda is served.

ALTER TABLE lambdas ADD COLUMN analytics_since TEXT NULL;
