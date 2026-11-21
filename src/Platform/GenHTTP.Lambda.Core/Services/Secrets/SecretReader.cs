namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// What the secrets class generated into a lambda calls, once the lambda is
/// loaded: a secret by its name, and whether the caller insists on it.
/// </summary>
/// <remarks>
/// Both forms, since the lambda keeps both: <c>Secret.ReadAsync</c>, which is
/// what agents are taught, and <c>Secret.Read</c>, which code written before
/// calls - and which waits where the integration does not answer at once.
/// </remarks>
/// <param name="Read">The plain call: <c>Secret.Read</c>, <c>Secret.Exists</c></param>
/// <param name="ReadAsync">The awaited one: <c>Secret.ReadAsync</c>, <c>Secret.ExistsAsync</c></param>
public sealed record SecretReader(Func<string, bool, string?> Read, Func<string, bool, CancellationToken, ValueTask<string?>> ReadAsync);
