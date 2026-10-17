using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Analytics;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Behind the editor key like everything else about a lambda: what is
/// counted of its visitors is the owner's to see, and nobody else's.
/// </remarks>
[Tag(ApiDocumentation.Analytics)]
[Summary("Who visits a lambda, counted by day: visitors, page views and requests, and where they came from.")]
[Remarks("""
         Counting is the owner's choice and never starts on its own. Once switched on, the visits of a lambda are counted by
         day in UTC: visitors, page views and requests, and the pages, referring sites, countries, devices, browsers, systems
         and crawlers behind them. They are written down every few minutes, so the last minutes are not in an answer yet.
         A year is kept of every lambda; the tier decides how much of it is shown - a free lambda the last seven days, a
         premium one all of it, as the operator set it.

         A **visitor** is a person who opened a page, counted once a day. A **page view** is a page opened, by people or by
         programs - `botViews` are the programs'. Requests count everything. Crawlers are told from people by a maintained
         list, and the big operators' (Google, Bing, OpenAI, Apple, Perplexity) are confirmed by the addresses they publish.

         Nothing that names a visitor is kept: counts only, the referrer as its host, and visitors told apart for a day by a
         hash that is never written down. No cookie, and nothing added to the lambda. The previews of features and the demos
         are not counted.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class LambdaAnalyticsResource(IAnalyticsService analytics, ILogger<LambdaAnalyticsResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/analytics")]
    [Summary("Reads what was counted")]
    [Remarks("""
             By day, and broken down by page, referring site, country, device, browser, system and crawler, with the same number
             of days before for comparison where the tier shows those as well. While counting is switched off, the answer says
             so and has no figures.
             """)]
    [return: Summary("Whether visits are counted, and what was counted")]
    public AnalyticsResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                 [Summary("How many days, today included; left out, as many as the tier shows, which is also the most")] int? days = null)
        => AnalyticsResponse.Of(analytics.Get(privateKey, days));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/analytics")]
    [Summary("Switches counting on")]
    [Remarks("Counting starts with the next request. Nothing happens if it is on already.")]
    [Response(ResponseStatus.Forbidden, "The lambda is a demo, whose visitors are not counted.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Analytics, switched on")]
    public AnalyticsResponse Put([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var enabled = analytics.Enable(privateKey);

        logger.LogInformation("Switched on analytics of lambda {Lambda}", enabled.PublicKey);

        return AnalyticsResponse.Of(enabled);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/analytics")]
    [Summary("Switches counting off and deletes it")]
    [Remarks("**This cannot be undone**: the whole year that was kept is deleted. Switched on again, counting starts from nothing.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    [Response(ResponseStatus.Forbidden, "The lambda is a demo, whose visitors are not counted.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var publicKey = analytics.Disable(privateKey);

        logger.LogInformation("Switched off analytics of lambda {Lambda}", publicKey);
    }

}
