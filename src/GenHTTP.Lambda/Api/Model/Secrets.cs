namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One secret, as anybody may see it: what it is called and when it was set -
/// never what it holds. There is no way to read a value back.
/// </summary>
/// <param name="Name">What the code asks for it by: <c>Secret.Read("NAME")</c></param>
/// <param name="Created">When it was first set</param>
/// <param name="Updated">When its value was last set</param>
public sealed record SecretResponse(string Name, DateTime Created, DateTime Updated);

/// <summary>
/// The secrets of a lambda, or of a feature's copy of them.
/// </summary>
/// <param name="Enabled">Whether they are switched on. Off, there are none, and none can be set</param>
/// <param name="Limit">How many secrets there may be</param>
/// <param name="Secrets">What there is, by name</param>
public sealed record SecretListingResponse(bool Enabled, int Limit, IReadOnlyList<SecretResponse> Secrets);

/// <summary>
/// The value of a secret to set.
/// </summary>
/// <param name="Value">The value. It is kept encrypted and cannot be read back: not here, not in the editor, not by an agent - only by the code of the lambda</param>
public sealed record SecretRequest(string Value);
