namespace GenHTTP.Lambda.Services.Secrets;

/// <summary>
/// One secret, as anybody but the lambda gets to see it: its name and when it
/// was set - never its value.
/// </summary>
/// <param name="Name">What the code reads it by</param>
/// <param name="Created">When it was first stored</param>
/// <param name="Changed">When its value was last replaced</param>
/// <param name="Used">Whether the code reads it (see <see cref="SecretListing.Used"/>)</param>
public sealed record SecretInfo(string Name, DateTime Created, DateTime Changed, bool Used);
