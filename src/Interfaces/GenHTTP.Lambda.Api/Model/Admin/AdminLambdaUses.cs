using GenHTTP.Api.Content.Documentation;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Which of the platform's features a lambda uses, as far as the platform's database tells.")]
public sealed record AdminLambdaUses(
    [Summary("Whether its database is switched on")]
    bool Database,
    [Summary("Whether its workspace is switched on")]
    bool Workspace,
    [Summary("Whether its secrets are switched on")]
    bool Secrets,
    [Summary("Since when its visits are counted; absent while they are not")]
    DateTime? AnalyticsSince,
    [Summary("Whether its owner put it in the showcase")]
    bool Showcase,
    [Summary("Whether its owner published its code at `/source`")]
    bool Source,
    [Summary("How many features (drafts) it has open")]
    int Drafts,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Simple`, for an owner who does not write code, or `Full`")]
    string View
)
{

    public static AdminLambdaUses Of(LambdaUses uses)
        => new(uses.Database, uses.Workspace, uses.Secrets, uses.AnalyticsSince, uses.Showcase, uses.Source, uses.Drafts, EditorViewNames.Name(uses.View));

}
