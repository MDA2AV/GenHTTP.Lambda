using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Sets what the sandbox and production may have and are called, the way a test needs it.
/// </summary>
public static class TierOptions
{

    /// <summary>
    /// The options with other sizes for the sandbox and for production - what
    /// is not given stays as it was.
    /// </summary>
    /// <param name="build">What a version of a sandbox lambda may come to</param>
    /// <param name="productionBuild">What a version of a lambda in production may come to</param>
    /// <param name="data">What the data of a sandbox lambda may take</param>
    /// <param name="productionData">What the data of a lambda in production may take</param>
    public static LambdaOptions WithSizes(this LambdaOptions options, long? build = null, long? productionBuild = null, long? data = null, long? productionData = null)
        => options with
        {
            Sandbox = options.Sandbox with { BuildBytes = build ?? options.Sandbox.BuildBytes, DataBytes = data ?? options.Sandbox.DataBytes },
            Production = options.Production with { BuildBytes = productionBuild ?? options.Production.BuildBytes, DataBytes = productionData ?? options.Production.DataBytes }
        };

    /// <summary>
    /// The options with labels for the sandbox and production, as a host may
    /// call its tiers on its pages.
    /// </summary>
    public static LambdaOptions WithLabels(this LambdaOptions options, string sandbox, string production)
        => options with
        {
            Sandbox = options.Sandbox with { Label = sandbox },
            Production = options.Production with { Label = production }
        };

}
