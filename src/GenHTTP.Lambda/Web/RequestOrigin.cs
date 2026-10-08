using System.Text;

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
    /// Passed on as it was sent: the engines keep each parameter as it arrived,
    /// still encoded, so decoding and encoding it again would only change it -
    /// a plus would become a space, a signature over the query would no longer
    /// match. Only what has no place in an address is encoded, should anything
    /// like it have come along.
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

            result.Append(i == 0 ? '?' : '&');

            AsSent(result, entry.Key.ToString());

            var value = entry.Value.ToString();

            if (value.Length > 0)
            {
                AsSent(result, value.Insert(0, "="));
            }
        }

        return result.ToString();
    }

    private static void AsSent(StringBuilder into, string raw)
    {
        foreach (var character in raw)
        {
            if (character is > ' ' and < '\u007f' and not '#')
            {
                into.Append(character);
            }
            else
            {
                foreach (var part in Encoding.UTF8.GetBytes(character.ToString()))
                {
                    into.Append('%').Append(part.ToString("X2"));
                }
            }
        }
    }

}
