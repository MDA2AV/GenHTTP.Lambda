using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Stands in for the lambda the usage reports are sent to
/// (telemetry.genhttp.run), which is outside this process: it keeps what it
/// was sent and answers as a test tells it to.
/// </summary>
public sealed class FakeCollector : IAsyncDisposable
{
    private readonly HttpListener _listener;
    private readonly Task _loop;

    private FakeCollector(HttpListener listener, string url)
    {
        _listener = listener;
        Url = url;
        _loop = Task.Run(ServeAsync);
    }

    /// <summary>Where it takes reports.</summary>
    public string Url { get; }

    /// <summary>Every report it was sent, as it was sent.</summary>
    public ConcurrentQueue<Received> Reports { get; } = [];

    /// <summary>What it answers with.</summary>
    public HttpStatusCode Answer { get; set; } = HttpStatusCode.NoContent;

    /// <param name="Method">The method of the request</param>
    /// <param name="ContentType">What the request said its body is</param>
    /// <param name="UserAgent">Who the request said it came from</param>
    /// <param name="Body">The body, as text</param>
    public sealed record Received(string Method, string? ContentType, string? UserAgent, string Body);

    public static FakeCollector Start()
    {
        var url = $"http://127.0.0.1:{FreePort()}/api/reports/";

        var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        return new FakeCollector(listener, url);
    }

    /// <summary>
    /// A port nobody has, found by asking for any.
    /// </summary>
    public static int FreePort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();

        var port = ((IPEndPoint)probe.LocalEndpoint).Port;

        probe.Stop();

        return port;
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception)
            {
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);

            var body = await reader.ReadToEndAsync();

            Reports.Enqueue(new Received(context.Request.HttpMethod, context.Request.ContentType, context.Request.UserAgent, body));

            context.Response.StatusCode = (int)Answer;
            context.Response.Close();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _listener.Stop();
        _listener.Close();

        await _loop;
    }

}
