namespace GenHTTP.Lambda.Data.Entities;

/// <summary>The kinds of thing worth recording.</summary>
/// <remarks>
/// Stored as its number, as <see cref="VersionOrigin"/> is; named by
/// <see cref="LambdaEventNames"/>. A number no member has is an event of no
/// kind the platform counts.
/// </remarks>
public enum LambdaEvent
{

    Created = 1,

    Saved = 2,

    Deployed = 3,

    Undeployed = 4,

    Deleted = 5

}
