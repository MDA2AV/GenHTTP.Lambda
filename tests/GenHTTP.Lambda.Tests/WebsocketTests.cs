using System.Net.WebSockets;
using System.Text;

using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// Lambdas that upgrade their connection instead of answering with a document,
/// in each of the three flavours the websocket module offers.
/// </summary>
[TestClass]
public sealed class WebsocketTests
{

    [TestMethod]
    public async Task FunctionalHandlersAnswer()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("functional");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Functional()
                            .OnMessage(async (c, m) => await c.WritePayloadAsync("echo: " + await m.ReadPayloadAsync<string>()));
            """);

        Assert.AreEqual("echo: hello", await RoundtripAsync(fixture, "/lambda/functional/", "hello"));
    }

    [TestMethod]
    public async Task ReactiveHandlersAnswer()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("reactive");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Reactive().Handler(new Echo());

            public class Echo : IReactiveHandler
            {
                public ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;

                public async ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message)
                    => await connection.WritePayloadAsync("echo: " + await message.ReadPayloadAsync<string>());

                public ValueTask OnClose(IReactiveConnection connection, IWebsocketFrame message) => ValueTask.CompletedTask;
            }
            """);

        Assert.AreEqual("echo: hello", await RoundtripAsync(fixture, "/lambda/reactive/", "hello"));
    }

    [TestMethod]
    public async Task ImperativeHandlersAnswerWithoutAnImport()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("imperative");

        // no using for the protocol namespace: FrameType is one of the names
        // every lambda is given, which is what makes this flavour usable
        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Imperative().Handler(new Loop());

            public class Loop : IImperativeHandler
            {
                public async ValueTask HandleAsync(IImperativeConnection connection)
                {
                    while (connection.Request.Server.Running)
                    {
                        var frame = await connection.ReadFrameAsync();

                        if (frame.Type == FrameType.Ping)
                        {
                            await connection.PongAsync();
                        }
                        else if (frame.Type == FrameType.Text)
                        {
                            await connection.WritePayloadAsync("echo: " + await frame.ReadPayloadAsync<string>());
                        }
                        else if (frame.Type == FrameType.Close)
                        {
                            await connection.CloseAsync();
                            break;
                        }
                    }
                }
            }
            """);

        Assert.AreEqual("echo: hello", await RoundtripAsync(fixture, "/lambda/imperative/", "hello"));
    }

    [TestMethod]
    public async Task ConnectionsOutliveTheExecutionTimeout()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { ExecutionTimeout = TimeSpan.FromSeconds(1) });

        var lambda = await fixture.CreateLambdaAsync("longlived");

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Functional()
                            .OnMessage(async (c, m) => await c.WritePayloadAsync(await m.ReadPayloadAsync<string>()));
            """);

        using var client = await ConnectAsync(fixture, "/lambda/longlived/");

        Assert.AreEqual("first", await RoundtripAsync(client, "first"));

        // the timeout bounds a request, and the upgrade is over long before it
        // elapses - what follows belongs to the connection, not to the request
        await Task.Delay(TimeSpan.FromSeconds(2));

        Assert.AreEqual("second", await RoundtripAsync(client, "second"));
    }

    [TestMethod]
    public async Task TheUpgradeRequestCanStillBeRead()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("upgraded");

        // what the guide tells people to do: a browser cannot set a header on
        // the handshake, so whatever the socket needs travels in the query
        await fixture.DeployAsync(lambda.PrivateKey, """
            return Websocket.Functional()
                            .OnMessage(async (c, m) => await c.WritePayloadAsync(c.Request.Header.Query.GetEntry("room") + ": " + await m.ReadPayloadAsync<string>()));
            """);

        Assert.AreEqual("lobby: hello", await RoundtripAsync(fixture, "/lambda/upgraded/?room=lobby", "hello"));
    }

    #region Helpers

    private static async Task<ClientWebSocket> ConnectAsync(LambdaFixture fixture, string path)
    {
        using var probe = fixture.Host.GetRequest(path);

        var address = new UriBuilder(probe.RequestUri!) { Scheme = "ws" }.Uri;

        var client = new ClientWebSocket();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await client.ConnectAsync(address, timeout.Token);

        return client;
    }

    private static async Task<string> RoundtripAsync(LambdaFixture fixture, string path, string message)
    {
        using var client = await ConnectAsync(fixture, path);

        return await RoundtripAsync(client, message);
    }

    private static async Task<string> RoundtripAsync(ClientWebSocket client, string message)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await client.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, timeout.Token);

        var buffer = new byte[4096];

        var received = await client.ReceiveAsync(buffer, timeout.Token);

        return Encoding.UTF8.GetString(buffer, 0, received.Count);
    }

    #endregion

}
