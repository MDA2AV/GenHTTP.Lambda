using System.Net;

using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Execution;

/// <summary>
/// The cap that stops one visitor occupying the whole server.
/// </summary>
/// <remarks>
/// The budget is deliberately tiny here. What is being checked is that a
/// client is cut off at the number it was given and let back in once the
/// second is over - neither of which depends on the number being large.
/// </remarks>
[TestClass]
public sealed class RateLimitTests
{

    [TestMethod]
    public async Task AClientIsCutOffAtItsAllowance()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { RateLimit = 3 });

        var lambda = await fixture.CreateLambdaAsync("busy");

        await fixture.DeployAsync(lambda.PrivateKey);

        var codes = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await fixture.GetAsync("/lambda/busy/");

            codes.Add(response.StatusCode);
        }

        Assert.AreEqual(3, codes.Count(c => c == HttpStatusCode.OK),
                        "exactly the allowance gets through");

        Assert.AreEqual(2, codes.Count(c => c == HttpStatusCode.TooManyRequests),
                        "and everything past it is refused rather than queued");
    }

    [TestMethod]
    public async Task TheAllowanceComesBackTheNextSecond()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { RateLimit = 1 });

        var lambda = await fixture.CreateLambdaAsync("patient");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var first = await fixture.GetAsync("/lambda/patient/"))
        {
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        }

        using (var second = await fixture.GetAsync("/lambda/patient/"))
        {
            Assert.AreEqual(HttpStatusCode.TooManyRequests, second.StatusCode);

            Assert.AreEqual("1", second.Headers.GetValues("Retry-After").Single(),
                            "a client told to come back in a minute for a one second window waits fifty nine too long");
        }

        // the window is a second, which is short enough to simply wait out
        await Task.Delay(TimeSpan.FromSeconds(1.2));

        using (var later = await fixture.GetAsync("/lambda/patient/"))
        {
            Assert.AreEqual(HttpStatusCode.OK, later.StatusCode);
        }
    }

    [TestMethod]
    public async Task TheApiIsNotSubjectToIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { RateLimit = 1 });

        // the cap is on the lambda routes; the editor talking to its own API
        // is not a visitor and must not be throttled out of its own session
        for (var attempt = 0; attempt < 4; attempt++)
        {
            using var response = await fixture.GetAsync("/api/v1/demos");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }

}
