using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// A feature as the editor and an agent see it.
/// </summary>
/// <param name="Key">What it is addressed by, and where its preview answers: <c>/features/{Key}/</c></param>
/// <param name="Name">What it is, in a few words</param>
/// <param name="Branch">The branch it is in the lambda's git repository</param>
/// <param name="Specification">What the user wants from it and why; the version it is merged into keeps it</param>
/// <param name="Change">What it changes, in a line; the version it is merged into keeps it</param>
/// <param name="Base">The version it is based on: the one it branched off, or a later one whose changes were brought in</param>
/// <param name="Newest">The newest version of the lambda, which the base has to be for the feature to be merged</param>
/// <param name="Origin">Where it came from: api or agent</param>
/// <param name="Modified">When its files, notes or preview last changed</param>
/// <param name="Online">Whether its preview is online</param>
/// <param name="Current">Whether its preview is online and serves what the feature holds now, rather than an earlier save</param>
/// <param name="Previewed">When its preview was last put online, while it is</param>
/// <param name="Revision">How many times its files were saved, counting the ones it began with</param>
public sealed record FeatureInfo(
    string Key,
    string Name,
    string Branch,
    string? Specification,
    string? Change,
    int Base,
    int? Newest,
    string? Origin,
    DateTime Created,
    DateTime Modified,
    bool Online,
    bool Current,
    DateTime? Previewed,
    int Revision
)
{

    /// <summary>
    /// Whether it can be merged: it is based on the newest version, so its
    /// files do not undo anything saved since it branched off.
    /// </summary>
    public bool Mergeable => Newest == Base;

    /// <summary>
    /// Where its preview answers, on the platform.
    /// </summary>
    public string Path => $"/features/{Key}/";

}

/// <summary>
/// A feature with the files it holds.
/// </summary>
/// <param name="Code">Every file, as the single blob a version is stored as</param>
public sealed record FeatureContent(FeatureInfo Feature, string Code);

/// <summary>
/// What a new feature is to be.
/// </summary>
/// <param name="Name">What it is, in a few words</param>
/// <param name="Specification">What the user wants from it and why</param>
/// <param name="Base">The version to branch off; the newest when left out</param>
/// <param name="Origin">Which door it came through</param>
/// <param name="Branch">The branch it is to be in the lambda's git repository; left out, one named after it</param>
public sealed record FeatureDraft(string? Name, string? Specification = null, int? Base = null, string? Origin = null, string? Branch = null);

/// <summary>
/// What to change about a feature. What is left out stays as it is.
/// </summary>
/// <param name="Base">
/// The version it is now based on - said once the changes of that version are in its files, and what allows it to be
/// merged once that is the newest
/// </param>
public sealed record FeatureUpdate(string? Name = null, string? Specification = null, string? Change = null, int? Base = null);

/// <summary>
/// How putting the preview of a feature online went.
/// </summary>
/// <param name="Feature">The feature afterwards</param>
/// <param name="Diagnostics">What the compiler said on the way</param>
public sealed record FeatureDeployment(bool Success, FeatureInfo Feature, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// How merging a feature went.
/// </summary>
/// <param name="Name">The name of the feature, to say which one it was once it is gone</param>
/// <param name="Merged">Whether its files became a version, and the feature is gone</param>
/// <param name="Version">The version it became</param>
/// <param name="Diagnostics">Why it was not merged, where its code does not compile</param>
/// <param name="Deployment">The outcome of putting that version online, if that was asked for</param>
public sealed record FeatureMerge(string Name, bool Merged, LambdaVersionInfo? Version, IReadOnlyList<CompilationDiagnostic> Diagnostics,
                                  DeploymentResult? Deployment);
