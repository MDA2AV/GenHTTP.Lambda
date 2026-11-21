namespace GenHTTP.Lambda.Server;

/// <summary>
/// Asks the server running beside it whether it answers - the health check
/// of an image that has no shell to ask with.
/// </summary>
/// <remarks>
/// <para>
/// The images are chiseled: no shell, no curl, nothing but the runtime and
/// the application. So the application asks itself. A host checks for
/// <see cref="Argument"/> before it builds anything and, given it, starts no
/// platform and no server: it sends one request to the port the server
/// listens on and exits with what a container runtime reads as healthy or
/// not - <c>HEALTHCHECK CMD ["./GenHTTP.Lambda.Community", "--health"]</c>.
/// </para>
/// <para>
/// Any answer below a server error counts: the server is up when it answers.
/// A redirect is what a plain request gets where TLS is set up, and a 401 what
/// it gets from a host that puts the platform behind a login - neither says
/// anything is wrong. The request names <c>localhost</c> as its host, which
/// the platform answers to whatever <c>LAMBDA_PUBLIC_URL</c> says.
/// </para>
/// </remarks>
public static class HealthProbe
{
    /// <summary>
    /// What a host is called with to check the server instead of running one.
    /// </summary>
    public const string Argument = "--health";

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Whether a host was called to check the server rather than to run it.
    /// </summary>
    public static bool IsRequested(string[] args) => args is [Argument];

    /// <summary>
    /// Asks the server on the port of these settings for the system's
    /// description, and returns the exit code that says whether it answered:
    /// 0 if it did, 1 if it did not, did not in time or failed.
    /// </summary>
    public static async Task<int> CheckAsync(ServerSettings settings, CancellationToken cancellation = default)
    {
        using var patience = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

        patience.CancelAfter(Patience);

        try
        {
            using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false, UseProxy = false });

            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{settings.Port}/api/v1/system");

            request.Headers.Host = "localhost";

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, patience.Token);

            return (int)response.StatusCode < 500 ? 0 : 1;
        }
        catch (Exception e) when (e is HttpRequestException or OperationCanceledException)
        {
            return 1;
        }
    }

}
