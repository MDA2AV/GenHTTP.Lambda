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
public sealed record VersionFacts(int CodeFiles, int CodeLength, int AssetFiles, long AssetBytes, bool ServesAssets, bool ServesWorkspace,
                                  bool UsesWorkspace, bool UsesDatabase, DocumentationSummary Documentation, DevelopmentSummary Development);

/// <summary>
/// What a version says about itself: the context kept in <c>.lambda/docs/</c>
/// and <c>.lambda/tests/</c> beside its program.
/// </summary>
/// <param name="About">The first paragraph of its product page, as plain text - what the app is, in a sentence or two</param>
/// <param name="Product">Whether it has a product page: what the app is, for whom, and why</param>
/// <param name="Decisions">Whether it says which technical decisions were made, and why</param>
/// <param name="Tests">Whether it says how it is tested</param>
/// <param name="Files">How many files its documentation and tests come to</param>
/// <param name="Bytes">What those weigh, which counts towards what its assets may come to</param>
public sealed record DocumentationSummary(string? About, bool Product, bool Decisions, bool Tests, int Files, long Bytes);

/// <summary>
/// What a version keeps of what its assets are built from: its development
/// space in <c>.lambda/dev/</c>.
/// </summary>
/// <param name="Files">How many files it holds - none for a version whose assets are their own source</param>
/// <param name="Bytes">What those weigh, which counts towards what its assets may come to</param>
public sealed record DevelopmentSummary(int Files, long Bytes);

/// <summary>
/// The facts of the versions whose overview was asked for lately, read off
/// the versions themselves.
/// </summary>
/// <remarks>
/// The overview is asked for every ten seconds by every editor that is open
/// on it, and reading these facts means reading and unpacking the whole
/// version - assets included, up to a hundred megabytes and more. A version
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

    public static VersionFacts Empty { get; } = new(0, 0, 0, 0, false, false, false, false, new DocumentationSummary(null, false, false, false, 0, 0),
                                                    new DevelopmentSummary(0, 0));

    #endregion

    #region Helpers

    private static VersionFacts Read(IReadOnlyList<LambdaFile> files)
    {
        var code = files.Where(f => f.IsCode).ToList();

        return new VersionFacts(
            code.Count,
            LambdaSource.Length(files),
            files.Count(f => f.IsAsset),
            LambdaSource.AssetBytes(files),
            code.Any(f => ServingAssets().IsMatch(f.Code)),
            code.Any(f => ServingWorkspace().IsMatch(f.Code)),
            code.Any(f => UsingWorkspace().IsMatch(f.Code)),
            code.Any(f => DatabaseService.Uses(f.Code)),
            Document(files),
            new DevelopmentSummary(files.Count(f => f.IsDevelopment), LambdaSource.DevelopmentBytes(files))
        );
    }

    /// <summary>
    /// What the documentation of that version says the app is, and which of
    /// its pages there are.
    /// </summary>
    private static DocumentationSummary Document(IReadOnlyList<LambdaFile> files)
    {
        var product = ContextPages.Read(files, LambdaSource.ProductDoc);

        return new DocumentationSummary(
            ContextPages.FirstParagraph(product),
            !string.IsNullOrWhiteSpace(product),
            !string.IsNullOrWhiteSpace(ContextPages.Read(files, LambdaSource.DecisionsDoc)),
            !string.IsNullOrWhiteSpace(ContextPages.Read(files, LambdaSource.TestingDoc)),
            files.Count(f => f.IsContext),
            LambdaSource.ContextBytes(files)
        );
    }

    /// <summary>
    /// The calls that turn a directory into something served. Read from the
    /// code, so it says what the code asks for rather than what a request
    /// would find - which is the right thing to warn about.
    /// </summary>
    [GeneratedRegex(@"\bAssets\s*\.\s*(App|Files|Tree)\s*\(")]
    private static partial Regex ServingAssets();

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
