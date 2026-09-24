using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Web;

/// <summary>
/// Arriving from a link on another page, the way a "try this online" button
/// does it.
/// </summary>
/// <remarks>
/// This used to create the lambda as the link was followed. It does not any
/// more, and most of what is asserted here is that it does not: a GET that
/// creates something is followed by every crawler and every link preview, none
/// of which has agreed to anything.
/// </remarks>
[TestClass]
public sealed class InvitationTests
{
    private const string Token = "a-token-nobody-would-guess";

    private static Func<LambdaOptions, LambdaOptions> WithPanel => o => o with { AdminToken = Token };

    [TestMethod]
    public async Task ALinkLandsInTheEditor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/start", "text/html");

        Assert.AreEqual(HttpStatusCode.SeeOther, response.StatusCode);

        var location = response.Headers.Location?.ToString();

        Assert.IsNotNull(location);
        Assert.StartsWith("/editor/create", location, "creating one is the visitor's decision to make");
        Assert.Contains("invited=1", location, "the editor has to know it is talking to somebody who has seen nothing yet");
    }

    [TestMethod]
    public async Task FollowingTheLinkCreatesNothing()
    {
        await using var fixture = await LambdaFixture.CreateAsync(WithPanel);

        using var invited = await fixture.GetAsync("/start?template=websocket-reactive", "text/html");

        Assert.AreEqual(HttpStatusCode.SeeOther, invited.StatusCode);

        // the panel is the only thing that can see every lambda, so it is what
        // can say that none of them appeared
        using var response = await Send(fixture, "/api/v1/admin/lambdas");

        var listing = await response.GetContentAsync<AdminListingResponse>();

        Assert.AreEqual(0, listing.Total, "a link that is merely followed must not leave a lambda behind");
    }

    [TestMethod]
    public async Task ATemplateCanBeLinkedTo()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/start?template=websocket-reactive", "text/html");

        var location = response.Headers.Location?.ToString();

        Assert.IsNotNull(location);
        Assert.Contains("template=websocket-reactive", location,
                        "the editor is what seeds the code, so the choice has to survive the redirect");
    }

    [TestMethod]
    public async Task AnUnknownTemplateStillLandsInTheEditor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/start?template=nope", "text/html");

        Assert.AreEqual(HttpStatusCode.SeeOther, response.StatusCode,
                        "a link outlives the name it mentions, and an error page is a worse answer than the editor");

        var location = response.Headers.Location?.ToString();

        Assert.IsNotNull(location);
        Assert.DoesNotContain("template=", location, "and it must not carry a name nothing answers to");
    }

    [TestMethod]
    public async Task TheCatalogueSaysWhichTemplatesAreHidden()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/system");

        var body = await response.Content.ReadAsStringAsync();

        var platform = await response.GetContentAsync<PlatformResponse>();

        var offered = platform.Templates.SelectMany(g => g.Templates).ToList();

        Assert.IsNotEmpty(offered, "the assistant has to have something to offer");

        // every one is described, hidden or not, because an editor reached by a
        // link that names one has to be able to say what it is. Which of them
        // the picker leaves out is the editor's decision, and it can only make
        // it if the catalogue tells it - so the flag has to be on the wire.
        Assert.Contains("\"hidden\"", body, "the editor filters the picker on this, so it has to be published");

        foreach (var template in offered)
        {
            using var linked = await fixture.GetAsync($"/start?template={template.Id}", "text/html");

            Assert.AreEqual(HttpStatusCode.SeeOther, linked.StatusCode);
            Assert.Contains($"template={template.Id}", linked.Headers.Location?.ToString() ?? string.Empty,
                            "a template that exists can be linked to, listed or not");
        }
    }

    private static async Task<HttpResponseMessage> Send(LambdaFixture fixture, string path)
    {
        using var request = fixture.Host.GetRequest(path, HttpMethod.Get);

        request.Headers.Add("X-Admin-Token", Token);

        return await fixture.Host.GetResponseAsync(request);
    }

}
