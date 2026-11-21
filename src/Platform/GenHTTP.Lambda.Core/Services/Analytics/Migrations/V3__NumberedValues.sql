-- Devices, browsers and systems are kept as a small number each rather than
-- as their names, which every row of every lambda and day repeated: the
-- classifier only ever writes one of a few, which are numbered in code
-- (VisitorDevice, VisitorBrowser, VisitorSystem in Services/Analytics/Model;
-- AnalyticsCodes names them), and 0 is other there. A name this migration
-- does not know - none was ever written - is counted as other, merged into
-- the other of its day.
--
-- The value is part of each table's key, so each is made anew and filled
-- from the old one, summed per day and number.

CREATE TABLE devices_numbered (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value INTEGER NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    bot_views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

INSERT INTO devices_numbered (lambda, year, month, day, value, hits, views, bot_views)
SELECT lambda, year, month, day, number, SUM(hits), SUM(views), SUM(bot_views)
FROM (
    SELECT lambda, year, month, day, hits, views, bot_views, CASE value
        WHEN 'desktop' THEN 1
        WHEN 'mobile' THEN 2
        WHEN 'tablet' THEN 3
        WHEN 'bot' THEN 4
        ELSE 0
    END AS number
    FROM devices
)
GROUP BY lambda, year, month, day, number;

DROP TABLE devices;

ALTER TABLE devices_numbered RENAME TO devices;

CREATE TABLE browsers_numbered (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value INTEGER NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    bot_views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

INSERT INTO browsers_numbered (lambda, year, month, day, value, hits, views, bot_views)
SELECT lambda, year, month, day, number, SUM(hits), SUM(views), SUM(bot_views)
FROM (
    SELECT lambda, year, month, day, hits, views, bot_views, CASE value
        WHEN 'Chrome' THEN 1
        WHEN 'Safari' THEN 2
        WHEN 'Firefox' THEN 3
        WHEN 'Edge' THEN 4
        WHEN 'Opera' THEN 5
        WHEN 'Brave' THEN 6
        WHEN 'Samsung Internet' THEN 7
        WHEN 'Yandex Browser' THEN 8
        WHEN 'Vivaldi' THEN 9
        WHEN 'UC Browser' THEN 10
        WHEN 'DuckDuckGo' THEN 11
        WHEN 'Chromium' THEN 12
        WHEN 'Instagram' THEN 13
        WHEN 'Facebook' THEN 14
        WHEN 'TikTok' THEN 15
        WHEN 'WeChat' THEN 16
        WHEN 'LINE' THEN 17
        WHEN 'Snapchat' THEN 18
        WHEN 'Pinterest' THEN 19
        WHEN 'LinkedIn' THEN 20
        ELSE 0
    END AS number
    FROM browsers
)
GROUP BY lambda, year, month, day, number;

DROP TABLE browsers;

ALTER TABLE browsers_numbered RENAME TO browsers;

CREATE TABLE systems_numbered (
    lambda INTEGER NOT NULL,
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    day INTEGER NOT NULL,
    value INTEGER NOT NULL,
    hits INTEGER NOT NULL DEFAULT 0,
    views INTEGER NOT NULL DEFAULT 0,
    bot_views INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (lambda, year, month, day, value)
) WITHOUT ROWID;

INSERT INTO systems_numbered (lambda, year, month, day, value, hits, views, bot_views)
SELECT lambda, year, month, day, number, SUM(hits), SUM(views), SUM(bot_views)
FROM (
    SELECT lambda, year, month, day, hits, views, bot_views, CASE value
        WHEN 'Windows' THEN 1
        WHEN 'macOS' THEN 2
        WHEN 'Linux' THEN 3
        WHEN 'Android' THEN 4
        WHEN 'iOS' THEN 5
        WHEN 'ChromeOS' THEN 6
        ELSE 0
    END AS number
    FROM systems
)
GROUP BY lambda, year, month, day, number;

DROP TABLE systems;

ALTER TABLE systems_numbered RENAME TO systems;
