using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What the server and the lambdas on it said, and who is allowed to read it.
/// </summary>
[TestClass]
public sealed class LogTests
{
    private const string Token = "a-token-nobody-would-guess";

    private static Func<LambdaOptions, LambdaOptions> WithPanel => o => o with { AdminToken = Token };

    #region The ring

    [TestMethod]
    public void TheOldestLineIsDroppedRatherThanGrowing()
    {
        var book = new LogBook(100);

        for (var i = 0; i < 250; i++)
        {
            book.Append("info", "test", null, $"line {i}");
        }

        var (lines, cursor, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(100, lines.Count, "it holds what it was told to hold and no more");
        Assert.AreEqual(250, cursor);
        Assert.AreEqual("line 150", lines[0].Text);
        Assert.AreEqual("line 249", lines[^1].Text);
    }

    [TestMethod]
    public void AReaderIsGivenOnlyWhatItHasNotSeen()
    {
        var book = new LogBook(100);

        book.Append("info", "test", null, "first");

        var (first, cursor, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(1, first.Count);

        book.Append("info", "test", null, "second");

        var (second, _, missed) = book.Read(cursor, null, LogLevel.Information, 5000);

        Assert.AreEqual(1, second.Count);
        Assert.AreEqual("second", second[0].Text);
        Assert.AreEqual(0, missed);
    }

    [TestMethod]
    public void AReaderThatFellBehindIsToldSoRatherThanQuietlyMissingLines()
    {
        var book = new LogBook(100);

        book.Append("info", "test", null, "the one it saw");

        // away long enough for the ring to turn over completely
        for (var i = 0; i < 250; i++)
        {
            book.Append("info", "test", null, $"while it was away {i}");
        }

        var (lines, _, missed) = book.Read(1, null, LogLevel.Information, 5000);

        Assert.AreEqual(100, lines.Count);
        Assert.AreEqual(150, missed, "and the count of what it will never see is part of the answer");
    }

    [TestMethod]
    public void LongLinesCostTheRingMoreThanTheirPlaceInIt()
    {
        var book = new LogBook(4000);

        // every line as long as one may be, which is the shape a lambda in a
        // throw loop leaves behind
        for (var i = 0; i < 4000; i++)
        {
            book.Append("error", "stderr", "noisy", new string('x', LogBook.MaxText), new string('y', 4000));
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.IsTrue(lines.Count < 400,
                      $"the ring is bounded in memory and not only in lines, but it held {lines.Count}");

        Assert.IsTrue(lines.Count > 0, "and it still holds the newest of them");
    }

    [TestMethod]
    public void ADeepRingIsAllowedAndStillBoundedInMemory()
    {
        // a million is what the ceiling allows; the byte budget is what
        // decides whether a million of these particular lines actually fit
        var book = new LogBook(1_000_000, megabytes: 8);

        Assert.AreEqual(1_000_000, book.Capacity);
        Assert.AreEqual(8L * 1024 * 1024 / 2, book.Budget, "counted in chars, which are two bytes each");

        for (var i = 0; i < 50_000; i++)
        {
            book.Append("info", "Requests", null, new string('x', 500));
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 50_000);

        Assert.IsTrue(lines.Count < 10_000,
                      $"eight megabytes of five hundred character lines is about eight thousand, not {lines.Count}");
    }

    [TestMethod]
    public void OneCallerCanBeReadWithoutTheRest()
    {
        var book = new LogBook(100);

        book.Append("info", "Requests", null, "from the first", null, "203.0.113.7");
        book.Append("info", "Requests", null, "from the second", null, "198.51.100.4");
        book.Append("info", "Requests", null, "from behind a proxy", null, "203.0.113.7 via 104.23.221.210");

        var (mine, _, _) = book.Read(0, null, LogLevel.Information, 5000, "203.0.113.7");

        Assert.AreEqual(2, mine.Count, "the claim and the hop are both searchable");

        var (hop, _, _) = book.Read(0, null, LogLevel.Information, 5000, "104.23.221.210");

        Assert.AreEqual(1, hop.Count);
    }

    [TestMethod]
    public void RepeatedCallersAreSharedRatherThanCopied()
    {
        var pool = new StringPool();

        var first = pool.Share("203.0.113.7");
        var second = pool.Share(string.Concat("203.0.", "113.7"));

        Assert.AreSame(first, second, "a million lines from one caller is one string, not a million");
        Assert.AreEqual(1, pool.Count);
        Assert.IsNull(pool.Share(null));
    }

    [TestMethod]
    public void ThePoolStopsRatherThanGrowingWithoutBound()
    {
        var pool = new StringPool(most: 4);

        for (var i = 0; i < 50; i++)
        {
            pool.Share($"198.51.100.{i}");
        }

        Assert.AreEqual(4, pool.Count, "past the bound it hands back what it was given and holds nothing");
    }

    [TestMethod]
    public void ArrivingIsNotFallingBehind()
    {
        var book = new LogBook(1000);

        for (var i = 0; i < 400; i++)
        {
            book.Append("info", "test", null, $"line {i}");
        }

        var (lines, _, missed) = book.Read(0, null, LogLevel.Information, 50);

        Assert.AreEqual(50, lines.Count, "a reader with no cursor is asking for the tail");
        Assert.AreEqual(0, missed, "and has not missed the rest, it never asked for it");

        var (_, _, behind) = book.Read(1, null, LogLevel.Information, 50);

        Assert.IsTrue(behind > 0, "one that did have a cursor is told what it will not be shown");
    }

    [TestMethod]
    public void OneLambdaCanBeReadWithoutTheRest()
    {
        var book = new LogBook(100);

        book.Append("info", "stdout", "alpha", "from alpha");
        book.Append("info", "stdout", "beta", "from beta");
        book.Append("info", "Server", null, "from the server");

        var (lines, _, _) = book.Read(0, "alpha", LogLevel.Information, 5000);

        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("from alpha", lines[0].Text);
    }

    [TestMethod]
    public void ALineIsCutRatherThanKeptWhole()
    {
        var book = new LogBook(100);

        book.Append("info", "stdout", "noisy", new string('x', LogBook.MaxText * 4));

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.IsTrue(lines[0].Text.Length < LogBook.MaxText + 16,
                      "a lambda printing a whole response body cannot push the rest of the ring out with it");
    }

    #endregion

    #region What a lambda prints

    [TestMethod]
    public async Task WhatALambdaPrintsIsFiledUnderIt()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("talkative");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get(() =>
                         {
                             Console.WriteLine("a line from inside the lambda");
                             return "done";
                         });
            """);

        using var called = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        Assert.AreEqual(HttpStatusCode.OK, called.StatusCode);

        var book = fixture.Book;

        var (lines, _, _) = book.Read(0, "talkative", LogLevel.Information, 5000);

        Assert.IsTrue(lines.Any(l => l.Text == "a line from inside the lambda" && l.Source == "stdout"),
                      $"instead: {string.Join(" | ", lines.Select(l => $"{l.Source}:{l.Text}"))}");
    }

    [TestMethod]
    public async Task WhatALambdaPrintsIsNotFiledUnderAnother()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var loud = await fixture.CreateLambdaAsync("loud");
        var quiet = await fixture.CreateLambdaAsync("quiet");

        await fixture.DeployAsync(loud.PrivateKey, """
            return Inline.Create()
                         .Get(() => { Console.WriteLine("mine"); return "done"; });
            """);

        await fixture.DeployAsync(quiet.PrivateKey, """
            return Inline.Create().Get(() => "done");
            """);

        using var first = await fixture.GetAsync($"/lambda/{loud.PublicKey}/");
        using var second = await fixture.GetAsync($"/lambda/{quiet.PublicKey}/");

        var book = fixture.Book;

        var (mine, _, _) = book.Read(0, "quiet", LogLevel.Information, 5000);

        Assert.IsFalse(mine.Any(l => l.Text == "mine"), "the console is shared; the attribution is not");
    }

    [TestMethod]
    public async Task WhatALambdaPrintsWhileStartingUpIsFiledUnderItToo()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("starting");

        await fixture.DeployAsync(lambda.PrivateKey, """
            Console.WriteLine("this runs once, when the lambda is built");

            return Inline.Create().Get(() => "done");
            """);

        var (lines, _, _) = fixture.Book.Read(0, "starting", LogLevel.Information, 5000);

        Assert.IsTrue(lines.Any(l => l.Text == "this runs once, when the lambda is built"),
                      "a deployment is not a request, so it carries the mark itself");
    }

    [TestMethod]
    public async Task ALambdaThatThrowsSaysWhyUnderItsOwnName()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("broken");

        await fixture.DeployAsync(lambda.PrivateKey, """
            string Boom() => throw new InvalidOperationException("the lambda is broken");

            return Inline.Create().Get(() => Boom());
            """);

        using var called = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/", "text/html");

        Assert.AreEqual(HttpStatusCode.InternalServerError, called.StatusCode);

        var book = fixture.Book;

        var (lines, _, _) = book.Read(0, "broken", LogLevel.Warning, 5000);

        var failure = lines.FirstOrDefault(l => l.Text.Contains("the lambda is broken"));

        Assert.IsNotNull(failure, "the warning the server logs about a lambda belongs to that lambda too");
        Assert.IsNotNull(failure.Detail, "and the trace it kept back from the visitor is here");
    }

    [TestMethod]
    public async Task NothingIsGatheredWhenCapturingIsOff()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync(o => WithPanel(o) with { CaptureLambdaOutput = false });

        var lambda = await fixture.CreateLambdaAsync("unwatched");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get(() => { Console.WriteLine("nobody is writing this down"); return "done"; });
            """);

        using var called = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        var book = fixture.Book;

        var (lines, _, _) = book.Read(0, "unwatched", LogLevel.Information, 5000);

        Assert.IsFalse(lines.Any(l => l.Text == "nobody is writing this down"));

        Assert.IsFalse(lines.Any(l => l.Source is "stdout" or "stderr"),
                       "which is what the setting turns off");

        // and not silence: that the lambda was reached is the server's own
        // record of a request, which this setting has nothing to do with
        Assert.IsTrue(lines.Any(l => l.Source == "Requests"));
    }

    #endregion

    [TestMethod]
    public void WhatTheEngineItselfPrintsIsKept()
    {
        var book = new LogBook(1000);

        LambdaOutput.Adopt(book);
        ConsoleTee.Install();

        // the shape the ioxide engine writes: straight to the console, no
        // logger, no lambda being served
        Console.WriteLine("[r0] listening on 0.0.0.0:80,443 (incremental=False)");
        Console.WriteLine("[r1] connection handler faulted: FlushAsync already in progress.");

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.IsTrue(lines.Any(l => l.Text.Contains("[r0] listening")), "a reactor coming up");
        Assert.IsTrue(lines.Any(l => l.Text.Contains("[r1] connection handler faulted")),
                      "and one faulting, which is the whole reason to want these");

        Assert.IsTrue(lines.All(l => l.Lambda == null), "none of it belongs to a lambda");
        Assert.IsTrue(lines.All(l => l.Source is "stdout" or "stderr"));
    }

    [TestMethod]
    public void ARecordIsKeptOnceRatherThanAlsoAsConsoleOutput()
    {
        var book = new LogBook(1000);

        LambdaOutput.Adopt(book);
        ConsoleTee.Install();

        // the console the provider writes to is taken before the tee, so its
        // line goes to the console and not back through the tee
        using var loggers = LoggerFactory.Create(b => b.AddProvider(new LogBookProvider(book, TextWriter.Null)));

        loggers.CreateLogger("Probe").LogInformation("said once");

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(1, lines.Count(l => l.Text == "said once"));
    }

    #region Folding what repeats

    [TestMethod]
    public void IdenticalLinesAreGatheredRatherThanWrittenAgain()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMinutes(5));

