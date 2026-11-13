using System.Globalization;
using System.Text;

using GenHTTP.Modules.Git;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// The branch a feature is in the lambda's git repository: what it may be
/// called, and what a feature started anywhere but git is called there.
/// </summary>
/// <remarks>
/// A branch is named once, when the feature starts, and kept: a clone knows
/// it by its name, so renaming the feature leaves it alone. A branch somebody
/// pushed keeps the name they gave it; any other feature is given its name
/// in the words git allows - "Dark mode" is <c>dark-mode</c> - and a number
/// where that is taken.
/// </remarks>
internal static class FeatureBranches
{

    /// <summary>
    /// The branch the versions are, which no feature may be.
    /// </summary>
    public const string Main = "main";

    /// <summary>
    /// How long the name of a branch may be.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// How long a branch named after a feature gets before the rest is cut.
    /// </summary>
    private const int Shortened = 40;

    #region Functionality

    /// <summary>
    /// Why a branch somebody wants could not be a feature's, or nothing when it could.
    /// </summary>
    public static string? Check(string branch)
    {
        if (branch.Length > MaxLength)
        {
            return $"A branch of a lambda may be called {MaxLength} characters at most.";
        }

        if (string.Equals(branch, Main, StringComparison.OrdinalIgnoreCase) || string.Equals(branch, "HEAD", StringComparison.OrdinalIgnoreCase))
        {
            return $"'{branch}' is not a name for a feature: main holds the versions.";
        }

        if (!GitReference.IsValidName($"refs/heads/{branch}"))
        {
            return $"'{branch}' is not a name git allows for a branch.";
        }

        return null;
    }

    /// <summary>
    /// The branch a feature called this is given, unless one of the taken is
    /// called that - then the first with a number after it that is free.
    /// </summary>
    /// <param name="taken">The branches of the lambda's other features</param>
    public static string For(string name, IEnumerable<string> taken)
    {
        var stem = Slug(name);

        var used = taken.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!Clashes(stem, used))
        {
            return stem;
        }

        for (var number = 2; ; number++)
        {
            var candidate = $"{stem}-{number}";

            if (!Clashes(candidate, used))
            {
                return candidate;
            }
        }
    }

    /// <summary>
    /// Whether a branch could not be held beside the taken ones: one of the
    /// same name, or one that would be a folder of it or it of one -
    /// <c>dark-mode</c> and <c>dark-mode/menu</c> cannot both be in a clone.
    /// </summary>
    /// <remarks>
    /// Without regard to case, since a clone on a file system that has none
    /// keeps its branches as files.
    /// </remarks>
    public static bool Clashes(string branch, IEnumerable<string> taken)
    {
        foreach (var other in taken)
        {
            if (string.Equals(branch, other, StringComparison.OrdinalIgnoreCase)
             || branch.StartsWith(other + "/", StringComparison.OrdinalIgnoreCase)
             || other.StartsWith(branch + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return string.Equals(branch, Main, StringComparison.OrdinalIgnoreCase) || branch.StartsWith(Main + "/", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A name in the words a branch may have: lower case letters and digits,
    /// and a dash for everything else.
    /// </summary>
    private static string Slug(string name)
    {
        var builder = new StringBuilder();

        foreach (var character in name.Normalize(NormalizationForm.FormD))
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                // the accent of a letter, which leaves the letter as it is
                continue;
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length > Shortened)
        {
            slug = slug[..Shortened].TrimEnd('-');
        }

        return slug.Length == 0 ? "feature" : slug;
    }

    #endregion

}
