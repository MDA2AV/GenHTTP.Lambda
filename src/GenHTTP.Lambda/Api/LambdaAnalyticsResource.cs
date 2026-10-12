using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Analytics;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// How a lambda's visitors reach it, for its owner.
/// </summary>
/// <remarks>
/// Behind the editor key like everything else about a lambda: what is
/// counted of its visitors is the owner's to see, and nobody else's.
/// Counting is the owner's choice and never starts on its own.
/// </remarks>
public sealed class LambdaAnalyticsResource(IAnalyticsService analytics, ILogger<LambdaAnalyticsResource> logger)
{

    /// <summary>
    /// Whether the lambda's visits are counted, and what was counted over the
    /// last days: by day, page, referrer, country, device, browser, system and
    /// crawler.
    /// </summary>
    /// <remarks>
    /// Counted by day in UTC, and written down every few minutes, so the
    /// visits of the last minutes are not in it yet. A year is kept of every
    /// lambda; a free one shows its owner the last seven days of it, a premium
    /// one all of it.
    /// </remarks>
    /// <param name="days">How many days, today included - left out, as many as its tier shows, which is also the most</param>
    [ResourceMethod("lambdas/:privateKey/analytics")]
    public AnalyticsResponse Get(string privateKey, int? days = null) => AnalyticsResponse.Of(analytics.Get(privateKey, days));

    /// <summary>
    /// Starts counting the lambda's visits, from the next request on.
    /// </summary>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/analytics")]
    public AnalyticsResponse Put(string privateKey)
    {
        var enabled = analytics.Enable(privateKey);

        logger.LogInformation("Switched on analytics of lambda {Lambda}", enabled.PublicKey);

        return AnalyticsResponse.Of(enabled);
    }

    /// <summary>
    /// Stops counting the lambda's visits, and deletes what was counted.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/analytics")]
    public void Delete(string privateKey)
    {
        var publicKey = analytics.Disable(privateKey);

        logger.LogInformation("Switched off analytics of lambda {Lambda}", publicKey);
    }

}
