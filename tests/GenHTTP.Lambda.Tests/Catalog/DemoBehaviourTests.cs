using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Catalog;

/// <summary>
/// What each demo promises a visitor, taken at its word. A demo that is read
/// to learn how something is done has to actually do it.
/// </summary>
[TestClass]
public sealed class DemoBehaviourTests
{

    [TestMethod]
    public async Task TasksCanBeListedAddedChangedAndRemoved()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var created = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-crud/tasks/",
                                                    new { title = "Buy milk", notes = "", done = false }, "application/json");

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode);

        var task = await Json(created);

        var id = task["id"]!.GetValue<string>();

        using var changed = await fixture.SendAsync(HttpMethod.Put, $"/lambda/demo-crud/tasks/{id}",
                                                    new { title = "Buy oat milk", notes = "", done = true }, "application/json");

        Assert.AreEqual(HttpStatusCode.OK, changed.StatusCode);
        Assert.IsTrue((await Json(changed))["done"]!.GetValue<bool>());

        using var found = await fixture.GetAsync("/lambda/demo-crud/tasks/?search=oat", "application/json");

        var matches = JsonNode.Parse(await found.Content.ReadAsStringAsync())!.AsArray();

        Assert.HasCount(1, matches, "search narrows the list down");

        using var invalid = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-crud/tasks/", new { title = " " }, "application/json");

        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);

        using var removed = await fixture.SendAsync(HttpMethod.Delete, $"/lambda/demo-crud/tasks/{id}");

        Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);

        using var missing = await fixture.GetAsync($"/lambda/demo-crud/tasks/{id}", "application/json");

        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);

        using var specification = await fixture.GetAsync("/lambda/demo-crud/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, specification.StatusCode, "the API describes itself");
    }

    [TestMethod]
    public async Task MembersRegisterSignInAndOut()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var anonymous = await fixture.GetAsync("/lambda/demo-registration/api/me", "application/json");

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode, "no session, no members area");

        using var registered = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-registration/register",
                                                       new { name = "ada", password = "correct horse" }, "application/json");

        Assert.AreEqual(HttpStatusCode.OK, registered.StatusCode);

        var cookie = registered.Headers.GetValues("Set-Cookie").Single();

        StringAssert.StartsWith(cookie, "session=");
        Assert.Contains("HttpOnly", cookie, "no script may read the session");
        Assert.Contains("SameSite=Lax", cookie, "and no other site may send it");
        Assert.DoesNotContain("Path=", cookie, "scoped by the address that set it, which is the root of the lambda");

        Assert.DoesNotContain("session", await registered.Content.ReadAsStringAsync(), "the token travels in the cookie only");

        using var again = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-registration/register",
                                                  new { name = "Ada", password = "something else" }, "application/json");

        Assert.AreEqual(HttpStatusCode.Conflict, again.StatusCode, "names are unique, whatever their case");

        using var wrong = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-registration/login",
                                                  new { name = "ada", password = "wrong password" }, "application/json");

        Assert.AreEqual(HttpStatusCode.Unauthorized, wrong.StatusCode);

        using var login = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-registration/login",
                                                  new { name = "ada", password = "correct horse" }, "application/json");

        var session = login.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        using (var me = await SendWithCookie(fixture, HttpMethod.Get, "/lambda/demo-registration/api/me", session))
        {
            Assert.AreEqual(HttpStatusCode.OK, me.StatusCode);
            Assert.AreEqual("ada", (await Json(me))["name"]!.GetValue<string>(), "the member is injected into the route");
        }

        using (var logout = await SendWithCookie(fixture, HttpMethod.Post, "/lambda/demo-registration/logout", session))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, logout.StatusCode);
            Assert.Contains("Max-Age=0", logout.Headers.GetValues("Set-Cookie").Single(), "the browser is told to forget it");
        }

        using var after = await SendWithCookie(fixture, HttpMethod.Get, "/lambda/demo-registration/api/me", session);

        Assert.AreEqual(HttpStatusCode.Forbidden, after.StatusCode, "a session that was signed out is worth nothing");

        var stored = await fixture.Application.Services.GetRequiredService<IWorkspaceService>()
                                  .ReadAsync((await fixture.Meta.GetIdAsync("demo-registration"))!.Value, "accounts.json");

        Assert.DoesNotContain("correct horse", Encoding.UTF8.GetString(stored!.Content), "passwords are never stored");

        using var members = await fixture.GetAsync("/lambda/demo-registration/members.html");

        Assert.AreEqual(HttpStatusCode.OK, members.StatusCode);
    }

    [TestMethod]
    public async Task FilesCanBeUploadedServedAndRemovedByTheirUploader()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var uploaded = await Upload(fixture, "notes.txt", "hello there");

        Assert.AreEqual(HttpStatusCode.Created, uploaded.StatusCode);

        var answer = await Json(uploaded);

        var upload = answer["upload"]!;

        using var served = await fixture.GetAsync($"/lambda/demo-files/{upload["url"]!.GetValue<string>()}");

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode);
        Assert.AreEqual("hello there", await served.Content.ReadAsStringAsync());
        Assert.AreEqual("nosniff", served.Headers.GetValues("X-Content-Type-Options").Single(), "the browser believes the type it is told");

        using var html = await Upload(fixture, "evil.html", "<script>alert(1)</script>");

        Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, html.StatusCode, "a page from a stranger would run on this address");

        using var large = await Upload(fixture, "large.txt", new string('x', 600 * 1024));

        Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, large.StatusCode);

        var id = upload["id"]!.GetValue<string>();

        using var stranger = await fixture.SendAsync(HttpMethod.Delete, $"/lambda/demo-files/api/files/{id}?key=guess");

        Assert.AreEqual(HttpStatusCode.Forbidden, stranger.StatusCode);

        using var owner = await fixture.SendAsync(HttpMethod.Delete, $"/lambda/demo-files/api/files/{id}?key={answer["key"]!.GetValue<string>()}");

        Assert.AreEqual(HttpStatusCode.NoContent, owner.StatusCode);

        using var listed = await fixture.GetAsync("/lambda/demo-files/api/files", "application/json");

        Assert.HasCount(0, JsonNode.Parse(await listed.Content.ReadAsStringAsync())!.AsArray());
    }

    [TestMethod]
    public async Task VotesAreCountedAndStreamed()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var vote = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-live/api/votes", new { option = "tabs" });

        Assert.AreEqual(HttpStatusCode.NoContent, vote.StatusCode);

        using var bad = await fixture.SendAsync(HttpMethod.Post, "/lambda/demo-live/api/votes", new { option = "neither" });

        Assert.AreEqual(HttpStatusCode.BadRequest, bad.StatusCode);

        using var poll = await fixture.GetAsync("/lambda/demo-live/api/poll", "application/json");

        Assert.AreEqual(1, (await Json(poll))["total"]!.GetValue<int>());

        // the stream opens with the counts as they are
        using var request = fixture.Host.GetRequest("/lambda/demo-live/events");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        using var client = new HttpClient();

        using var stream = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);

        Assert.AreEqual("text/event-stream", stream.Content.Headers.ContentType?.MediaType);

        using var reader = new StreamReader(await stream.Content.ReadAsStreamAsync(timeout.Token));

        var lines = new List<string>();

        while (await reader.ReadLineAsync(timeout.Token) is { } line)
        {
            if (line.StartsWith("data: {", StringComparison.Ordinal))
            {
                lines.Add(line);
                break;
            }
        }

        Assert.HasCount(1, lines, "the counts arrive as the first message, as JSON");
        Assert.Contains("\"total\":1", lines[0]);
    }

    [TestMethod]
    public async Task TheHouseTakesItsTurnAtTicTacToe()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        using var probe = fixture.Host.GetRequest("/lambda/demo-game/play");

        using var socket = new ClientWebSocket();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await socket.ConnectAsync(new UriBuilder(probe.RequestUri!) { Scheme = "ws" }.Uri, timeout.Token);

        Assert.AreEqual("waiting", (await Receive(socket, timeout.Token))["type"]!.GetValue<string>());

        await Send(socket, new { type = "house" }, timeout.Token);

        var start = await Receive(socket, timeout.Token);

        Assert.AreEqual("X", start["you"]!.GetValue<string>());
        Assert.AreEqual(".........", start["cells"]!.GetValue<string>());

        await Send(socket, new { type = "move", cell = 4 }, timeout.Token);

        var answered = await Receive(socket, timeout.Token);

        var cells = answered["cells"]!.GetValue<string>();

        Assert.AreEqual('X', cells[4]);
        Assert.AreEqual(1, cells.Count(c => c == 'O'), "the house answers straight away");
        Assert.AreEqual("X", answered["turn"]!.GetValue<string>());

        // an illegal move is ignored: the next frame is the answer to a legal one
        await Send(socket, new { type = "move", cell = 4 }, timeout.Token);

        var free = cells.IndexOf('.');

        await Send(socket, new { type = "move", cell = free }, timeout.Token);

        var next = await Receive(socket, timeout.Token);

        Assert.AreEqual('X', next["cells"]!.GetValue<string>()[free]);
    }

    #region Helpers

    private static async Task<JsonNode> Json(HttpResponseMessage response)
        => JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

    private static async Task<HttpResponseMessage> SendWithCookie(LambdaFixture fixture, HttpMethod method, string path, string cookie)
    {
        using var request = fixture.Host.GetRequest(path, method);

        request.Headers.Add("Cookie", cookie);
        request.Headers.Add("Accept", "application/json");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static async Task<HttpResponseMessage> Upload(LambdaFixture fixture, string name, string content)
    {
        using var request = fixture.Host.GetRequest($"/lambda/demo-files/api/files?name={Uri.EscapeDataString(name)}", HttpMethod.Post);

        request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        request.Headers.Add("Accept", "application/json");

        return await fixture.Host.GetResponseAsync(request);
    }

    private static Task Send(ClientWebSocket socket, object message, CancellationToken token)
        => socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message), WebSocketMessageType.Text, true, token);

    private static async Task<JsonNode> Receive(ClientWebSocket socket, CancellationToken token)
    {
        var buffer = new byte[4096];

        var received = await socket.ReceiveAsync(buffer, token);

        return JsonNode.Parse(Encoding.UTF8.GetString(buffer, 0, received.Count))!;
    }

    #endregion

}
