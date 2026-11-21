using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// A host the installation does not answer for: neither one of the
/// platform's names nor a lambda's - a domain pointed at the server before
/// it was given to a lambda or after it was taken off one, or an address
/// typed in place of a name.
/// </summary>
/// <remarks>
/// Only where the installation knows its own name, from its public address;
/// without one, any host it does not know is the platform's.
/// </remarks>
public sealed record UnknownHost(string Name) : LambdaHost(Name);
