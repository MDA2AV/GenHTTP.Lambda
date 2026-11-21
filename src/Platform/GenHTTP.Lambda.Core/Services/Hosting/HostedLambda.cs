namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// A subdomain of the hosting domain, which is the address of the lambda
/// whose public key it is named after - if there is one.
/// </summary>
/// <param name="Label">What is in front of the hosting domain, a key only where a key could be what it says</param>
public sealed record HostedLambda(string Name, string Label) : LambdaHost(Name);
