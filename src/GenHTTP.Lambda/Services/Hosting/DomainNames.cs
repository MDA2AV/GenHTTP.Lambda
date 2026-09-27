using System.Globalization;
using System.Net;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Turns what somebody typed as a domain into the form it is stored and
/// matched in, or says why it cannot be one.
/// </summary>
/// <remarks>
/// The stored form is the one a Host header is reduced to before the lookup:
/// lower case, no port, no trailing dot, and an internationalized name in its
/// ASCII form. Both sides going through <see cref="Normalize"/> is what makes
/// the lookup a plain comparison.
/// </remarks>
public static class DomainNames
{
    private const int MaxLength = 253;

    private const int MaxLabel = 63;

    private static readonly IdnMapping Idn = new();

    #region Functionality

    /// <summary>
    /// Normalizes and validates a domain an owner asked for.
    /// </summary>
    /// <param name="input">The domain as it was entered - a pasted address is forgiven its scheme and path</param>
    /// <param name="options">Which hosts belong to the platform itself and may not be claimed</param>
    /// <param name="normalized">The domain as it would be stored</param>
    /// <param name="reason">Why it cannot be used, if it cannot</param>
    public static bool TryNormalize(string? input, LambdaOptions options, out string normalized, out string? reason)
    {
        normalized = Strip(input ?? string.Empty);

        if (normalized.Length == 0)
        {
            reason = "Enter a domain, such as shop.example.com.";
            return false;
        }

        if (IPAddress.TryParse(normalized.Trim('[', ']'), out _))
        {
            reason = "That is an address, not a domain. Enter the name that points to it.";
            return false;
        }

        try
        {
            // what a browser sends for a name written in any other script
            normalized = Idn.GetAscii(normalized).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            reason = "That is not a valid domain.";
            return false;
        }

        if (normalized.Length > MaxLength)
        {
            reason = $"A domain can be {MaxLength} characters at most.";
            return false;
        }

        var labels = normalized.Split('.');

        if (labels.Length < 2)
        {
            reason = "Enter the full domain, including its ending - such as example.com.";
            return false;
        }

        foreach (var label in labels)
        {
            if (!IsLabel(label))
            {
                reason = "That is not a valid domain. Each part may hold letters, digits and dashes, and may not start or end with a dash.";
                return false;
            }
        }

        if (labels[^1].All(char.IsAsciiDigit))
        {
            reason = "That is not a valid domain.";
            return false;
        }

        if (IsPlatform(normalized, options))
        {
            reason = "That domain belongs to this platform.";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// The domain a request was addressed to, from its Host header, in the
    /// form a stored domain is compared with.
    /// </summary>
    /// <remarks>
    /// Deliberately cheap and deliberately strict: it runs for every request
    /// the server receives, and a header that does not reduce to something a
    /// domain could be is simply not a domain of anybody's.
    /// </remarks>
    public static string? Normalize(string? host)
    {
        if (string.IsNullOrEmpty(host))
        {
            return null;
        }

        var span = host.AsSpan().Trim();

        // an address in brackets is never a domain, and its colons are not a port
        if (span.Length == 0 || span[0] == '[')
        {
            return null;
        }

        var colon = span.IndexOf(':');

        if (colon >= 0)
        {
            span = span[..colon];
        }

        if (span.Length > 0 && span[^1] == '.')
        {
            span = span[..^1];
        }

        if (span.Length == 0 || span.Length > MaxLength)
        {
            return null;
        }

        return span.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Whether a domain is one the platform itself answers at, or below one
    /// of them - claiming either would take the platform's own address.
    /// </summary>
    private static bool IsPlatform(string domain, LambdaOptions options)
    {
        foreach (var own in PlatformHosts(options))
        {
            if (domain == own || domain.EndsWith("." + own, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> PlatformHosts(LambdaOptions options)
    {
        yield return "localhost";

        if (options.PublicUrl != null && Uri.TryCreate(options.PublicUrl, UriKind.Absolute, out var url))
        {
            yield return url.IdnHost.ToLowerInvariant();
        }

        foreach (var origin in options.McpOrigins)
        {
            if (Normalize(origin) is { } host)
            {
                yield return host;
            }
        }
    }

    /// <summary>
    /// What is left of an entry once whatever came with a pasted address is
    /// taken off it: the scheme, a path, a port, a trailing dot.
    /// </summary>
    private static string Strip(string input)
    {
        var value = input.Trim();

        var scheme = value.IndexOf("://", StringComparison.Ordinal);

        if (scheme >= 0)
        {
            value = value[(scheme + 3)..];
        }

        var path = value.IndexOfAny(['/', '?', '#']);

        if (path >= 0)
        {
            value = value[..path];
        }

        return Normalize(value) ?? string.Empty;
    }

    private static bool IsLabel(string label)
    {
        if (label.Length == 0 || label.Length > MaxLabel || label[0] == '-' || label[^1] == '-')
        {
            return false;
        }

        foreach (var character in label)
        {
            if (character is not ((>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

}
