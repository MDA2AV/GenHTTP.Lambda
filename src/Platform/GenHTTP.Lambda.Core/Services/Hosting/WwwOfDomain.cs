using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// <c>www.</c> in front of a domain of its own that its owner entered
/// without it, which sends its visitors on to that domain.
/// </summary>
/// <param name="Domain">The domain it is in front of, the one the lambda answers at</param>
public sealed record WwwOfDomain(string Name, string Domain) : LambdaHost(Name);
