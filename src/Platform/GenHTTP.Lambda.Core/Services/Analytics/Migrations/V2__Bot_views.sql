-- The pages opened by programs rather than people, counted apart from the
-- views they are part of, so the owner can leave them out.
--
-- bot_views are the views by anything the classifier did not take for a
-- person: crawlers, and programs that name no browser. What people opened is
-- views - bot_views.
--
-- What was counted before is filled in where it can be told: a day's totals
-- from its devices, whose bots and others are programs to a visit, and the
-- devices and crawlers themselves. Browsers and systems only ever counted
-- people. The pages, referrers and countries of those days cannot be told
-- apart any more and keep their bots among their people.

ALTER TABLE days ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE pages ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE referrers ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE countries ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE devices ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE browsers ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE systems ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;
ALTER TABLE crawlers ADD COLUMN bot_views INTEGER NOT NULL DEFAULT 0;

UPDATE devices SET bot_views = views WHERE value IN ('bot', 'other');

UPDATE crawlers SET bot_views = views;

UPDATE days SET bot_views = (
    SELECT IFNULL(SUM(d.views), 0) FROM devices d
    WHERE d.lambda = days.lambda AND d.year = days.year AND d.month = days.month AND d.day = days.day AND d.value IN ('bot', 'other')
);
