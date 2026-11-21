namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What an ending is called wherever it leaves the platform.
/// </summary>
public static class ActivationEndingNames
{

    public static string? Name(ActivationEnding? ending) => ending switch
    {
        ActivationEnding.Replaced => "replaced",
        ActivationEnding.Stopped => "stopped",
        ActivationEnding.Expired => "expired",
        ActivationEnding.Admin => "admin",
        _ => null
    };

}
