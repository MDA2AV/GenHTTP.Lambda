using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// One of the platform's features a lambda may use, to narrow the listing to.
/// </summary>
public enum LambdaFeature
{

    Database,

    Workspace,

    Secrets,

    Analytics,

    Showcase,

    Source,

    Domain,

    /// <summary>It has a feature (a draft) open.</summary>
    Drafts,

    /// <summary>Its code may connect to other servers, whether its tier includes it or the operator allowed it.</summary>
    Connects,

    /// <summary>Its owner asked to connect to other servers, and waits for a decision.</summary>
    Requested

}
