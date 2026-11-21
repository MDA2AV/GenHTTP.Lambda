using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// What the project of a lambda's repository says about the lambda.
/// </summary>
/// <param name="PublicKey">The name of the lambda, which names the project</param>
/// <param name="Address">Where it runs</param>
/// <param name="Home">Where the installation is, which its files link to</param>
/// <param name="License">The license its source is published under, while it is</param>
/// <param name="Installation">What the installation is called, which its files name: GenHTTP Lambda, or what its operator named it</param>
public sealed record RepositoryProject(string PublicKey, string Address, string Home, RepositoryLicense? License, string Installation = Brand.PlatformName);
