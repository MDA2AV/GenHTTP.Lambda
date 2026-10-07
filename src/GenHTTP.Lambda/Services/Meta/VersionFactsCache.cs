using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What the overview of a lambda says about one of its versions: how large it
/// is, what its code reaches for and what its documentation says.
/// </summary>
/// <param name="CodeFiles">Every file of its code: the C# it is compiled from and whatever else it is kept with</param>
/// <param name="CodeBytes">What those weigh</param>
/// <param name="ResourceFiles">The files it reads and serves while it runs</param>
/// <param name="ResourceBytes">What those weigh, decoded</param>
public sealed record VersionFacts(int CodeFiles, long CodeBytes, int ResourceFiles, long ResourceBytes, bool ServesResources, bool ServesWorkspace,
                                  bool UsesWorkspace, bool UsesDatabase, DocumentationSummary Documentation);

/// <summary>
/// What a version says about itself: the pages of its code kept in
/// <c>docs/</c> and <c>tests/</c>.
/// </summary>
/// <param name="About">The first paragraph of its product page, as plain text - what the app is, in a sentence or two</param>
/// <param name="Product">Whether it has a product page: what the app is, for whom, and why</param>
/// <param name="Decisions">Whether it says which technical decisions were made, and why</param>
/// <param name="Tests">Whether it says how it is tested</param>
/// <param name="Files">How many files its documentation and tests come to</param>
/// <param name="Bytes">What those weigh, which counts towards what a version may come to like the rest of its code</param>
public sealed record DocumentationSummary(string? About, bool Product, bool Decisions, bool Tests, int Files, long Bytes);

/// <summary>
/// The facts of the versions whose overview was asked for lately, read off
/// the versions themselves.
/// </summary>
/// <remarks>
/// The overview is asked for every ten seconds by every editor that is open
/// on it, and reading these facts means reading and unpacking the whole
/// version - resources included, up to a hundred megabytes and more. A version
/// never changes once saved, so what is read off it once holds for good: kept
/// by the lambda's id, which is never handed out again, and the number of the
/// version. Bounded by starting over, like the other caches of requests.
/// </remarks>
public sealed partial class VersionFactsCache(IStorageService storage)
{
    private const int MostEntries = 1024;

    private readonly ConcurrentDictionary<(long LambdaId, int Version), VersionFacts> _facts = [];

    #region Functionality

    /// <summary>
    /// What the given version of a lambda is made of, or nothing at all for
    /// no version.
    /// </summary>
    public VersionFacts Of(long lambdaId, int? version)
    {
        if (version is not { } wanted)
        {
            return Empty;
        }

        if (_facts.TryGetValue((lambdaId, wanted), out var known))
        {
            return known;
        }

        // a version whose code has gone missing is reported as empty rather
        // than making the whole overview unavailable - and not remembered as
        // empty, it is asked about again
        if (storage.Read(lambdaId, wanted) is not { } code)
        {
            return Empty;
        }

        if (_facts.Count >= MostEntries)
        {
            _facts.Clear();
        }

        return _facts[(lambdaId, wanted)] = Read(LambdaSource.Parse(code));
    }

    public static VersionFacts Empty { get; } = new(0, 0, 0, 0, false, false, false, false, new DocumentationSummary(null, false, false, false, 0, 0));

    #endregion

    #region Helpers

    private static VersionFacts Read(IReadOnlyList<LambdaFile> files)
    {
        var code = files.Where(f => f.IsCode).ToList();

        var compiled = code.Where(f => f.IsCompiled).ToList();

        var resources = files.Where(f => f.IsResource).ToList();

        return new VersionFacts(
            code.Count,
            LambdaSource.Size(code),
            resources.Count,
            LambdaSource.Size(resources),
            compiled.Any(f => ServingResources().IsMatch(f.Code)),
            compiled.Any(f => ServingWorkspace().IsMatch(f.Code)),
            compiled.Any(f => UsingWorkspace().IsMatch(f.Code)),
            compiled.Any(f => DatabaseService.Uses(f.Code)),
            Document(files)
        );
    }

    /// <summary>
    /// What the documentation of that version says the app is, and which of
    /// its pages there are.
    /// </summary>
    private static DocumentationSummary Document(IReadOnlyList<LambdaFile> files)
    {
        var product = ContextPages.Read(files, LambdaSource.ProductDoc);

        var written = files.Where(f => f.Name.StartsWith(LambdaSource.DocsFolder, StringComparison.Ordinal)
                                    || f.Name.StartsWith(LambdaSource.TestsFolder, StringComparison.Ordinal)).ToList();

        return new DocumentationSummary(
            ContextPages.FirstParagraph(product),
            !string.IsNullOrWhiteSpace(product),
            !string.IsNullOrWhiteSpace(ContextPages.Read(files, LambdaSource.DecisionsDoc)),
            !string.IsNullOrWhiteSpace(ContextPages.Read(files, LambdaSource.TestingDoc)),
            written.Count,
            LambdaSource.Size(written)
        );
    }

    /// <summary>
    /// The calls that turn a directory into something served. Read from the
    /// code, so it says what the code asks for rather than what a request
    /// would find - which is the right thing to warn about. Assets is what
    /// the resources were called before, which older code still says.
    /// </summary>
    [GeneratedRegex(@"\b(Resources|Assets)\s*\.\s*(App|Files|Tree)\s*\(")]
    private static partial Regex ServingResources();

    [GeneratedRegex(@"\bWorkspace\s*\.\s*(App|Files|Tree)\s*\(")]
    private static partial Regex ServingWorkspace();

    /// <summary>
    /// Any use of the workspace at all, to warn whoever is about to switch it
    /// off that the code online would then fail where it reaches for it.
    /// </summary>
    [GeneratedRegex(@"\bWorkspace\s*\.\s*[A-Z]\w*")]
    private static partial Regex UsingWorkspace();

    #endregion

}
