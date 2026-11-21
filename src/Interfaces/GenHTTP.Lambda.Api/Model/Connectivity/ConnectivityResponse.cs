using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Connectivity;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Whether a lambda's code may connect to other servers, and where its owner's request for that stands.")]
public sealed record ConnectivityResponse(
    [Summary("Whether its code may connect to other servers now - HttpClient, sockets, a WebSocket client, mail, DNS: the operator allowed it, or its tier does by default and the operator did not decline it. Where it may not, code that does is refused as it compiles, with `LAMBDA0003`.")]
    bool Allowed,
    [Summary("Whether its tier allows it while nothing was decided about the lambda - production does by default; a decision of the operator comes first")]
    bool Included,
    [Summary("Where the request stands: `Requested` while it waits for the operator, `Allowed`, or `Declined` - asked for and turned down, or allowed and taken back. Absent while nobody asked.")]
    string? Status,
    [Summary("Since when it stands so")]
    DateTime? Changed,
    [Summary("What the owner said the lambda connects to, and why")]
    string? Purpose,
    [Summary("What the operator said deciding it, which the owner reads")]
    string? Note
)
{

    public static ConnectivityResponse Of(ConnectivityInfo? connectivity)
    {
        var known = connectivity ?? ConnectivityInfo.None;

        return new(known.Allowed, known.Included, ConnectivityStatusNames.Name(known.Status), known.Changed, known.Purpose, known.Note);
    }

}
