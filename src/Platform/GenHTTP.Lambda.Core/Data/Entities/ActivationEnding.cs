namespace GenHTTP.Lambda.Data.Entities;

/// <summary>The ways a stretch of being online ends.</summary>
/// <remarks>
/// Stored as its number, as <see cref="VersionOrigin"/> is; named by
/// <see cref="ActivationEndingNames"/>. A number no member has reads as not
/// recorded.
/// </remarks>
public enum ActivationEnding
{

    /// <summary>Another deployment took its place.</summary>
    Replaced = 1,

    /// <summary>The owner took it offline.</summary>
    Stopped = 2,

    /// <summary>Nobody used it for long enough and the sweep took it down.</summary>
    Expired = 3,

    /// <summary>The operator took it offline.</summary>
    Admin = 4

}