        for (var i = 0; i < 500; i++)
        {
            book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(1, lines.Count, "five hundred of the same thing is one fact and a rate");
        Assert.AreEqual(1, lines[0].Repeats, "the window has not closed, so nothing has been counted out yet");
    }

    [TestMethod]
    public void TheCountIsWrittenOutWhenTheWindowCloses()
    {
        // a window already past, so the next repeat closes it
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromTicks(1));

        book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");

        for (var i = 0; i < 9; i++)
        {
            book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(10, lines.Sum(l => l.Repeats), "every request is accounted for, written or folded");
    }

    [TestMethod]
    public void WhatMeasuresALineDoesNotStopItFolding()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMinutes(5));

        // the shape a request line really has: identical request, different
        // duration every time. Keyed on the text, none of these would fold.
        for (var i = 0; i < 60; i++)
        {
            book.Append("info", "Requests", null, $"GET /api/v1/examples — 200 · 0 B · {9 + i * 0.27:N2} ms",
                        null, "203.0.113.7", folding: "GET /api/v1/examples 200");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(1, lines.Count, "sixty requests, one line");
    }

    [TestMethod]
    public void ALineWithNothingToFoldOnIsAlwaysWritten()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMinutes(5));

        for (var i = 0; i < 5; i++)
        {
            book.Append("info", "Startup", null, "the same words every time");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(5, lines.Count, "folding is opted into by whoever knows what identifies the line");
    }

    [TestMethod]
    public void AFoldedLineShowsTheMostRecentOfWhatItStandsFor()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMilliseconds(40));

        book.Append("info", "Requests", null, "GET /x — 200 · 0 B · 1.00 ms", null, "203.0.113.7", folding: "GET /x 200");
        book.Append("info", "Requests", null, "GET /x — 200 · 0 B · 2.00 ms", null, "203.0.113.7", folding: "GET /x 200");
        book.Append("info", "Requests", null, "GET /x — 200 · 0 B · 3.00 ms", null, "203.0.113.7", folding: "GET /x 200");

        Thread.Sleep(80);

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        var folded = lines.Last();

        Assert.AreEqual(2, folded.Repeats);
        Assert.Contains("3.00 ms", folded.Text, "the newest of them, not the one that opened the run");
    }

    [TestMethod]
    public void ARunThatStopsStillOwesItsCount()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMilliseconds(40));

        book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");

        Thread.Sleep(80);

        // nothing more arrives; reading is what closes it out
        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(3, lines.Sum(l => l.Repeats),
                        "a scanner that has moved on would otherwise leave its last few counted and unseen");
    }

    [TestMethod]
    public void DifferentCallersDoingTheSameThingStayApart()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromMinutes(5));

        for (var i = 0; i < 20; i++)
        {
            book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
            book.Append("info", "Requests", null, "GET /probe — 404", null, "198.51.100.4", folding: "GET /probe 404");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(2, lines.Count, "which of them is doing it is the question");
    }

    [TestMethod]
    public void NothingIsFoldedWhenTheWindowIsZero()
    {
        var book = new LogBook(1000);

        for (var i = 0; i < 50; i++)
        {
            book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        }

        var (lines, _, _) = book.Read(0, null, LogLevel.Information, 5000);

        Assert.AreEqual(50, lines.Count);
    }

    [TestMethod]
    public void EveryCallerIsSummarised()
    {
        var book = new LogBook(1000);

        for (var i = 0; i < 30; i++)
        {
            book.Append("info", "Requests", null, $"GET /a/{i} — 200", null, "203.0.113.7", null, "PT", "Aveiro, PT · MEO");
        }

        book.Append("error", "Requests", null, "GET /b — 500", null, "198.51.100.4", null, "US", "Atlanta, US");
        book.Append("info", "Requests", null, "GET /c — 200", null, "198.51.100.4", null, "US", "Atlanta, US");
        book.Append("info", "Startup", null, "no caller here");

        var callers = book.Callers();

        Assert.AreEqual(2, callers.Count, "lines with no address are not a caller");

        Assert.AreEqual("203.0.113.7", callers[0].Client, "busiest first");
        Assert.AreEqual(30, callers[0].Lines);
        Assert.AreEqual("Aveiro, PT · MEO", callers[0].Place);
        Assert.AreEqual(0, callers[0].Failed);

        Assert.AreEqual(2, callers[1].Lines);
        Assert.AreEqual(1, callers[1].Failed, "and what went wrong for them");
    }

    [TestMethod]
    public void ASummaryCountsWhatAFoldedLineStandsFor()
    {
        var book = new LogBook(1000, repeatWindow: TimeSpan.FromTicks(1));

        for (var i = 0; i < 40; i++)
        {
            book.Append("info", "Requests", null, "GET /probe — 404", null, "203.0.113.7", folding: "GET /probe 404");
        }

        var callers = book.Callers();

        Assert.AreEqual(1, callers.Count);
        Assert.AreEqual(40, callers[0].Lines, "requests, not rows");
    }

    #endregion

    #region Where a caller is registered

    /// <summary>
    /// A few rows in the format the registries publish, enough to test the
    /// lookup without downloading fifty megabytes.
    /// </summary>
    private static readonly string[] Delegations =
    [
        "2.0|ripencc|1746057600|100|19830101|20260101|+0000",
        "ripencc|*|ipv4|*|100782|summary",
        "ripencc|AT|ipv4|152.53.0.0|65536|20220301|allocated|abc",
        "ripencc|GB|ipv4|74.0.0.0|16777216|19910101|allocated|def",
        "ripencc|PT|ipv6|2001:8a0::|29|20040101|allocated|ghi",
        "ripencc|DE|ipv6|2a0a:4cc0::|29|20160101|allocated|jkl",
        "arin|US|ipv4|8.8.8.0|256|20140101|assigned|mno",
        "ripencc|XX|ipv4|9.9.9.0|256|20140101|reserved|pqr",
    ];

    [TestMethod]
    public void AnAddressIsPlacedByTheRangeItBelongsTo()
    {
        var table = new GeoTable();

        table.Load(Delegations);

        Assert.AreEqual("AT", table.CountryOf("152.53.120.139"), "somewhere inside the range, not its first address");
        Assert.AreEqual("AT", table.CountryOf("152.53.0.0"), "the first address of the range");
        Assert.AreEqual("AT", table.CountryOf("152.53.255.255"), "the last one");
        Assert.AreEqual("GB", table.CountryOf("74.7.227.5"));
        Assert.AreEqual("US", table.CountryOf("8.8.8.8"), "assigned counts as well as allocated");
    }

    [TestMethod]
    public void AnIPv6AddressIsPlacedTheSameWay()
    {
        var table = new GeoTable();

        table.Load(Delegations);

        Assert.AreEqual("PT", table.CountryOf("2001:8a0:d7a7:1e00:6b5b:90c1:bca4:c586"));
        Assert.AreEqual("DE", table.CountryOf("2a0a:4cc0:c0:4fc0:54d0:b1ff:fe46:c896"));
    }

    [TestMethod]
    public void WhatIsNotDelegatedHasNoCountry()
    {
        var table = new GeoTable();

        table.Load(Delegations);

        Assert.IsNull(table.CountryOf("10.0.0.5"), "private space is nobody's");
        Assert.IsNull(table.CountryOf("127.0.0.1"), "nor is loopback");
        Assert.IsNull(table.CountryOf("9.9.9.9"), "reserved is not a delegation");
        Assert.IsNull(table.CountryOf("152.52.255.255"), "one below a range is outside it");
        Assert.IsNull(table.CountryOf("not an address"));
        Assert.IsNull(table.CountryOf(null));
    }

    [TestMethod]
    public void AnEmptyTableAnswersNothingRatherThanFailing()
    {
        var table = new GeoTable();

        Assert.AreEqual(0, table.Ranges);
        Assert.IsNull(table.CountryOf("152.53.120.139"), "before anything has been downloaded");
    }

    [TestMethod]
    public void AForwardedCallerIsPlacedByWhatItClaimed()
    {
        var table = new GeoTable();

        table.Load(Delegations);

        // recorded as "<claimed> via <peer>"; the claim is the one being placed
        Assert.AreEqual("GB", table.CountryOf("74.7.227.5 via 152.53.120.139"));
    }

    [TestMethod]
    public void AnIPv4MappedAddressIsPlacedAsIPv4()
    {
        var table = new GeoTable();

        table.Load(Delegations);

        // what a v4 client looks like arriving on a dual stack socket
        Assert.AreEqual("AT", table.CountryOf("::ffff:152.53.120.139"));
    }

    [TestMethod]
    public void WithNoDatabaseNothingIsClaimed()
    {
        using var places = new GeoPlaces();

        Assert.IsFalse(places.Ready, "before anything has been downloaded");
        Assert.IsNull(places.Find("152.53.120.139"));
        Assert.IsNull(places.Find(null));
        Assert.IsNull(places.Find("not an address"));
    }

    [TestMethod]
    public void AMissingOrBrokenDatabaseIsNotAnError()
    {
        using var places = new GeoPlaces();

        // a path that is not there, and one that is there but is not a database
        var rubbish = Path.Combine(Path.GetTempPath(), $"not-a-database-{Guid.NewGuid():n}.mmdb");

        File.WriteAllText(rubbish, "this is not the MaxMind DB format");

        try
        {
            places.Open("/does/not/exist.mmdb", rubbish);

            Assert.IsNull(places.Find("152.53.120.139"), "a server does not stop answering because a database is bad");
        }
        finally
        {
            File.Delete(rubbish);
        }
    }

    [TestMethod]
    public void OneLineOfIt()
    {
        Assert.AreEqual("Aveiro, PT · MEO", new Place("Aveiro", null, "PT", "MEO").Describe());
        Assert.AreEqual("Aveiro, PT", new Place("Aveiro", null, "PT", null).Describe());
        Assert.AreEqual("MEO", new Place(null, null, null, "MEO").Describe());
        Assert.AreEqual("", new Place(null, null, null, null).Describe());

        // a city that shares its name with its region is said once
        Assert.AreEqual("Lisbon, PT · MEO", new Place("Lisbon", "Lisbon", "PT", "MEO").Describe());
    }

    #endregion

    #region The route

    [TestMethod]
    public async Task ARequestIsRecordedWithWhereItCameFrom()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await fixture.GetAsync("/api/v1/system");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var (lines, _, _) = fixture.Book.Read(0, null, LogLevel.Information, 5000);

        var served = lines.LastOrDefault(l => l.Source == "Requests" && l.Text.Contains("/api/v1/system"));

        Assert.IsNotNull(served, "the request the server just answered is in its own log");
        Assert.IsNotNull(served.Client, "and says who it was answering");
        Assert.Contains("200", served.Text);
    }

    [TestMethod]
    public async Task TheAddressIsLeftOffWhenTheInstallationSaysSo()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => WithPanel(o) with { LogClientAddress = false });

        using var response = await fixture.GetAsync("/api/v1/system");

        var (lines, _, _) = fixture.Book.Read(0, null, LogLevel.Information, 5000);

        var served = lines.LastOrDefault(l => l.Source == "Requests" && l.Text.Contains("/api/v1/system"));

        Assert.IsNotNull(served, "the line is still there");
        Assert.IsNull(served.Client, "with nothing personal on it");
    }

    [TestMethod]
    public async Task WhatALambdaPrintsNamesTheVisitorItWasPrintedFor()
    {
        ConsoleTee.Install();

        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var lambda = await fixture.CreateLambdaAsync("traced");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get(() => { Console.WriteLine("who asked for this"); return "done"; });
            """);

        using var called = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/");

        var (lines, _, _) = fixture.Book.Read(0, "traced", LogLevel.Information, 5000);

        var printed = lines.FirstOrDefault(l => l.Text == "who asked for this");

        Assert.IsNotNull(printed);
        Assert.IsNotNull(printed.Client, "a print is attributable to the request that caused it");
    }

    [TestMethod]
    public async Task ReadingTheLogDoesNotFillTheLog()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var first = await Send(fixture, "/api/v1/logs", Token);

        for (var i = 0; i < 5; i++)
        {
            using var poll = await Send(fixture, "/api/v1/logs", Token);
        }

        var (lines, _, _) = fixture.Book.Read(0, null, LogLevel.Information, 50_000);

        Assert.IsFalse(lines.Any(l => l.Source == "Requests" && l.Text.Contains("/api/v1/logs")),
                       "the panel polls every second and a half; a line each would be most of the ring");

        // and a request that is not the log being read is still recorded
        using var other = await fixture.GetAsync("/api/v1/system");

        var (after, _, _) = fixture.Book.Read(0, null, LogLevel.Information, 50_000);

        Assert.IsTrue(after.Any(l => l.Source == "Requests" && l.Text.Contains("/api/v1/system")));
    }

    [TestMethod]
    public async Task WithoutATokenThereIsNoLog()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/logs");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task TheWrongTokenIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var response = await Send(fixture, "/api/v1/logs", "not-the-token");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                        "and it looks the same as having no panel at all");
    }

    [TestMethod]
    public async Task TheLogIsServedToAnOperator()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var book = fixture.Book;

        book.Append("info", "Test", null, "something the server said");

        using var response = await Send(fixture, "/api/v1/logs", Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.GetContentAsync<LogResponse>();

        Assert.IsTrue(body.Lines.Any(l => l.Text == "something the server said"));
        Assert.IsTrue(body.Cursor > 0);
        Assert.AreEqual(fixture.Options.LogHistory, body.Capacity);
    }

    [TestMethod]
    public async Task OnlyOneLambdaCanBeAskedFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        var book = fixture.Book;

        book.Append("info", "stdout", "wanted", "keep this");
        book.Append("info", "stdout", "other", "not this");

        using var response = await Send(fixture, "/api/v1/logs?lambda=wanted", Token);

        var body = await response.GetContentAsync<LogResponse>();

        Assert.AreEqual(1, body.Lines.Count);
        Assert.AreEqual("keep this", body.Lines[0].Text);
    }

    private static async Task<HttpResponseMessage> Send(LambdaFixture fixture, string path, string? token)
    {
        using var request = fixture.Host.GetRequest(path, HttpMethod.Get);

        if (token != null)
        {
            request.Headers.Add("X-Admin-Token", token);
        }

        return await fixture.Host.GetResponseAsync(request);
    }

    #endregion

}
