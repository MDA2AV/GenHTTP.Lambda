using System.Security.Cryptography;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Creates and validates the two keys of a lambda: the public one it is hosted
/// at and the private one its editor is reachable with.
/// </summary>
public static class LambdaKeys
{
    // no vowels (to avoid accidental words) and no characters that look alike
    private const string Alphabet = "bcdfghjkmnpqrstvwxyz23456789";

    private const int MinLength = 3;

    private const int MaxLength = 40;

    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
    {
        "admin", "api", "assets", "create", "editor", "health", "index", "lambda", "new", "static", "www"
    };

    /// <summary>
    /// What every demo is hosted at the start of.
    /// </summary>
    public const string DemoPrefix = "demo-";

    /// <summary>
    /// Why a key for a demo cannot be claimed.
    /// </summary>
    public const string DemoReason = "Keys starting with 'demo-' are kept for the demos of this installation.";

    #region Functionality

    /// <summary>
    /// Whether a normalized key is one only a demo may have.
    /// </summary>
    /// <remarks>
    /// The whole prefix rather than the keys the catalogue holds today, so a
    /// demo added later does not find its key already taken by somebody else.
    /// </remarks>
    public static bool IsDemo(string normalized) => normalized.StartsWith(DemoPrefix, StringComparison.Ordinal);

    /// <summary>
    /// Creates a short, pronounceable key to host a lambda at.
    /// </summary>
    public static string CreatePublicKey() => Create(8);

    /// <summary>
    /// Creates the secret key that grants access to the editor of a lambda.
    /// </summary>
    public static string CreatePrivateKey() => Create(32);

    /// <summary>
    /// Normalizes and validates a key a user asked for.
    /// </summary>
    /// <param name="key">The key as entered by the user</param>
    /// <param name="normalized">The key as it would be stored</param>
    /// <param name="reason">Why the key cannot be used, if it cannot</param>
    public static bool TryNormalize(string? key, out string normalized, out string? reason)
    {
        normalized = (key ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Length < MinLength || normalized.Length > MaxLength)
        {
            reason = $"The key must be between {MinLength} and {MaxLength} characters long.";
            return false;
        }

        foreach (var character in normalized)
        {
            if (character is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-')
            {
                continue;
            }

            reason = "The key may only contain lower case letters, digits and dashes.";
            return false;
        }

        if (normalized[0] == '-' || normalized[^1] == '-')
        {
            reason = "The key must not start or end with a dash.";
            return false;
        }

        if (Reserved.Contains(normalized))
        {
            reason = $"'{normalized}' is reserved by the platform.";
            return false;
        }

        reason = null;
        return true;
    }

    private static string Create(int length)
    {
        return string.Create(length, Alphabet, static (target, alphabet) =>
        {
            for (var i = 0; i < target.Length; i++)
            {
                target[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }

    #endregion

}
