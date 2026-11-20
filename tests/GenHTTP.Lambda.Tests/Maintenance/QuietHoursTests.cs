using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Maintenance;

/// <summary>
/// When the installation, and each lambda, is usually quiet: counted from
/// the requests by the hour of the day, kept as a typical day, and read by
/// the operator in the panel.
/// </summary>
[TestClass]
public sealed class QuietHoursTests
{

    [TestMethod]
    public async Task EveryRequestIsCountedForTheInstallationAndALambdasForTheLambda()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("counted");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"busy\");");

        for (var i = 0; i < 5; i++)
        {
            using var visit = await fixture.GetAsync("/lambda/counted/");

            Assert.AreEqual(HttpStatusCode.OK, visit.StatusCode);
        }

        // the requests are written down by the hour: nothing is read before
        await fixture.Platform.Services.GetRequiredService<IQuietHoursService>().FlushAsync(DateTime.UtcNow);

        var installation = await InstallationAsync(fixture);

        // by the hour they were made in, which a test may straddle
        Assert.HasCount(24, installation.Hours);
        Assert.IsGreaterThan(5.0, installation.Hours.Sum(), "the lambda's requests, and the API's that made it");
        Assert.IsFalse(installation.Known, "a moment is no typical day");

        var detail = await DetailAsync(fixture, "counted");

        Assert.IsNotNull(detail.TypicalDay);
        Assert.AreEqual(5.0, detail.TypicalDay.Hours.Sum(), 0.01, "only the requests it answered, as requests on a typical day");
        Assert.IsFalse(detail.TypicalDay.Known);
    }

    [TestMethod]
    public async Task ALambdaNobodyVisitedHasNoQuietHoursOfItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        await fixture.CreateLambdaAsync("unvisited");

        await fixture.Platform.Services.GetRequiredService<IQuietHoursService>().FlushAsync(DateTime.UtcNow);

        var detail = await DetailAsync(fixture, "unvisited");

        Assert.IsNotNull(detail.TypicalDay);
        Assert.IsFalse(detail.TypicalDay.Known);
        Assert.IsNull(detail.TypicalDay.Since, "nothing was counted");
        Assert.AreEqual(0.0, detail.TypicalDay.Hours.Sum());
    }

    [TestMethod]
    public async Task TheQuietHoursAreKnownOnceEveryHourOfADayWasCounted()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var start = ThisHour();

        // busy in the day, quiet from 02:00 to 05:00
        await LiveAsync(fixture, start, TimeSpan.FromHours(20), hour => hour is >= 2 and < 5 ? 1 : 40);

        var learning = await InstallationAsync(fixture, start.AddHours(20));

        Assert.IsFalse(learning.Known, "not every hour of the day was counted yet");

        await LiveAsync(fixture, start.AddHours(20), TimeSpan.FromDays(2), hour => hour is >= 2 and < 5 ? 1 : 40);

        var known = await InstallationAsync(fixture, start.AddDays(2).AddHours(20));

        Assert.IsTrue(known.Known);
        Assert.AreEqual(2, known.QuietFrom);
        Assert.AreEqual(3, known.QuietLength);
        Assert.IsLessThan(0.01, known.QuietShare, "three requests of more than eight hundred");

        // read as requests on a typical day, however few days were counted
        Assert.AreEqual(40.0, known.Hours[12], 2.0);
        Assert.AreEqual(1.0, known.Hours[3], 0.2);
    }

    [TestMethod]
    public async Task TheQuietHoursFollowTheTrafficAsItChanges()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var start = ThisHour();

        // a week quiet at night, then three weeks quiet in the afternoon
        await LiveAsync(fixture, start, TimeSpan.FromDays(7), hour => hour is >= 2 and < 5 ? 0 : 30);

        Assert.AreEqual(2, (await InstallationAsync(fixture, start.AddDays(7))).QuietFrom);

        await LiveAsync(fixture, start.AddDays(7), TimeSpan.FromDays(21), hour => hour is >= 14 and < 17 ? 0 : 30);

        var moved = await InstallationAsync(fixture, start.AddDays(28));

        Assert.AreEqual(14, moved.QuietFrom, "what was a week ago counts half as much as today");
    }

    [TestMethod]
    public async Task ABurstIsNotTakenForTheBusiestHourForLong()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var start = ThisHour();

        // quiet from 03:00 to 06:00, and once a crawler asking for
        // everything at 03:00
        await LiveAsync(fixture, start, TimeSpan.FromDays(3), hour => hour is >= 3 and < 6 ? 5 : 20);

        var burst = new DateTime(start.Year, start.Month, start.Day, 3, 0, 0, DateTimeKind.Utc).AddDays(4);

        await LiveAsync(fixture, start.AddDays(3), burst - start.AddDays(3), hour => hour is >= 3 and < 6 ? 5 : 20);
        await LiveAsync(fixture, burst, TimeSpan.FromHours(1), hour => 1000);

        Assert.IsFalse((await InstallationAsync(fixture, burst.AddHours(1))).IsQuiet(3), "the hour it happened in");

        await LiveAsync(fixture, burst.AddHours(1), TimeSpan.FromDays(35), hour => hour is >= 3 and < 6 ? 5 : 20);

        Assert.AreEqual(3, (await InstallationAsync(fixture, burst.AddDays(35).AddHours(1))).QuietFrom, "five weeks later");
    }

    [TestMethod]
    public async Task ALambdasQuietHoursAreItsOwn()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("office");

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var start = ThisHour();

        // the installation is quiet at night, the lambda - used from another
        // part of the world - in the morning
        await LiveAsync(fixture, start, TimeSpan.FromDays(3), hour => hour is >= 1 and < 4 ? 2 : 50,
                        (counts, hour) =>
                        {
                            for (var i = 0; i < (hour is >= 8 and < 11 ? 0 : 10); i++)
                            {
                                counts.CountLambda(id, hour);
                            }
                        });

        var service = fixture.Platform.Services.GetRequiredService<IQuietHoursService>();

        var now = start.AddDays(3);

        Assert.AreEqual(1, (await service.OfInstallationAsync(now)).QuietFrom);

        var own = await service.OfLambdaAsync(id, now);

        Assert.IsTrue(own.Known);
        Assert.AreEqual(8, own.QuietFrom);
        Assert.AreEqual(0.0, own.QuietShare, 0.0001);
    }

    /// <summary>
    /// The job writes in the middle of an hour, so a lambda's first write
    /// holds the end of one hour and the start of the next - neither of which
    /// is taken for an hour nobody asked for anything in.
    /// </summary>
    [TestMethod]
    public async Task AnHourWrittenDownInTwoPartsIsReadWhole()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("half");

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var counts = fixture.Platform.Services.GetRequiredService<HourlyCounts>();

        var service = fixture.Platform.Services.GetRequiredService<IQuietHoursService>();

        var hour = ThisHour();

        await service.FlushAsync(hour.AddMinutes(30));

        // its first visitors, in the second half of one hour and the first of the next
        for (var i = 0; i < 10; i++)
        {
            counts.CountLambda(id, hour.Hour);
            counts.CountLambda(id, hour.AddHours(1).Hour);
        }

        var written = hour.AddHours(1).AddMinutes(30);

        await service.FlushAsync(written);

        var day = await service.OfLambdaAsync(id, written);

        // read as added when it was over, which it was half an hour before
        // the write - a fraction of a percent
        Assert.AreEqual(10.0, day.Hours[hour.Hour], 0.1, "the hour before the write");
        Assert.AreEqual(10.0, day.Hours[hour.AddHours(1).Hour], 0.1, "the hour of the write");
    }

    [TestMethod]
    public async Task ARemovedLambdaTakesItsQuietHoursWithIt()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync(LambdaFixture.WithPanel);

        var lambda = await fixture.CreateLambdaAsync("gone");

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(() => \"soon gone\");");

        using (var visit = await fixture.GetAsync("/lambda/gone/"))
        {
            Assert.AreEqual(HttpStatusCode.OK, visit.StatusCode);
        }

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        var service = fixture.Platform.Services.GetRequiredService<IQuietHoursService>();

        await service.FlushAsync(DateTime.UtcNow);

        Assert.IsNotNull((await service.OfLambdaAsync(id, DateTime.UtcNow)).Since);

        using (var removed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);
        }

        var databases = fixture.Platform.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        Assert.IsFalse(await database.QuietHours.AnyAsync(q => q.LambdaId == id));
        Assert.IsTrue(await database.QuietHours.AnyAsync(q => q.LambdaId == 0), "the installation's stays");
    }

    #region Helpers

    /// <summary>
    /// Lets the server live through a stretch of time an hour at a time, as
    /// the job would write it down: what each hour is asked for is counted,
    /// and written down as the hour is over.
    /// </summary>
    /// <remarks>
    /// The counts are made as the concern makes them, by the hour of the day
    /// - the requests themselves cannot be sent in another hour than now.
    /// </remarks>
    private static async Task LiveAsync(LambdaFixture fixture, DateTime from, TimeSpan stretch, Func<int, int> requests, Action<HourlyCounts, int>? lambdas = null)
    {
        var counts = fixture.Platform.Services.GetRequiredService<HourlyCounts>();

        var service = fixture.Platform.Services.GetRequiredService<IQuietHoursService>();

        for (var hour = from; hour < from + stretch; hour = hour.AddHours(1))
        {
            for (var i = 0; i < requests(hour.Hour); i++)
            {
                counts.CountInstallation(hour.Hour);
            }

            lambdas?.Invoke(counts, hour.Hour);

            await service.FlushAsync(hour.AddMinutes(59));
        }
    }

    /// <summary>
    /// The beginning of this hour, which the fixture began counting in: a
    /// day simulated from then on is one the server counted all of.
    /// </summary>
    private static DateTime ThisHour()
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
    }

    private static async Task<TypicalDay> InstallationAsync(LambdaFixture fixture, DateTime? now = null)
    {
        if (now is { } at)
        {
            return await fixture.Platform.Services.GetRequiredService<IQuietHoursService>().OfInstallationAsync(at);
        }

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/quiet-hours");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<TypicalDay>();
    }

    private static async Task<AdminLambdaDetail> DetailAsync(LambdaFixture fixture, string publicKey)
    {
        using var response = await fixture.GetAsOperatorAsync($"/api/v1/admin/lambdas/{publicKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return await response.GetContentAsync<AdminLambdaDetail>();
    }

    #endregion

}
