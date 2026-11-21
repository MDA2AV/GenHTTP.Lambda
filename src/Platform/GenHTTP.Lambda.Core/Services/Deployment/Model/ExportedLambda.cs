namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// What an exported project says about the lambda it came from.
/// </summary>
/// <param name="PublicKey">The name of the lambda, which names the project</param>
/// <param name="Version">The version being exported</param>
/// <param name="Saved">When that version was saved</param>
/// <param name="Change">What that version changed, in a line</param>
/// <param name="Address">Where the lambda answers - its domain while it has one, its address below the hosting domain otherwise</param>
/// <param name="Exported">When the export was made, or nothing for a project packed to be published</param>
/// <param name="Secrets">The names of the secrets it keeps or reads, which become environment variables - never their values</param>
/// <param name="License">The license its source is published under, if its owner published it</param>
public sealed record ExportedLambda(string PublicKey, int Version, DateTime Saved, string? Change, string? Address, DateTime? Exported,
                                    IReadOnlyList<string>? Secrets = null, ExportedLicense? License = null);
