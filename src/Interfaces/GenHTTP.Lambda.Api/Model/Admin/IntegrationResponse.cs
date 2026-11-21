using GenHTTP.Api.Content.Documentation;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Integrations;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the installation keeps some of its data in: its database, its files or its secrets.")]
public sealed record IntegrationResponse(
    [Summary("What it keeps: `database` - the platform's records, its analytics and the lambdas' databases; `files` - the lambdas' workspaces; `secrets` - their API keys and passwords")]
    string Kind,
    [Summary("What it is called: `SQLite`, `PostgreSQL`, `Local`, `Database`")]
    string Name,
    [Summary("The assembly it is in, which names it: what a host references to use it, such as `GenHTTP.Lambda.Integrations.Sqlite`")]
    string Assembly,
    [Summary("Where it keeps what it keeps, as label and value - never a password, a key or a connection string")]
    IReadOnlyList<IntegrationDetailResponse> Details
)
{

    public static IntegrationResponse Of(IntegrationState state)
        => new(KindOf(state.Info.Kind), state.Info.Name, state.Assembly, [.. state.Info.Details.Select(d => new IntegrationDetailResponse(d.Label, d.Value))]);

    /// <summary>
    /// What a kind of integration is called on the wire, which the panel reads.
    /// </summary>
    private static string KindOf(IntegrationKind kind) => kind switch
    {
        IntegrationKind.Database => "database",
        IntegrationKind.Files => "files",
        IntegrationKind.Secrets => "secrets",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no such kind of integration.")
    };

}
