using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// Changes being worked on beside a lambda, without touching it.
/// </summary>
/// <remarks>
/// Versions never change once they are stored, so a feature is where work
/// happens: it branches off a version, is changed in place for as long as it
/// takes, tries itself out at an address of its own against a copy of the
/// lambda's data, and is then merged - its files become the next version - or
/// deleted. Only a feature based on the newest version is merged; bringing a
/// later version's changes in is left to whoever works on it, who says so by
/// moving its base. Nothing here merges anything by itself.
/// </remarks>
public interface IFeatureService
{

    /// <summary>
    /// The features of a lambda, the most recently changed first.
    /// </summary>
    IReadOnlyList<FeatureInfo> List(string privateKey);

    /// <summary>
    /// One feature, with its files.
    /// </summary>
    FeatureContent Get(string privateKey, string feature);

    /// <summary>
    /// Branches a feature off a version: a copy of its files, and a copy of the
    /// lambda's data as it is now.
    /// </summary>
    ValueTask<FeatureInfo> CreateAsync(string privateKey, FeatureDraft draft, CancellationToken cancellation = default);

    /// <summary>
    /// Renames a feature, changes what is said about it, or moves its base.
    /// </summary>
    FeatureInfo Update(string privateKey, string feature, FeatureUpdate update);

    /// <summary>
    /// Saves the given files as the feature's, replacing what it held.
    /// </summary>
    /// <param name="note">What the user wants from it and what it changes, where given; the rest is kept</param>
    /// <param name="after">
    /// The save of its files these were made from, to be refused as a conflict when the feature was saved
    /// again since - by somebody else, whose save would otherwise be lost without a trace
    /// </param>
    FeatureInfo Save(string privateKey, string feature, string code, VersionNote? note = null, int? after = null);

    /// <summary>
    /// Puts the feature's files online at its own address, with its own copy
    /// of the data. The lambda itself is not touched.
    /// </summary>
    ValueTask<FeatureDeployment> DeployAsync(string privateKey, string feature, CancellationToken cancellation = default);

    /// <summary>
    /// Takes the preview of a feature offline.
    /// </summary>
    FeatureInfo Undeploy(string privateKey, string feature);

    /// <summary>
    /// Replaces the feature's copy of the data with a fresh copy of the lambda's.
    /// </summary>
    ValueTask<FeatureInfo> RefreshDataAsync(string privateKey, string feature, CancellationToken cancellation = default);

    /// <summary>
    /// Makes the feature's files the next version of the lambda, and removes
    /// the feature - its preview and its copy of the data with it.
    /// </summary>
    /// <remarks>
    /// Refused unless the feature is based on the newest version, differs from
    /// it and compiles. The lambda's own data is not touched.
    /// </remarks>
    /// <param name="note">Notes for the version, where the feature's own should not be used</param>
    /// <param name="deploy">Whether to put the new version online as well</param>
    ValueTask<FeatureMerge> MergeAsync(string privateKey, string feature, VersionNote? note = null, bool deploy = false,
                                       CancellationToken cancellation = default);

    /// <summary>
    /// Removes a feature, its preview and its copy of the data.
    /// </summary>
    void Delete(string privateKey, string feature);

    /// <summary>
    /// The identities a feature's files and data are kept under, for the
    /// services that reach into its copy of the workspace.
    /// </summary>
    /// <param name="editable">Whether it is about to be changed, which is refused for a demo</param>
    (long LambdaId, long FeatureId) Require(string privateKey, string feature, bool editable);

    /// <summary>
    /// The preview a request to <c>/features/{key}/</c> is for, if it is online.
    /// </summary>
    ResolvedLambda? ResolvePreview(string key);

    /// <summary>
    /// Takes the previews of free lambdas offline once their feature was left
    /// alone for as long as a deployment of the lambda itself would be.
    /// </summary>
    int RunMaintenance(DateTime now);

    /// <summary>
    /// Removes the files of features that no longer exist.
    /// </summary>
    /// <returns>How many were removed</returns>
    int Sweep();

}
