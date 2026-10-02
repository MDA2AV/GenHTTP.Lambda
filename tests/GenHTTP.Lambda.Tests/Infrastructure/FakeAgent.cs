using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Answers the way the build agent's runner does (docker/agent/builder.mjs),
/// without a container or a model behind it: jobs are taken, queued and kept
/// by lambda, and a test says how each one ends.
/// </summary>
/// <remarks>
/// What is tested with it is this server's half - what it forwards, what it
/// refuses before forwarding, and what it counts - so the runner is played
/// only as far as those need it.
/// </remarks>
public sealed class FakeAgent : IAsyncDisposable
{
    private readonly HttpListener _listener;
    private readonly Task _loop;

    private readonly ConcurrentDictionary<string, JsonObject> _jobs = [];
    private readonly ConcurrentDictionary<string, string> _latest = [];

    private FakeAgent(HttpListener listener, string url)
    {
        _listener = listener;
        Url = url;
        _loop = Task.Run(ServeAsync);
    }

    /// <summary>Where it listens.</summary>
    public string Url { get; }

    /// <summary>Every job it was asked for, as it was asked.</summary>
    public ConcurrentQueue<JsonObject> Received { get; } = [];

    /// <summary>Every token a request came with.</summary>
    public ConcurrentQueue<string?> Tokens { get; } = [];

    /// <summary>When set, a new job is refused with this status, as a full queue is.</summary>
    public HttpStatusCode? Refuse { get; set; }

    public static FakeAgent Start()
    {
        // a port nobody has, found by asking for any
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        var url = $"http://127.0.0.1:{port}/";

        var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        return new FakeAgent(listener, url);
    }

    /// <summary>Records a step of a job the way the runner does when the agent calls a tool.</summary>
    public void Step(string id, JsonObject step)
    {
        var job = _jobs[id];

        lock (job)
        {
            job["state"] = "running";
            job["waiting"] = 0;

            ((JsonArray)job["steps"]!).Add(step);
        }
    }

    /// <summary>Says how long a job has run and may run, the way the runner counts it.</summary>
    public void Clock(string id, int seconds, int? limit)
    {
        var job = _jobs[id];

        lock (job)
        {
            job["seconds"] = seconds;
            job["limit"] = limit;
        }
    }

    /// <summary>Ends a job the way the runner would once the agent is done.</summary>
    public void Finish(string id, JsonObject result, string state = "done")
    {
        var job = _jobs[id];

        lock (job)
        {
            job["state"] = state;
            job["result"] = result;
            job["waiting"] = 0;
        }
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

            try
            {
                await AnswerAsync(context);
            }
            catch (Exception e)
            {
                await SendAsync(context, HttpStatusCode.InternalServerError, new JsonObject { ["error"] = e.Message });
            }
        }
    }

    private async Task AnswerAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var path = request.Url!.AbsolutePath;

        Tokens.Enqueue(request.Headers["X-Agent-Token"]);

        if (request.HttpMethod == "POST" && path == "/build")
        {
            using var reader = new StreamReader(request.InputStream, Encoding.UTF8);

            var body = (JsonObject)JsonNode.Parse(await reader.ReadToEndAsync())!;

            Received.Enqueue(body);

            if (Refuse is { } refused)
            {
                await SendAsync(context, refused, new JsonObject { ["error"] = "Too many builds waiting. Try again shortly." });
                return;
            }

            var id = Guid.NewGuid().ToString();
            var key = body["key"]?.GetValue<string>();

            var job = new JsonObject
            {
                ["id"] = id,
                ["kind"] = key != null ? "change" : "build",
                ["state"] = "queued",
                ["prompt"] = body["prompt"]?.GetValue<string>(),
                ["deploy"] = body["deploy"]?.GetValue<bool>() ?? true,
                ["model"] = "opus",
                ["before"] = body["before"]?.DeepClone(),
                ["feature"] = body["feature"]?.DeepClone(),
                ["steps"] = new JsonArray(),
                ["waiting"] = 1,
                ["seconds"] = 0,
                ["limit"] = 600
            };

            _jobs[id] = job;

            if (key != null)
            {
                _latest[body["lambda"]!.GetValue<string>()] = id;
            }

            var answer = (JsonObject)job.DeepClone();
            answer["queued"] = 1;

            await SendAsync(context, HttpStatusCode.Accepted, answer);
            return;
        }

        if (request.HttpMethod == "GET" && path.StartsWith("/lambda/", StringComparison.Ordinal))
        {
            if (_latest.TryGetValue(path["/lambda/".Length..], out var id))
            {
                await SendAsync(context, HttpStatusCode.OK, Copy(_jobs[id]));
            }
            else
            {
                await SendAsync(context, HttpStatusCode.NotFound, new JsonObject { ["error"] = "Nothing has been asked of this lambda lately." });
            }

            return;
        }

        if (path.StartsWith("/build/", StringComparison.Ordinal) && _jobs.TryGetValue(path["/build/".Length..], out var found))
        {
            if (request.HttpMethod == "GET")
            {
                await SendAsync(context, HttpStatusCode.OK, Copy(found));
                return;
            }

            if (request.HttpMethod == "DELETE")
            {
                var lambda = request.QueryString["lambda"];

                if (_latest.GetValueOrDefault(lambda ?? "") != found["id"]!.GetValue<string>())
                {
                    await SendAsync(context, HttpStatusCode.NotFound, new JsonObject { ["error"] = "No such build." });
                    return;
                }

                Finish(found["id"]!.GetValue<string>(), new JsonObject { ["ok"] = false, ["cancelled"] = true }, "cancelled");

                await SendAsync(context, HttpStatusCode.OK, Copy(found));
                return;
            }
        }

        await SendAsync(context, HttpStatusCode.NotFound, new JsonObject { ["error"] = "No such thing." });
    }

    private static JsonObject Copy(JsonObject job)
    {
        lock (job)
        {
            return (JsonObject)job.DeepClone();
        }
    }

    private static async Task SendAsync(HttpListenerContext context, HttpStatusCode status, JsonObject body)
    {
        var bytes = Encoding.UTF8.GetBytes(body.ToJsonString());

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = bytes.Length;

        await context.Response.OutputStream.WriteAsync(bytes);

        context.Response.Close();
    }

    public async ValueTask DisposeAsync()
    {
        _listener.Stop();
        _listener.Close();

        await _loop;
    }

}
