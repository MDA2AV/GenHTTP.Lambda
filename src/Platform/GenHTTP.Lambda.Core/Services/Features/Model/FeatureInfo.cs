using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Hosting;

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
    public string Path => PlatformRoutes.PreviewOf(Key);

}
