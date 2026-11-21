namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// A host that is not the platform's: one that belongs to the lambdas, or to
/// nobody here.
/// </summary>
/// <param name="Name">The host, normalized - what is shown and what the log records</param>
public abstract record LambdaHost(string Name);
