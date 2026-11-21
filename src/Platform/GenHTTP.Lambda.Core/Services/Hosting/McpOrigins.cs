namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Reads the entries of <c>LambdaOptions.McpOrigins</c>: the names the
/// platform answers at besides its public address, and the pages a browser
/// may drive the MCP endpoint from.
/// </summary>
public static class McpOrigins
{

    /// <summary>
    /// The origin an entry of <c>LAMBDA_MCP_ORIGINS</c> stands for, or nothing
    /// for one that names none.
    /// </summary>
    /// <remarks>
    /// The variable has always named hosts, so a host stands for its page over
    /// HTTPS - the only way the platform serves one - and an entry with a
    /// scheme is taken as the origin it names, a port included. The module
    /// refuses to start with an origin it cannot read, so one that is none - a
    /// wildcard, say, which was never matched either - is left out instead,
    /// with a warning on startup.
    /// </remarks>
    public static string? Of(string entry)
    {
        var origin = IsOrigin(entry) ? entry : $"https://{entry}";

        return IsOrigin(origin) ? origin : null;
    }

    private static bool IsOrigin(string candidate)
        => Uri.TryCreate(candidate, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

}
