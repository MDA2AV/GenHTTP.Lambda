namespace GenHTTP.Lambda.Data.Entities;

/// <summary>The kinds of thing worth recording.</summary>
public static class LambdaEvents
{

    public const string Created = "created";

    public const string Saved = "saved";

    public const string Deployed = "deployed";

    public const string Undeployed = "undeployed";

    public const string Deleted = "deleted";

    /// <summary>In the order a panel should show them.</summary>
    public static readonly string[] All = [Created, Saved, Deployed, Undeployed, Deleted];

}
