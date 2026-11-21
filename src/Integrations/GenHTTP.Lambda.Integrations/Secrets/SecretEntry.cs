namespace GenHTTP.Lambda.Integrations.Secrets;

/// <summary>
/// A secret, as anybody but its lambda sees it: by its name.
/// </summary>
/// <param name="Created">When it was first stored, in UTC</param>
/// <param name="Changed">When its value was last replaced, in UTC</param>
public sealed record SecretEntry(string Name, DateTime Created, DateTime Changed);
