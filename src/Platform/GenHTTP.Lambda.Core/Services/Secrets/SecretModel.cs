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

/// <summary>
/// The secrets of a lambda, or of a feature's copy of them.
/// </summary>
/// <param name="Enabled">Whether the lambda has secrets switched on</param>
/// <param name="Secrets">What is stored, by name</param>
/// <param name="Used">
/// Every name the code reads with <c>Secret.Read("...")</c> or <c>Secret.Exists("...")</c>
/// - the code online and the newest version, or the feature's own - whether it
/// is stored or not. A name read here and missing above is a value somebody
/// still has to give.
/// </param>
/// <param name="Missing">The names the code reads that are not stored - and are not asked about with Exists first, so reading them fails</param>
/// <param name="Optional">The names the code asks about with Exists that are not stored - it does without them</param>
/// <param name="Limit">How many secrets there may be</param>
public sealed record SecretListing(bool Enabled, IReadOnlyList<SecretInfo> Secrets, IReadOnlyList<string> Used, IReadOnlyList<string> Missing,
                                   IReadOnlyList<string> Optional, int Limit);
