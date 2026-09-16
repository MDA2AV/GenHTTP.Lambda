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
