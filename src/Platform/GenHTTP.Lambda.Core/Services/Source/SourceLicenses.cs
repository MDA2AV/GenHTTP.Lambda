using System.Globalization;
using System.Reflection;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// The licenses a lambda's source can be published under.
/// </summary>
/// <remarks>
/// A short list rather than every license there is. Whoever publishes here has
/// most likely never chosen one, and a choice between seven that each say
/// something different is one they can make; the list covers what people
/// actually want - let anybody do anything, the same with a patent grant,
/// keep changes open, keep a hosted copy open, or give it away altogether.
/// MIT is the default, being the one most people mean when they say "open
/// source" and the one GenHTTP itself is under.
///
/// The texts are the licenses' own, word for word, embedded rather than
/// fetched: a LICENSE file is the one file of a project that must never be
/// paraphrased. The two that name a year and a holder get them filled in.
/// </remarks>
public static class SourceLicenses
{

    public const string Default = "MIT";

    /// <summary>
    /// How long the name of whoever holds the copyright may be.
    /// </summary>
    public const int MaxAuthor = 100;

    private static readonly SourceLicense[] Catalogue =
    [
        new("MIT", "MIT License", LicenseKind.Permissive),
        new("Apache-2.0", "Apache License 2.0", LicenseKind.Permissive),
        new("BSD-3-Clause", "BSD 3-Clause License", LicenseKind.Permissive),
        new("MPL-2.0", "Mozilla Public License 2.0", LicenseKind.Copyleft),
        new("GPL-3.0-or-later", "GNU General Public License v3.0 or later", LicenseKind.Copyleft),
        new("AGPL-3.0-or-later", "GNU Affero General Public License v3.0 or later", LicenseKind.Copyleft),
        new("Unlicense", "The Unlicense", LicenseKind.PublicDomain)
    ];

    #region Functionality

    /// <summary>
    /// Every license on offer, the default first.
    /// </summary>
    public static IReadOnlyList<SourceLicense> All => Catalogue;

    /// <summary>
    /// The license with an SPDX identifier, however it is capitalised - or
    /// nothing, for one that is not on offer.
    /// </summary>
    public static SourceLicense? Find(string? id)
        => id == null ? null : Array.Find(Catalogue, l => string.Equals(l.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The text of a license as the LICENSE file of a project has it.
    /// </summary>
    /// <param name="year">The year the copyright is claimed for</param>
    /// <param name="holder">Who holds it</param>
    public static string Text(SourceLicense license, int year, string holder)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GenHTTP.Lambda.Resources.Licenses.{license.Id}.txt")
                        ?? throw new InvalidOperationException($"The text of the license '{license.Id}' is missing from the assembly.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd()
                     .ReplaceLineEndings("\n")
                     .Replace("{year}", year.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                     .Replace("{holder}", holder, StringComparison.Ordinal);
    }

    /// <summary>
    /// Who holds the copyright where the owner named nobody.
    /// </summary>
    /// <remarks>
    /// There are no accounts, so there is no name to put there - and a
    /// license naming nobody is weaker than one naming the people behind the
    /// lambda, whoever they turn out to be.
    /// </remarks>
    public static string Holder(string? author, string publicKey)
        => string.IsNullOrWhiteSpace(author) ? $"The authors of {publicKey}" : author.Trim();

    #endregion

}

/// <summary>
/// A license a source can be published under.
/// </summary>
/// <param name="Id">Its SPDX identifier, which is also how it is stored and asked for</param>
/// <param name="Name">Its full name</param>
/// <param name="Kind">What it asks of whoever reuses the code, roughly</param>
public sealed record SourceLicense(string Id, string Name, LicenseKind Kind)
{

    /// <summary>
    /// Where the license is described in full.
    /// </summary>
    public string Url => $"https://spdx.org/licenses/{Id}.html";

}

/// <summary>
/// What a license asks of whoever reuses the code, in the three kinds a
/// person choosing one tells apart.
/// </summary>
public enum LicenseKind
{

    /// <summary>
    /// Anything goes, as long as the notice stays.
    /// </summary>
    Permissive,

    /// <summary>
    /// Changes are shared under the same terms.
    /// </summary>
    Copyleft,

    /// <summary>
    /// No conditions at all.
    /// </summary>
    PublicDomain

}
