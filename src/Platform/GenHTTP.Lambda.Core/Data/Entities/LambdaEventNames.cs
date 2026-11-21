namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a kind of event is called wherever it leaves the platform, and the
/// kinds there are.
/// </summary>
public static class LambdaEventNames
{

    /// <summary>In the order a panel should show them.</summary>
    public static readonly LambdaEvent[] All = [LambdaEvent.Created, LambdaEvent.Saved, LambdaEvent.Deployed, LambdaEvent.Undeployed, LambdaEvent.Deleted];

    public static string Name(LambdaEvent kind) => kind switch
    {
        LambdaEvent.Created => "created",
        LambdaEvent.Saved => "saved",
        LambdaEvent.Deployed => "deployed",
        LambdaEvent.Undeployed => "undeployed",
        LambdaEvent.Deleted => "deleted",
        _ => "unknown"
    };

}
