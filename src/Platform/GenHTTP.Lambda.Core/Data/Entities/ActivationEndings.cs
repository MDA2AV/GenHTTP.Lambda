namespace GenHTTP.Lambda.Data.Entities;

/// <summary>The ways a stretch of being online ends.</summary>
public static class ActivationEndings
{

    /// <summary>Another deployment took its place.</summary>
    public const string Replaced = "replaced";

    /// <summary>The owner took it offline.</summary>
    public const string Stopped = "stopped";

    /// <summary>Nobody used it for long enough and the sweep took it down.</summary>
    public const string Expired = "expired";

    /// <summary>The operator took it offline.</summary>
    public const string Admin = "admin";

}
