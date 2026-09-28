using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The saved versions of a lambda's code.
/// </summary>
/// <remarks>
/// A version is the program - every C# file and every asset - and the newest
/// one is the one being worked on: it can be saved over in place, with a put of
/// all its files or a patch of some, as often as it takes, and deployed again
/// each time. The versions before it are history. They never change again and
/// are never removed one by one; to carry on from one, copy it, and the copy
/// becomes the newest. So a new version is a decision - the next thing somebody
/// asked for - rather than what every save happens to produce.
///
/// Every way of storing one takes an optional specification and change: what
/// the user wanted and what was done about it, which the code alone cannot say.
/// Saved over, a version keeps the ones it has unless new ones are given.
/// </remarks>
public sealed class VersionResource(IMetaService meta, LambdaOptions options)
{

    #region Reading

    /// <summary>
    /// Lists the stored versions, newest first.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions")]
    public async ValueTask<List<VersionResponse>> List(string privateKey)
    {
        var versions = await meta.GetVersionsAsync(privateKey);

        return versions.Select(Describe).ToList();
    }

    /// <summary>
    /// Reads the code of a single version.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    public async ValueTask<VersionContentResponse> Get(string privateKey, int version)
        => Describe(await meta.GetVersionAsync(privateKey, version));

    /// <summary>
    /// Downloads the files of a single version as a zip archive.
    /// </summary>
    /// <remarks>
    /// The archive holds the files as they are named in the lambda, so it can
    /// be changed locally and uploaded again - over the newest version with a
    /// put, or as a new one.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/versions/:version/zip")]
    public async ValueTask<IResponse> GetArchive(string privateKey, int version, IRequest request)
    {
        var lambda = await meta.RequireAsync(privateKey);

        var content = await meta.GetVersionAsync(privateKey, version);

        var zip = LambdaArchive.Pack(LambdaSource.Parse(content.Code));

        return request.Respond()
                      .Content(new BinaryContent(zip, "application/zip"))
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}-v{version}.zip\"")
                      .Build();
    }

    #endregion

    #region New versions

    /// <summary>
    /// Stores the code as a new version.
    /// </summary>
    /// <remarks>
    /// For the next thing the user asked for. While working on it, save over
    /// the version this creates with a put or a patch rather than adding
    /// another version for every change.
    /// </remarks>
    /// <param name="deploy">Whether to put the new version online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions")]
    public async ValueTask<Result<SavedVersionResponse>> Create(string privateKey, bool? deploy, VersionRequest request)
        => await CreateAsync(privateKey, request.Files, deploy, request.Specification, request.Change);

    /// <summary>
    /// Stores the files of a zip archive as a new version.
    /// </summary>
    /// <remarks>
    /// The archive replaces the whole set of files, so it has to hold all of
    /// them - a file left out is gone from the new version. Hidden files and
    /// folders are skipped, and a single top level folder is removed.
    /// </remarks>
    /// <param name="deploy">Whether to put the new version online as well</param>
    /// <param name="specification">What the user wants from this version and why</param>
    /// <param name="change">What this version changes, in a line</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/zip")]
    public async ValueTask<Result<SavedVersionResponse>> CreateFromArchive(string privateKey, bool? deploy, string? specification, string? change, Stream body)
        => await CreateAsync(privateKey, await UnpackAsync(privateKey, body), deploy, Decode(specification), Decode(change));

    /// <summary>
    /// Applies changes to the newest version and stores the result as a new one.
    /// </summary>
    /// <remarks>
    /// Files that are not named stay as they are, so a small change does not
    /// need every file to be sent again. To change the newest version where it
    /// is instead, patch it.
    /// </remarks>
    /// <param name="deploy">Whether to put the new version online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/changes")]
    public async ValueTask<Result<SavedVersionResponse>> Change(string privateKey, bool? deploy, VersionChangeRequest request)
    {
        var files = LambdaChanges.Apply(await LatestAsync(meta, privateKey), request.Files, request.Remove, request.Edits);

        return await CreateAsync(privateKey, files, deploy, request.Specification, request.Change);
    }

    /// <summary>
    /// Starts a new version as a copy of this one.
    /// </summary>
    /// <remarks>
    /// Nothing has to be sent: the copy becomes the newest version, to be
    /// worked on in place, and the one it was copied from stays as it is.
    /// Copying the newest keeps it as a step to go back to before the next
    /// piece of work; copying an older one carries on from there.
    /// </remarks>
    /// <param name="version">The version to copy</param>
    /// <param name="deploy">Whether to put the copy online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/:version/copy")]
    public async ValueTask<Result<SavedVersionResponse>> Copy(string privateKey, int version, bool? deploy, CopyVersionRequest? request)
    {
        var copied = await meta.CopyAsync(privateKey, version, new VersionNote(request?.Specification, request?.Change, VersionOrigins.Api));

        return await RespondAsync(privateKey, copied, deploy, ResponseStatus.Created);
    }

    #endregion

    #region Working on the newest

    /// <summary>
    /// Saves all files over the newest version.
    /// </summary>
    /// <remarks>
    /// The way to keep working on something: change, deploy, look, change
    /// again - all in one version. Only the newest version can be saved over;
    /// any other is answered with 409 and what to do instead. The version
    /// online, saved over, goes on serving what was deployed until it is
    /// deployed again, which <c>deploy=true</c> does in the same request.
    /// </remarks>
    /// <param name="version">The number of the newest version</param>
    /// <param name="deploy">Whether to put it online as well</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/versions/:version")]
    public async ValueTask<Result<SavedVersionResponse>> Update(string privateKey, int version, bool? deploy, VersionRequest request)
        => await UpdateAsync(privateKey, version, request.Files, deploy, request.Specification, request.Change);

    /// <summary>
    /// Saves the files of a zip archive over the newest version.
    /// </summary>
    /// <remarks>
    /// The archive holds every file of the version, as for a new one. Download
    /// the version, change it locally, and put it back as often as it takes.
    /// </remarks>
    /// <param name="version">The number of the newest version</param>
    /// <param name="deploy">Whether to put it online as well</param>
    /// <param name="specification">What the user wants from this version and why; left out, the one it has is kept</param>
    /// <param name="change">What this version changes, in a line; left out, the one it has is kept</param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/versions/:version/zip")]
    public async ValueTask<Result<SavedVersionResponse>> UpdateFromArchive(string privateKey, int version, bool? deploy, string? specification, string? change,
                                                                           Stream body)
        => await UpdateAsync(privateKey, version, await UnpackAsync(privateKey, body), deploy, Decode(specification), Decode(change));

    /// <summary>
    /// Changes some files of the newest version, in place.
    /// </summary>
    /// <remarks>
    /// Files that are not named stay as they are: add or replace files, remove
    /// them, or replace text within them. Only the newest version can be
    /// changed; any other is answered with 409.
    /// </remarks>
    /// <param name="version">The number of the newest version</param>
    /// <param name="deploy">Whether to put it online as well</param>
    [ResourceMethod(Method.Patch, "lambdas/:privateKey/versions/:version")]
    public async ValueTask<Result<SavedVersionResponse>> Patch(string privateKey, int version, bool? deploy, VersionChangeRequest request)
    {
        var current = LambdaSource.Parse((await meta.GetVersionAsync(privateKey, version)).Code);

        var files = LambdaChanges.Apply(current, request.Files, request.Remove, request.Edits);

        return await UpdateAsync(privateKey, version, files, deploy, request.Specification, request.Change);
    }

    #endregion

    #region Helpers

    private async ValueTask<Result<SavedVersionResponse>> CreateAsync(string privateKey, IReadOnlyList<LambdaFile>? files, bool? deploy,
                                                                       string? specification, string? change)
    {
        var version = await meta.SaveAsync(privateKey, Serialize(files), new VersionNote(specification, change, VersionOrigins.Api));

        return await RespondAsync(privateKey, version, deploy, ResponseStatus.Created);
    }

    private async ValueTask<Result<SavedVersionResponse>> UpdateAsync(string privateKey, int version, IReadOnlyList<LambdaFile>? files, bool? deploy,
                                                                       string? specification, string? change)
    {
        var saved = await meta.UpdateAsync(privateKey, version, Serialize(files), new VersionNote(specification, change, VersionOrigins.Api));

        return await RespondAsync(privateKey, saved, deploy, ResponseStatus.Ok);
    }

    /// <summary>
    /// Deploys what was just stored if asked to, and describes both.
    /// </summary>
    /// <remarks>
    /// Answers with the status of the save either way, because the version was
    /// stored; whether it went online is in the deployment it carries.
    /// </remarks>
    private async ValueTask<Result<SavedVersionResponse>> RespondAsync(string privateKey, LambdaVersionInfo version, bool? deploy, ResponseStatus status)
    {
        DeploymentOutcomeResponse? deployment = null;

        if (deploy == true)
        {
            var result = await meta.DeployAsync(privateKey, version.Version, VersionOrigins.Api);

            deployment = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);
        }

        var saved = new SavedVersionResponse(version.Version, version.Created, version.Specification, version.Change, version.Origin,
                                             version.Revision, version.Modified, deployment);

        return new Result<SavedVersionResponse>(saved).Status(status);
    }

    /// <summary>
    /// The files of an archive, read no further than what the tier of the
    /// lambda could keep.
    /// </summary>
    private async ValueTask<IReadOnlyList<LambdaFile>> UnpackAsync(string privateKey, Stream body)
    {
        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = await meta.RequireAsync(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        return await LambdaArchive.UnpackAsync(body, options.MaxCodeLengthOf(tier) * 4L + options.MaxAssetBytesOf(tier));
    }

    /// <summary>
    /// A note sent in the query, as it was written.
    /// </summary>
    /// <remarks>
    /// Query values arrive exactly as they were sent, so a change of "Says
    /// hello" sent as <c>Says%20hello</c> - which is how curl and every URL
    /// builder send it - was kept with the percent sign in it. A plus is a
    /// space there too, the way a form encodes one.
    /// </remarks>
    private static string? Decode(string? value)
        => value == null ? null : Uri.UnescapeDataString(value.Replace('+', ' '));

    /// <summary>
    /// The files of the newest version of a lambda.
    /// </summary>
    internal static async ValueTask<IReadOnlyList<LambdaFile>> LatestAsync(IMetaService meta, string privateKey)
    {
        var lambda = await meta.RequireAsync(privateKey);

        if (lambda.LatestVersion is not { } latest)
        {
            return [];
        }

        return LambdaSource.Parse((await meta.GetVersionAsync(privateKey, latest)).Code);
    }

    internal static VersionResponse Describe(LambdaVersionInfo version)
        => new(version.Version, version.Created, version.Specification, version.Change, version.Origin, version.Revision, version.Modified);

    internal static VersionContentResponse Describe(LambdaVersionContent content)
        => new(content.Version, content.Created, content.Specification, content.Change, content.Origin, content.Revision, content.Modified,
               LambdaSource.Parse(content.Code));

    /// <summary>
    /// Turns what was submitted into the single blob a version is stored as.
    /// </summary>
    internal static string Serialize(IReadOnlyList<LambdaFile>? files)
    {
        if (LambdaSource.Validate(files) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        return LambdaSource.Serialize(files!);
    }

    #endregion

}
