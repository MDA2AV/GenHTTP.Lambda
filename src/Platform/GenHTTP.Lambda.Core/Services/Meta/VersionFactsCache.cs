using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The facts of the versions whose overview was asked for lately, read off
/// the versions themselves.
/// </summary>
/// <remarks>
/// The overview is asked for every ten seconds by every editor that is open
/// on it. What a version is made of is read off its manifest, and what its
/// code reaches for and its documentation says off its C# and its pages -
/// never its resources, however much they weigh. A version never changes
/// once saved, so what is read off it once holds for good: kept by the
/// lambda's id, which is never handed out again, and the number of the
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

        // a version whose files have gone missing is reported as empty rather
        // than making the whole overview unavailable - and not remembered as
        // empty, it is asked about again
        VersionFacts facts;

        try
        {
            if (storage.Read(lambdaId, wanted) is not { } files)
            {
                return Empty;
            }

            facts = Read(files);
        }
        catch (Exception e) when (e is IOException or InvalidDataException)
        {
            return Empty;
        }

        if (_facts.Count >= MostEntries)
        {
            _facts.Clear();
        }

        return _facts[(lambdaId, wanted)] = facts;
    }

    public static VersionFacts Empty { get; } = new(0, 0, 0, 0, false, false, false, false, false, new DocumentationSummary(null, false, false, false, 0, 0));

    #endregion

    #region Helpers

    private static VersionFacts Read(IReadOnlyList<VersionFile> files)
    {
        var code = files.Where(f => f.IsCode).ToList();

        // the C# alone is read, once each
        var compiled = LambdaSource.Compiled(files);

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
            // read as the code guard reads it where a lambda may not connect
            CodeGuard.Connects(compiled),
            Document(files)
        );
    }

    /// <summary>
    /// What the documentation of that version says the app is, and which of
    /// its pages there are.
    /// </summary>
    private static DocumentationSummary Document(IReadOnlyList<VersionFile> files)
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
