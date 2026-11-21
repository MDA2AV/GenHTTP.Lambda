using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Which of the platform's features a lambda uses, as far as the platform's
/// database tells - what the operator would otherwise open the editor for.
/// </summary>
/// <param name="AnalyticsSince">Since when its visits are counted, or nothing while they are not</param>
/// <param name="Showcase">Whether its owner put it in the showcase</param>
/// <param name="Source">Whether its owner published its code</param>
/// <param name="Drafts">How many features (drafts) it has open</param>
/// <param name="View">How its editor opens: the simple view for an owner who does not write code</param>
public sealed record LambdaUses(
    bool Database,
    bool Workspace,
    bool Secrets,
    DateTime? AnalyticsSince,
    bool Showcase,
    bool Source,
    int Drafts,
    EditorView View
);
