using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

public static class RequestOrigin
{

    /// <summary>
    /// The scheme and host the caller used, so the links handed back can be followed as they are.
    /// </summary>
    /// <remarks>
    /// A proxy in front terminates TLS and says so in its forwarding headers;
    /// without one, the connection itself tells.
    /// </remarks>
    public static string Of(IRequest request)
    {
        var forwarded = request.Header.Headers.GetForwardings().FirstOrDefault();

        var host = forwarded?.Host ?? request.Header.Headers.GetEntry("Host");

        if (string.IsNullOrEmpty(host))
        {
            return string.Empty;
        }

        var protocol = forwarded?.Protocol ?? request.Client.Protocol;

        return $"{(protocol == ClientProtocol.Https ? "https" : "http")}://{host}";
    }

}
