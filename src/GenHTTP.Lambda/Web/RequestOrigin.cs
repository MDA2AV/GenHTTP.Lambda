using System.Text;
using System.Web;

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

    /// <summary>
    /// The query of the request, to be passed along with it to where it is
    /// sent on - or nothing, where it has none.
    /// </summary>
    /// <remarks>
    /// The parameters are kept as they were sent, so each is decoded before it
    /// is encoded again - a key encoded twice would be another key.
    /// </remarks>
    public static string Query(IRequest request)
    {
        var query = request.Header.Query;

        if (query.Count == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        for (var i = 0; i < query.Count; i++)
        {
            var entry = query.GetStringEntry(i);

            result.Append(i == 0 ? '?' : '&')
                  .Append(Uri.EscapeDataString(HttpUtility.UrlDecode(entry.Key.ToString())))
                  .Append('=')
                  .Append(Uri.EscapeDataString(HttpUtility.UrlDecode(entry.Value.ToString())));
        }

        return result.ToString();
    }

}
