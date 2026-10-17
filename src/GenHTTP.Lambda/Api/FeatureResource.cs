using GenHTTP.Api.Content.Services;
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
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

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
[Tag(ApiDocumentation.Features)]
[Summary("Changes worked on beside a lambda, tried at an address of their own, and merged into it once they work.")]
[Remarks("""
         A feature starts as a copy of a version - the newest, unless another is named - and of the lambda's data: its
         database, workspace and secrets as they are at that moment. It is saved in place as often as needed; unlike a
         version, it keeps no history of its saves. Its **preview** answers at `previewPath` on this site, against its own
         copy of the data, while the lambda goes on serving its visitors as before.

         **Merging** makes the feature's files the next version of the lambda and deletes the feature, its preview and its
         copy of the data. What the preview wrote into its copy - records, files - is not carried over: the lambda's own data
         is never touched by a feature.

         Only a feature based on the newest version can be merged, so a merge cannot undo a version saved after the feature
         started. Where one was, its changes are brought into the feature's files first, then the feature's `base` is moved
         to it (`PATCH`), and then it merges. The platform does not merge files itself.

         A free lambda's preview goes offline, like the lambda, when nobody changes the feature for a while.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoFeature, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class FeatureResource(IFeatureService features, IMetaService meta, IDataService data, ILogBook book, LambdaOptions options, ILimitsService limits,
                                     ILogger<FeatureResource> logger)
{

    #region The features

    [ResourceMethod("lambdas/:privateKey/features")]
    [Summary("Lists the features")]
    [Remarks("The most recently changed first, without their files.")]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The features, the most recently changed first")]
    public List<FeatureResponse> List([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => [.. (features.List(privateKey)).Select(Describe)];

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features")]
    [Summary("Starts a feature")]
    [Remarks("""
             The feature starts from the version named as `base`, or the newest. It gets a copy of the version's files, and a
             copy of the lambda's database, workspace and secrets as they are now - which may take a moment for a lot of data.
             Its preview is not online until it is started, or a save asks for it (`?deploy=true`). The lambda itself is not
             touched.

             It is given a `key`, which every call about it takes, and a `branch`, which is where it is in the lambda's git
             repository. Only so many features may be open at once, as the tier of the lambda allows.
             """)]
    [Response(ResponseStatus.Created, "The feature was started.")]
    [Response(ResponseStatus.BadRequest, "The name is missing, too long (80 characters at most) or more than one line, or the lambda has no version yet.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The lambda has as many features open as its tier allows. Merge or delete one first.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no such version to start from.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<FeatureResponse>> Create([Summary(ApiDocumentation.PrivateKey)] string privateKey, CreateFeatureRequest request)
    {
        var created = await features.CreateAsync(privateKey, new FeatureDraft(request.Name, request.Specification, request.Base, VersionOrigins.Api));

        logger.LogInformation("Created feature '{Feature}' of lambda {Lambda} base {Version}", created.Name, meta.PublicKeyOf(privateKey), created.Base);

        return new Result<FeatureResponse>(Describe(created)).Status(ResponseStatus.Created);
    }

    [ResourceMethod("lambdas/:privateKey/features/:feature")]
    [Summary("Reads a feature with its files")]
    [Remarks("Its `revision` is what a later `PUT …/files` passes to be refused, rather than replace a save made since.")]
    [return: Summary("The feature with its files")]
    public FeatureContentResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                      [Summary("Only the files whose name starts with this - `docs/` for its documentation, `tests/` for its tests")] string? folder)
    {
        var found = features.Get(privateKey, feature);

        return new FeatureContentResponse(Describe(found.Feature), VersionResource.Below(LambdaSource.Parse(found.Code), folder));
    }

    [ResourceMethod(Method.Patch, "lambdas/:privateKey/features/:feature")]
    [Summary("Changes a feature, or moves its base")]
    [Remarks("""
             Its name, its specification or its change. What the request leaves out stays as it is; a `specification` or `change`
             sent empty is cleared. The files are not
             touched.

             Move the `base` only once the changes of that version are in the feature's files. Merging replaces the lambda's
             files with the feature's, and nothing checks that those changes are in.
             """)]
    [Response(ResponseStatus.BadRequest, "The name is empty, too long or more than one line.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, it has no such feature, or there is no version to move the base to.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature afterwards")]
    public FeatureResponse Update([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                  UpdateFeatureRequest request)
    {
        var updated = features.Update(privateKey, feature, new FeatureUpdate(request.Name, request.Specification, request.Change, request.Base));

        logger.LogInformation("Updated feature '{Feature}' of lambda {Lambda} base {Version}", updated.Name, meta.PublicKeyOf(privateKey), updated.Base);

        return Describe(updated);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/features/:feature")]
    [Summary("Deletes a feature")]
    [Remarks("With its preview and its copy of the data. Its files are gone for good unless they were merged. The lambda is not touched.")]
    [Response(ResponseStatus.NoContent, ApiDocumentation.Deleted)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
    {
        var name = features.Delete(privateKey, feature);

        logger.LogInformation("Deleted feature '{Feature}' of lambda {Lambda}", name, meta.PublicKeyOf(privateKey));
    }

    #endregion

    #region Its files

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/files")]
    [Summary("Saves every file of a feature")]
    [Remarks("""
             `files` is the whole feature, `lambda.cs` first, held to the same rules as a version - including what a version may
             come to, since the feature becomes one. A feature keeps no history of its saves: pass the `revision` it was read
             at, and a save made in the meantime (by an agent, or in another window) is refused rather than silently replaced.
             """)]
    [Response(ResponseStatus.BadRequest, "A file cannot be part of a lambda, or the files are larger than a version may be. Nothing was saved.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The feature was saved again since `revision`. Read it again and make the change on top of what it holds now.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature afterwards, and what came of its preview where that was asked for")]
    public async ValueTask<FeatureSavedResponse> PutFiles([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                          [Summary("Whether to put its preview online with what was saved")] bool? deploy,
                                                          FeatureFilesRequest request)
        => await SaveAsync(privateKey, feature, request.Files, deploy, request.Specification, request.Change, request.Revision);

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/changes")]
    [Summary("Changes some files of a feature")]
    [Remarks("""
             Files that are not named stay as they are. As for a version: `remove` takes files out, `files` adds or replaces
             them, and `edits` replaces text within them, each `find` occurring exactly once. The changes are made on top of
             what the feature holds as the request arrives.
             """)]
    [Response(ResponseStatus.BadRequest, "Nothing to change, a file to remove or edit that does not exist, a `find` that occurs never or more than once, or a result that cannot be a lambda. Nothing was saved.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "The feature was saved by somebody else while this change was being made. Try again.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature afterwards, and what came of its preview where that was asked for")]
    public async ValueTask<FeatureSavedResponse> Change([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                        [Summary("Whether to put its preview online with what was saved")] bool? deploy,
                                                        VersionChangeRequest request)
    {
        var read = features.Get(privateKey, feature);

        var files = LambdaChanges.Apply(LambdaSource.Parse(read.Code), request.Files, request.Remove, request.Edits);

        // made on top of what was read, and refused if something else was saved meanwhile
        return await SaveAsync(privateKey, feature, files, deploy, request.Specification, request.Change, read.Feature.Revision);
    }

    [ResourceMethod("lambdas/:privateKey/features/:feature/zip")]
    [Summary("Downloads a feature as a zip archive")]
    [Remarks("As for a version: the files as the lambda names them, or with `?layout=project` as a clone has them - to change them locally and put them back with `PUT …/zip`.")]
    [Response(ResponseStatus.Ok, "The files, as a zip archive", ContentType = ApiDocumentation.Zip)]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, named after the start of the feature's key: `feature-3f9a1c2e.zip`")]
    [Response(ResponseStatus.BadRequest, "There is no such layout.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public IResponse GetArchive([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                [Summary(ApiDocumentation.Layout)] string? layout, IRequest request)
    {
        var laid = VersionResource.LayoutOf(layout);

        var found = features.Get(privateKey, feature);

        var zip = LambdaArchive.Pack(LambdaSource.Parse(found.Code), laid);

        logger.LogInformation("Downloaded feature '{Feature}' of lambda {Lambda} layout {Layout}", found.Feature.Name, meta.PublicKeyOf(privateKey), laid);

        return request.Respond()
                      .Content(zip, new ContentType("application/zip"))
                      .Header("Content-Disposition", $"attachment; filename=\"feature-{found.Feature.Key[..8]}{(laid == ArchiveLayout.Project ? "-project" : string.Empty)}.zip\"")
                      .Build();
    }

    [ResourceMethod(Method.Put, "lambdas/:privateKey/features/:feature/zip")]
    [Summary("Saves a zip archive as the feature's files")]
    [Remarks("""
             The archive replaces every file of the feature, and is read exactly as one saved as a version is
             (`POST …/versions/zip`): a folder around everything removed, `.gitignore` files followed, counted as it is sent.
             """)]
    [RequestBody("A zip archive of every file of the feature", ContentType = ApiDocumentation.Zip)]
    [Response(ResponseStatus.BadRequest, "The body is not a zip archive, it is larger than a version may be, it has no `lambda.cs`, or a file in it cannot be part of a lambda.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The feature afterwards, and what came of its preview where that was asked for")]
    public async ValueTask<FeatureSavedResponse> PutArchive([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                            [Summary("Whether to put its preview online with what was saved")] bool? deploy,
                                                            [Summary("What the user wants from the feature and why; left out, the one it has is kept")] string? specification,
                                                            [Summary("What it changes, in a line; left out, the one it has is kept")] string? change,
                                                            [Summary(ApiDocumentation.Layout)] string? layout, Stream body)
    {
        var laid = VersionResource.LayoutOf(layout);

        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = meta.Require(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        var files = await VersionResource.UnpackAsync(body, limits.BuildOf(tier), laid,
                                                      LambdaSource.Parse(features.Get(privateKey, feature).Code));

        return await SaveAsync(privateKey, feature, files, deploy, specification, change);
    }

    #endregion

    #region Its preview

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/preview/start")]
    [Summary("Puts the preview of a feature online")]
    [Remarks("""
             Compiles the feature's files and serves them at its `previewPath`, `/features/{feature}/` on this site, against its
             own copy of the data. Nobody is sent there; it is reached by its address only. The lambda is not touched.

             Links inside a lambda's pages should be relative: a feature answers below `/features/{feature}/`, where a link that
             starts with a slash leaves it.
             """)]
    [Response(ResponseStatus.Ok, "The preview is online.")]
    [Response(ResponseStatus.UnprocessableEntity, "The code does not compile. `diagnostics` holds what the compiler said; whatever the preview served before, it still serves.",
              Type = typeof(FeaturePreviewResponse))]
    public async ValueTask<Result<FeaturePreviewResponse>> Start([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
    {
        var deployment = await features.DeployAsync(privateKey, feature);

        logger.Previewed(deployment, meta.PublicKeyOf(privateKey));

        var outcome = Describe(deployment);

        return new Result<FeaturePreviewResponse>(outcome).Status(outcome.Success ? ResponseStatus.Ok : ResponseStatus.UnprocessableEntity);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/preview/stop")]
    [Summary("Takes the preview of a feature offline")]
    [Remarks("The feature, its files and its copy of the data are kept. Nothing happens if the preview is offline already.")]
    [return: Summary("The feature afterwards")]
    public FeatureResponse Stop([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
    {
        var stopped = features.Undeploy(privateKey, feature);

        logger.LogInformation("Undeployed feature '{Feature}' of lambda {Lambda}", stopped.Name, meta.PublicKeyOf(privateKey));

        return Describe(stopped);
    }

    [ResourceMethod("lambdas/:privateKey/features/:feature/logs")]
    [Summary("Reads the log of a feature's preview")]
    [Remarks("""
             Its requests, what its code printed, and what the server said while serving it - kept apart from the lambda's own
             log, and read the same way (`GET /lambdas/{privateKey}/logs`): oldest first, followed with `since`.
             """)]
    [return: Summary("The lines, and where to carry on from")]
    public OwnerLogResponse Logs([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                 [Summary("The `cursor` of the previous answer: only what came after it. Left out, the last 500 lines.")] long? since,
                                 [Summary("The lowest level to answer with: `debug`, `info` (the default), `warn` or `error`")] string? level,
                                 [Summary("At most this many lines, up to 2000; by default 500, or 1000 with `since`")] int? limit)
    {
        var (lambdaId, featureId) = features.Require(privateKey, feature, false);

        var wanted = Math.Clamp(limit ?? (since.HasValue ? 1000 : 500), 1, 2000);

        var (lines, cursor, missed) = book.Read(since ?? 0, null, MonitoringResource.Minimum(level), wanted, lambdaId: lambdaId,
                                                feature: FeatureLines.Of(featureId));

        return new OwnerLogResponse([.. lines.Select(MonitoringResource.Describe)], cursor, missed, options.CaptureLambdaOutput);
    }

    #endregion

    #region Its data

    [ResourceMethod("lambdas/:privateKey/features/:feature/data")]
    [Summary("Lists a feature's copy of the data")]
    [Remarks("""
             Which kinds are switched on is the lambda's to decide; the feature has a copy of what the lambda had when the
             feature was started or its data was last refreshed. What is in the copies is under `…/features/{feature}/database`,
             `…/workspace` and `…/secrets`.
             """)]
    [return: Summary("Every kind of data, as the feature's copy of it is")]
    public List<DataStoreResponse> Data([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
        => [.. (data.List(privateKey, feature)).Select(DataResource.Describe)];

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/data/refresh")]
    [Summary("Copies the lambda's data into a feature again")]
    [Remarks("""
             The database, workspace and secrets of the feature are thrown away and copied from the lambda again, as they are
             now. What the preview wrote into its copy is lost. The preview starts again on its next request, so it reads the
             fresh copy rather than what it held in memory.
             """)]
    [return: Summary("The feature afterwards")]
    public async ValueTask<FeatureResponse> Refresh([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature)
    {
        var refreshed = await features.RefreshDataAsync(privateKey, feature);

        logger.LogInformation("Refreshed data of feature '{Feature}' of lambda {Lambda}", refreshed.Name, meta.PublicKeyOf(privateKey));

        return Describe(refreshed);
    }

    #endregion

    #region Merging

    [ResourceMethod(Method.Post, "lambdas/:privateKey/features/:feature/merge")]
    [Summary("Merges a feature as the next version")]
    [Remarks("""
             The feature has to be based on the newest version, and its code has to compile - a version is only ever made of
             code that compiles. Then its files are saved as the next version, with the feature's specification and change
             unless the request says others, and the feature is deleted with its preview and its copy of the data. The lambda's
             own data is not touched, so what the preview wrote is not carried over.

             With `deploy`, the new version is put online as well; `deployment` says how that went. The version is made either
             way. The body is optional.
             """)]
    [Response(ResponseStatus.Created, "The feature was merged as `version`, and is gone.")]
    [Response(ResponseStatus.UnprocessableEntity, "The code of the feature does not compile. `diagnostics` holds what the compiler said; the feature is kept as it was, to be fixed.",
              Type = typeof(FeatureMergeResponse))]
    [Response(ResponseStatus.Conflict, "The feature is not based on the newest version - the message says which versions to bring in - or it was saved while it was being checked. It is kept as it was.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.BadRequest, "The feature holds exactly what the version it is based on holds, so there is nothing to merge.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<FeatureMergeResponse>> Merge([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.FeatureKey)] string feature,
                                                               MergeFeatureRequest? request)
    {
        var note = new VersionNote(request?.Specification, request?.Change, VersionOrigins.Api);

        var merged = await features.MergeAsync(privateKey, feature, note, request?.Deploy == true);

        var publicKey = meta.PublicKeyOf(privateKey);

        if (merged.Merged)
        {
            logger.LogInformation("Merged feature '{Feature}' of lambda {Lambda} as version {Version}", merged.Name, publicKey, merged.Version?.Version);
        }
        else
        {
            logger.LogInformation("Failed to merge feature '{Feature}' of lambda {Lambda}", merged.Name, publicKey);
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
        var saved = features.Save(privateKey, feature, VersionResource.Serialize(files), new VersionNote(specification, change), after);

        var publicKey = meta.PublicKeyOf(privateKey);

        logger.LogInformation("Saved feature '{Feature}' of lambda {Lambda} files {Files}", saved.Name, publicKey, files!.Count);

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
        => new(feature.Key, feature.Name, feature.Branch, feature.Specification, feature.Change, feature.Base, feature.Newest, feature.Mergeable,
               feature.Origin, feature.Created, feature.Modified, feature.Online, feature.Current, feature.Previewed, feature.Path, feature.Revision);

    private static FeaturePreviewResponse Describe(FeatureDeployment deployment)
        => new(deployment.Success, Describe(deployment.Feature), deployment.Diagnostics);

    #endregion

}
