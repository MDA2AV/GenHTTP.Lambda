using System.Diagnostics;
using System.Text;
using System.Text.Json;

using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Visits;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Analytics;

/// <summary>
/// Whether the analytics take a visitor for a person or a program, and what
/// they say a person came with.
/// </summary>
[TestClass]
public sealed class VisitorTests
{

    #region Programs

    [TestMethod]
    public async Task EveryCrawlerTheListKnowsIsTakenForABot()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var classifier = Classifier(fixture);

        using var stream = typeof(Application).Assembly.GetManifestResourceStream("GenHTTP.Lambda.Resources.Crawlers.crawler-user-agents.json")!;

        using var list = JsonDocument.Parse(stream);

        var instances = list.RootElement.EnumerateArray()
                            .Where(e => e.TryGetProperty("instances", out _))
                            .SelectMany(e => e.GetProperty("instances").EnumerateArray().Select(i => i.GetString()!))
                            .ToList();

        Assert.IsGreaterThan(1000, instances.Count, "the list comes with the agents its crawlers really send");

        var missed = instances.Where(i => classifier.Classify(i).Device != VisitorClassifier.Bot).ToList();

        Assert.IsEmpty(missed, "taken for people:\n" + string.Join("\n", missed.Take(20)));
    }

    [TestMethod]
    [DataRow("okhttp/4.12.0")]
    [DataRow("curl/8.5.0")]
    [DataRow("python-requests/2.32.3")]
    [DataRow("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/141.0.0.0 Safari/537.36")]
    [DataRow("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)")]
    [DataRow("meta-externalagent/1.1 (+https://developers.facebook.com/docs/sharing/webmasters/crawler)")]
    [DataRow("Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; ClaudeBot/1.0; +claudebot@anthropic.com)")]
    public async Task AProgramIsNoPerson(string agent)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var read = Classifier(fixture).Classify(agent);

        Assert.AreEqual(VisitorClassifier.Bot, read.Device);
        Assert.IsNull(read.Browser, "a program has no browser to count");
        Assert.IsNotNull(read.Crawler);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("MyShop/3.2 CFNetwork/1568.100.1 Darwin/24.0.0")]
    public async Task WhatDoesNotNameABrowserIsNeither(string? agent)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var read = Classifier(fixture).Classify(agent);

        Assert.AreEqual(VisitorClassifier.Unknown, read.Device);
        Assert.IsNull(read.Browser);
        Assert.IsNull(read.Crawler, "nothing says it is a crawler either");
    }

    #endregion

    #region People

    [TestMethod]
    [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36", "desktop", "Chrome", "Windows")]
    [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.6 Mobile/15E148 Safari/604.1", "mobile", "Safari", "iOS")]
    [DataRow("Mozilla/5.0 (iPad; CPU OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.6 Mobile/15E148 Safari/604.1", "tablet", "Safari", "iOS")]
    [DataRow("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Mobile Safari/537.36", "mobile", "Chrome", "Android")]
    [DataRow("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36", "tablet", "Chrome", "Android")]
    [DataRow("Mozilla/5.0 (Linux; Android 13; Cubot X30) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Mobile Safari/537.36", "mobile", "Chrome", "Android")]
    [DataRow("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.6 Safari/605.1.15", "desktop", "Safari", "macOS")]
    [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:143.0) Gecko/20100101 Firefox/143.0", "desktop", "Firefox", "Windows")]
    [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/143.0 Mobile/15E148 Safari/605.1.15", "mobile", "Firefox", "iOS")]
    [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/141.0.7390.41 Mobile/15E148 Safari/604.1", "mobile", "Chrome", "iOS")]
    [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0", "desktop", "Edge", "Windows")]
    [DataRow("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 OPR/120.0.0.0", "desktop", "Opera", "Linux")]
    [DataRow("Mozilla/5.0 (Linux; Android 14; SAMSUNG SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/28.0 Chrome/130.0.0.0 Mobile Safari/537.36", "mobile", "Samsung Internet", "Android")]
    [DataRow("Mozilla/5.0 (X11; CrOS x86_64 14541.0.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36", "desktop", "Chrome", "ChromeOS")]
    [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/22G86 Instagram 389.0.0.0 (iPhone15,2; iOS 18_6; de_DE; de; scale=3.00; 1179x2556)", "mobile", "Instagram", "iOS")]
    [DataRow("Mozilla/5.0 (Linux; Android 14; Pixel 8 Build/AP2A; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/141.0.0.0 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/480.0.0.0;]", "mobile", "Facebook", "Android")]
    [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148", "mobile", "other", "iOS")]
    public async Task APersonIsReadByTheBrowserTheyCameWith(string agent, string device, string browser, string system)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var read = Classifier(fixture).Classify(agent);

        Assert.AreEqual(device, read.Device);
        Assert.AreEqual(browser, read.Browser);
        Assert.AreEqual(system, read.System);
        Assert.IsNull(read.Crawler);
    }

    [TestMethod]
    public async Task TheHintsOfChromiumSayWhatItsAgentNoLongerDoes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var classifier = Classifier(fixture);

        // Chromium's agent of a desktop is the same on every system; Brave
        // does not name itself there at all
        const string agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

        var brave = classifier.Classify(agent, new ClientHints("\"Brave\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"", "?0", "\"macOS\""));

        Assert.AreEqual("Brave", brave.Browser);
        Assert.AreEqual("macOS", brave.System);
        Assert.AreEqual(VisitorClassifier.Desktop, brave.Device);

        // a phone that asked for the desktop site still says it is a phone
        var phone = classifier.Classify(agent, new ClientHints("\"Google Chrome\";v=\"141\", \"Chromium\";v=\"141\"", "?1", "\"Android\""));

        Assert.AreEqual(VisitorClassifier.Mobile, phone.Device);
        Assert.AreEqual("Android", phone.System);
        Assert.AreEqual("Chrome", phone.Browser);
    }

    [TestMethod]
    public async Task HintsAreTakenOnlyWhereTheyAreChromiumsAndSayWhatIsKnown()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var classifier = Classifier(fixture);

        // a Firefox sending Chromium's hints is a Firefox
        var firefox = classifier.Classify("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:143.0) Gecko/20100101 Firefox/143.0",
                                          new ClientHints("\"Google Chrome\";v=\"141\"", "?1", "\"Android\""));

        Assert.AreEqual("Firefox", firefox.Browser);
        Assert.AreEqual("Windows", firefox.System);
        Assert.AreEqual(VisitorClassifier.Desktop, firefox.Device);

        // a brand or a system nobody knows is not written into anybody's
        // figures: the agent says what it is instead
        var made = classifier.Classify("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36",
                                       new ClientHints("\"<b>Buy now</b>\";v=\"1\", \"Not_A Brand\";v=\"8\", \"Chromium\";v=\"141\"", "?0", "\"TempleOS\""));

        Assert.AreEqual("Chrome", made.Browser);
        Assert.AreEqual("Windows", made.System);
    }

    #endregion

    #region Claims

    [TestMethod]
    public async Task ACrawlerIsGooglesOnlyFromAnAddressGooglePublishes()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var classifier = Classifier(fixture);

        const string googlebot = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

        Assert.AreEqual(CrawlerRanges.Unconfirmed, classifier.Classify(googlebot, client: "66.249.66.1").Crawler,
                        "nothing is confirmed before the addresses are known");

        Ranges(fixture).Load("Google", [Prefixes("66.249.64.0/19", "2001:4860:4801:10::/64")]);

        Assert.AreEqual("Google", classifier.Classify(googlebot, client: "66.249.66.1").Crawler);
        Assert.AreEqual("Google", classifier.Classify(googlebot, client: "2001:4860:4801:10::1").Crawler);

        var impostor = classifier.Classify(googlebot, client: "203.0.113.9");

        Assert.AreEqual(CrawlerRanges.Unconfirmed, impostor.Crawler, "a scraper calling itself Googlebot");
        Assert.AreEqual(VisitorClassifier.Bot, impostor.Device, "but still no person");

        Assert.AreEqual(CrawlerRanges.Unconfirmed, classifier.Classify(googlebot, client: "66.249.66.1 via 203.0.113.9").Crawler,
                        "a forwarding header is written by the sender, and claims Google's address as easily as Google's name");

        Assert.AreEqual(CrawlerRanges.Unconfirmed, classifier.Classify(googlebot).Crawler, "nor without an address");

        Assert.Contains("Google", Ranges(fixture).Confirmed);
    }

    [TestMethod]
    public async Task AListThatDoesNotReadLeavesTheOneBefore()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var list = fixture.Application.Services.GetRequiredService<CrawlerList>();

        var known = list.Count;

        Assert.IsFalse(list.Load(new MemoryStream("<html>Service unavailable</html>"u8.ToArray())));
        Assert.IsFalse(list.Load(new MemoryStream("[]"u8.ToArray())));
        Assert.IsFalse(Ranges(fixture).Load("Bing", [new MemoryStream("{\"oops\": 1}"u8.ToArray())]));

        Assert.AreEqual(known, list.Count);

        // and a newer one is taken, what was read before let go of
        var classifier = Classifier(fixture);

        Assert.AreEqual(VisitorClassifier.Unknown, classifier.Classify("FreshBot/1.0").Device);

        Assert.IsTrue(list.Load(new MemoryStream("[{\"pattern\": \"FreshBot\", \"tags\": [\"seo\"]}]"u8.ToArray())));

        Assert.AreEqual("seo", classifier.Classify("FreshBot/1.0").Crawler);
    }

    #endregion

    #region Abuse

    [TestMethod]
    public async Task AnAgentMadeToBeSlowIsNot()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var classifier = Classifier(fixture);

        // warm, so what is measured is the reading and not the first build of
        // the expressions
        classifier.Classify("Mozilla/5.0 (Windows NT 10.0) Chrome/141.0.0.0");

        var hostile = "Mozilla/5.0 (" + string.Concat(Enumerable.Repeat("compatible; a", 20_000)) + ")";

        var clock = Stopwatch.StartNew();

        for (var i = 0; i < 50; i++)
        {
            classifier.Classify(hostile + i);
        }

        Assert.IsLessThan(5000, clock.ElapsedMilliseconds, "matching takes time in proportion to what is read, and only so much is read");
    }

    #endregion

    #region Helpers

    private static VisitorClassifier Classifier(LambdaFixture fixture) => fixture.Application.Services.GetRequiredService<VisitorClassifier>();

    private static CrawlerRanges Ranges(LambdaFixture fixture) => fixture.Application.Services.GetRequiredService<CrawlerRanges>();

    /// <summary>
    /// A list of addresses in the shape the operators publish them.
    /// </summary>
    private static MemoryStream Prefixes(params string[] prefixes)
    {
        var entries = prefixes.Select(p => p.Contains(':') ? $"{{\"ipv6Prefix\": \"{p}\"}}" : $"{{\"ipv4Prefix\": \"{p}\"}}");

        return new MemoryStream(Encoding.UTF8.GetBytes($"{{\"creationTime\": \"2026-10-12T00:00:00\", \"prefixes\": [{string.Join(',', entries)}]}}"));
    }

    #endregion

}
