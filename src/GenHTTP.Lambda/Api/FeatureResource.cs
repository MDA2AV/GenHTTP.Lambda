using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Features: changes being worked on beside a lambda, without touching it.
/// </summary>
/// <remarks>
/// Versions never change once they are stored. A feature is where a change is
/// made instead: it branches off a version with a copy of its files and a copy
/// of the lambda's data, is changed in place as often as it takes, and its
/// preview is put online at <c>/features/{feature}/</c> to try it - against
/// its copy of the data, while the lambda goes on serving its visitors from
/// the version that is online. Once it does what was asked, merging it makes
/// its files the next version (and optionally puts that online); deleting it
/// throws it away. Either way its copy of the data goes with it.
///
/// Only a feature based on the newest version is merged, so it cannot undo a
/// version saved after it branched off. Bringing such a version's changes into
/// the feature is left to whoever works on it; once they are in, moving the
/// feature's base to that version with a patch allows the merge.
/// </remarks>
public sealed class FeatureResource(IFeatureService features, IMetaService meta, IDataService data, LogBook book, LambdaOptions options,
                                     ILogger<FeatureResource> logger)
{

    #region The features

    /// <summary>
    /// Lists the features of a lambda, the most recently changed first.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features")]
    public async ValueTask<List<FeatureResponse>> List(string privateKey)
        => [.. (await features.ListAsync(privateKey)).Select(Describe)];

    /// <summary>
    /// Starts a feature from a version - the newest unless another is named.
    /// </summary>
    /// <remarks>
    /// The feature gets a copy of the version's files and a copy of the
    /// lambda's workspace as it is now. Nothing is online until its preview is
    /// started, and the lambda itself is not touched at all.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features")]
    public async ValueTask<Result<FeatureResponse>> Create(string privateKey, CreateFeatureRequest request)
    {
        var created = await features.CreateAsync(privateKey, new FeatureDraft(request.Name, request.Specification, request.Base, VersionOrigins.Api));

        logger.LogInformation("Started feature '{Feature}' of lambda {Lambda} from version {Version}", created.Name, await meta.PublicKeyOfAsync(privateKey), created.Base);

        return new Result<FeatureResponse>(Describe(created)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// Reads a feature with its files.
    /// </summary>
    /// <param name="folder">Only the files below this folder - <c>.lambda/</c> for its documentation and tests - rather than every one</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature")]
    public async ValueTask<FeatureContentResponse> Get(string privateKey, string feature, string? folder)
    {
        var found = await features.GetAsync(privateKey, feature);

        return new FeatureContentResponse(Describe(found.Feature), VersionResource.Below(LambdaSource.Parse(found.Code), VersionResource.Decode(folder)));
    }

    /// <summary>
    /// Renames a feature, changes what is said about it, or moves its base.
    /// </summary>
    /// <remarks>
    /// Move the base only once the changes of that version are in the
    /// feature's files: merging replaces the lambda's files with the feature's,
    /// and nothing checks that they are.
    /// </remarks>
    [ResourceMethod(Method.Patch, "lambdas/:privateKey/features/:feature")]
    public async ValueTask<FeatureResponse> Update(string privateKey, string feature, UpdateFeatureRequest request)
    {
        var updated = await features.UpdateAsync(privateKey, feature, new FeatureUpdate(request.Name, request.Specification, request.Change, request.Base));

        logger.LogInformation("Changed feature '{Feature}' of lambda {Lambda}, based on version {Version}", updated.Name, await meta.PublicKeyOfAsync(privateKey), updated.Base);

        return Describe(updated);
    }

    /// <summary>
    /// Deletes a feature, its preview and its copy of the data.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature")]
    public async ValueTask Delete(string privateKey, string feature)
    {
        var name = await features.NameOfAsync(privateKey, feature);

        await features.DeleteAsync(privateKey, feature);

        logger.LogInformation("Deleted feature '{Feature}' of lambda {Lambda}", name, await meta.PublicKeyOfAsync(privateKey));
    }

    #endregion

    #region Its files

    /// <summary>
    /// Saves every file of a feature, replacing the ones it has.
    /// </summary>
    /// <param name="deploy">Whether to put its preview online as well</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/files")]
    public async ValueTask<FeatureSavedResponse> PutFiles(string privateKey, string feature, bool? deploy, FeatureFilesRequest request)
        => await SaveAsync(privateKey, feature, request.Files, deploy, request.Specification, request.Change, request.Revision);

    /// <summary>
    /// Changes some files of a feature: adds or replaces files, removes them,
    /// or replaces text within them. Files that are not named stay as they are.
    /// </summary>
    /// <param name="deploy">Whether to put its preview online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/changes")]
    public async ValueTask<FeatureSavedResponse> Change(string privateKey, string feature, bool? deploy, VersionChangeRequest request)
    {
        var read = await features.GetAsync(privateKey, feature);

        var files = LambdaChanges.Apply(LambdaSource.Parse(read.Code), request.Files, request.Remove, request.Edits);

        // made on top of what was read, and refused if something else was saved meanwhile
        return await SaveAsync(privateKey, feature, files, deploy, request.Specification, request.Change, read.Feature.Revision);
    }

    /// <summary>
    /// Downloads the files of a feature as a zip archive.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/features/:feature/zip")]
    public async ValueTask<IResponse> GetArchive(string privateKey, string feature, IRequest request)
    {
        var found = await features.GetAsync(privateKey, feature);

        var zip = LambdaArchive.Pack(LambdaSource.Parse(found.Code));

        logger.LogInformation("Downloaded feature '{Feature}' of lambda {Lambda} as a zip archive", found.Feature.Name, await meta.PublicKeyOfAsync(privateKey));

        return request.Respond()
                      .Content(new BinaryContent(zip, "application/zip"))
                      .Header("Content-Disposition", $"attachment; filename=\"feature-{found.Feature.Key[..8]}.zip\"")
                      .Build();
    }

    /// <summary>
    /// Saves the files of a zip archive as the feature's, replacing the ones
    /// it has - every file, as for a version.
    /// </summary>
    /// <param name="deploy">Whether to put its preview online as well</param>
    /// <param name="specification">What the user wants from it and why; left out, the one it has is kept</param>
    /// <param name="change">What it changes, in a line; left out, the one it has is kept</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/zip")]
    public async ValueTask<FeatureSavedResponse> PutArchive(string privateKey, string feature, bool? deploy, string? specification, string? change,
                                                            Stream body)
    {
        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = await meta.RequireAsync(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        var files = await LambdaArchive.UnpackAsync(body, options.MaxCodeLengthOf(tier) * 4L + options.MaxAssetBytesOf(tier));

        return await SaveAsync(privateKey, feature, files, deploy, VersionResource.Decode(specification), VersionResource.Decode(change));
    }

    #endregion

    #region Its preview

    /// <summary>
    /// Puts the feature's files online at its own address, <c>/features/{feature}/</c>,
    /// against its own copy of the data. The lambda itself is not touched.
    /// </summary>
    /// <remarks>
    /// Code that does not compile is answered with 422 and what the compiler
    /// said, and whatever the preview served before goes on being served.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/preview/start")]
    public async ValueTask<Result<FeaturePreviewResponse>> Start(string privateKey, string feature)
    {
        var deployment = await features.DeployAsync(privateKey, feature);

        logger.Previewed(deployment, await meta.PublicKeyOfAsync(privateKey));

        var outcome = Describe(deployment);

        return new Result<FeaturePreviewResponse>(outcome).Status(outcome.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    /// <summary>
    /// Takes the preview of a feature offline.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/preview/stop")]
    public async ValueTask<FeatureResponse> Stop(string privateKey, string feature)
    {
        var stopped = await features.UndeployAsync(privateKey, feature);

        logger.LogInformation("Took the preview of feature '{Feature}' of lambda {Lambda} offline", stopped.Name, await meta.PublicKeyOfAsync(privateKey));

        return Describe(stopped);
    }

    /// <summary>
    /// What the preview of a feature has been doing, oldest first - kept apart
    /// from what the lambda's own visitors caused.
    /// </summary>
    /// <param name="since">The last sequence already seen. Omitted, only the tail comes back.</param>
    /// <param name="level">The lowest level worth returning: debug, info, warn or error</param>
    /// <param name="limit">At most this many lines, up to 2000</param>
    [ResourceMethod("lambdas/:privateKey/features/:feature/logs")]
    public async ValueTask<OwnerLogResponse> Logs(string privateKey, string feature, long? since, string? level, int? limit)
    {
        var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, false);

        var wanted = Math.Clamp(limit ?? (since.HasValue ? 1000 : 500), 1, 2000);

        var (lines, cursor, missed) = book.Read(since ?? 0, null, MonitoringResource.Minimum(level), wanted, lambdaId: lambdaId,
                                                feature: FeatureLines.Of(featureId));

        return new OwnerLogResponse([.. lines.Select(MonitoringResource.Describe)], cursor, missed, options.CaptureLambdaOutput);
    }

    #endregion

    #region Its data

    /// <summary>
    /// Every kind of data there is, as the feature's copy of it is.
    /// </summary>
    /// <remarks>
    /// Which kinds there are is the lambda's to decide; a feature has a copy
    /// of whatever the lambda has, taken when the feature was started. Its
    /// workspace is under <c>/lambdas/{privateKey}/features/{feature}/workspace</c>.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/features/:feature/data")]
    public async ValueTask<List<DataStoreResponse>> Data(string privateKey, string feature)
        => [.. (await data.ListAsync(privateKey, feature)).Select(DataResource.Describe)];

    /// <summary>
    /// Replaces the feature's copy of the data with a fresh copy of the
    /// lambda's, and builds its preview again on its next request.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/data/refresh")]
    public async ValueTask<FeatureResponse> Refresh(string privateKey, string feature)
    {
        var refreshed = await features.RefreshDataAsync(privateKey, feature);

        logger.LogInformation("Gave feature '{Feature}' of lambda {Lambda} a fresh copy of the data", refreshed.Name, await meta.PublicKeyOfAsync(privateKey));

        return Describe(refreshed);
    }

    #endregion

    #region Merging

    /// <summary>
    /// Makes the feature's files the next version of the lambda, and deletes
    /// the feature - its preview and its copy of the data with it.
    /// </summary>
    /// <remarks>
    /// Answered with 409 while the feature is not based on the newest version,
    /// and with 422 and what the compiler said when its code does not compile;
    /// in both cases the feature stays as it is. The lambda's own data is not
    /// touched. With <c>deploy</c> the new version is put online as well,
    /// which is answered in the deployment it carries.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/merge")]
    public async ValueTask<Result<FeatureMergeResponse>> Merge(string privateKey, string feature, MergeFeatureRequest? request)
    {
        var note = new VersionNote(request?.Specification, request?.Change, VersionOrigins.Api);

        var name = await features.NameOfAsync(privateKey, feature);

        var merged = await features.MergeAsync(privateKey, feature, note, request?.Deploy == true);

        var publicKey = await meta.PublicKeyOfAsync(privateKey);

        if (merged.Merged)
        {
            logger.LogInformation("Merged feature '{Feature}' of lambda {Lambda} as version {Version}", name, publicKey, merged.Version?.Version);
        }
        else
        {
            logger.LogInformation("Feature '{Feature}' of lambda {Lambda} was not merged: its code does not compile", name, publicKey);
        }

        if (merged.Deployment is { } online)
        {
            logger.Deployed(online, publicKey, merged.Version?.Version);
        }

        var deployment = merged.Deployment is { } result
            ? new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics)
            : null;

        var response = new FeatureMergeResponse(merged.Merged, merged.Version == null ? null : VersionResource.Describe(merged.Version),
                                                merged.Diagnostics, deployment);

        return new Result<FeatureMergeResponse>(response).Status(merged.Merged ? ResponseStatus.Created : ResponseStatus.UnprocessableEntity);
    }

    #endregion

    #region Helpers

    private async ValueTask<FeatureSavedResponse> SaveAsync(string privateKey, string feature, IReadOnlyList<LambdaFile>? files, bool? deploy,
                                                            string? specification, string? change, int? after = null)
    {
        var saved = await features.SaveAsync(privateKey, feature, VersionResource.Serialize(files), new VersionNote(specification, change), after);

        var publicKey = await meta.PublicKeyOfAsync(privateKey);

        logger.LogInformation("Saved feature '{Feature}' of lambda {Lambda} with {Files} file(s)", saved.Name, publicKey, files!.Count);

        FeaturePreviewResponse? preview = null;

        if (deploy == true)
        {
            var deployment = await features.DeployAsync(privateKey, feature);

            logger.Previewed(deployment, publicKey);

            preview = Describe(deployment);

            return new FeatureSavedResponse(preview.Feature, preview);
        }

        return new FeatureSavedResponse(Describe(saved), preview);
    }

    internal static FeatureResponse Describe(FeatureInfo feature)
        => new(feature.Key, feature.Name, feature.Specification, feature.Change, feature.Base, feature.Newest, feature.Mergeable, feature.Origin,
               feature.Created, feature.Modified, feature.Online, feature.Current, feature.Previewed, feature.Path, feature.Revision);

    private static FeaturePreviewResponse Describe(FeatureDeployment deployment)
        => new(deployment.Success, Describe(deployment.Feature), deployment.Diagnostics);

    #endregion

}
