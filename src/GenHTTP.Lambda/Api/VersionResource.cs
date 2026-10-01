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

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The saved versions of a lambda's code.
/// </summary>
/// <remarks>
/// Versions are never changed once they are stored and never removed one by
/// one, so there is nothing to put or delete here: a change to the code is a
/// new version, and the history goes with the lambda.
///
/// Every way of storing one takes an optional specification and change: what
/// the user wanted and what was done about it, which the code alone cannot say.
/// </remarks>
public sealed class VersionResource(IMetaService meta, LambdaOptions options, ILogger<VersionResource> logger)
{

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
    /// Reads the files of a single version.
    /// </summary>
    /// <remarks>
    /// Every file unless a folder is named. Its documentation and its tests
    /// are <c>?folder=.lambda/</c>, which is how they are read without every
    /// asset of the version coming along.
    /// </remarks>
    /// <param name="folder">Only the files below this folder, such as <c>.lambda/</c></param>
    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    public async ValueTask<VersionContentResponse> Get(string privateKey, int version, string? folder)
        => Describe(await meta.GetVersionAsync(privateKey, version), folder);

    /// <summary>
    /// Downloads the files of a single version as a zip archive.
    /// </summary>
    /// <remarks>
    /// The archive holds the files as they are named in the lambda, so it can
    /// be changed locally and uploaded again as a new version.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/versions/:version/zip")]
    public async ValueTask<IResponse> GetArchive(string privateKey, int version, IRequest request)
    {
        var lambda = await meta.RequireAsync(privateKey);

        var content = await meta.GetVersionAsync(privateKey, version);

        var zip = LambdaArchive.Pack(LambdaSource.Parse(content.Code));

        logger.LogInformation("Downloaded version {Version} of lambda {Lambda} as a zip archive", version, lambda.PublicKey);

        return request.Respond()
                      .Content(new BinaryContent(zip, "application/zip"))
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}-v{version}.zip\"")
                      .Build();
    }

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
    {
        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = await meta.RequireAsync(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        var files = await LambdaArchive.UnpackAsync(body, options.MaxCodeLengthOf(tier) * 4L + options.MaxAssetBytesOf(tier));

        return await SaveAsync(privateKey, files, deploy, specification, change);
    }

    /// <summary>
    /// Applies changes to the newest version and stores the result as a new one.
    /// </summary>
    /// <remarks>
    /// Files that are not named stay as they are, so a small change does not
    /// need every file to be sent again.
    /// </remarks>
    /// <param name="deploy">Whether to put the new version online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/changes")]
    public async ValueTask<Result<SavedVersionResponse>> Change(string privateKey, bool? deploy, VersionChangeRequest request)
    {
        var files = LambdaChanges.Apply(await LatestAsync(meta, privateKey), request.Files, request.Remove, request.Edits);

        return await SaveAsync(privateKey, files, deploy, request.Specification, request.Change);
    }

    /// <summary>
    /// Stores the code as a new version.
    /// </summary>
    /// <param name="deploy">Whether to put the new version online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions")]
    public async ValueTask<Result<SavedVersionResponse>> Create(string privateKey, bool? deploy, VersionRequest request)
        => await SaveAsync(privateKey, request.Files, deploy, request.Specification, request.Change);

    /// <summary>
    /// Stores the files as a new version and deploys it if asked to.
    /// </summary>
    /// <remarks>
    /// Answers with 201 either way, because the version was stored; whether
    /// it went online is in the deployment it carries.
    /// </remarks>
    private async ValueTask<Result<SavedVersionResponse>> SaveAsync(string privateKey, IReadOnlyList<LambdaFile>? files, bool? deploy,
                                                                     string? specification, string? change)
    {
        var note = new VersionNote(specification, change, VersionOrigins.Api);

        var version = await meta.SaveAsync(privateKey, Serialize(files), note);

        var publicKey = await meta.PublicKeyOfAsync(privateKey);

        logger.LogInformation("Saved version {Version} of lambda {Lambda} with {Files} file(s)", version.Version, publicKey, files!.Count);

        DeploymentOutcomeResponse? deployment = null;

        if (deploy == true)
        {
            var result = await meta.DeployAsync(privateKey, version.Version, VersionOrigins.Api);

            logger.Deployed(result, publicKey, version.Version);

            deployment = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);
        }

        var saved = new SavedVersionResponse(version.Version, version.Created, version.Specification, version.Change, version.Origin, deployment);

        return new Result<SavedVersionResponse>(saved).Status(ResponseStatus.Created);
    }

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
        => new(version.Version, version.Created, version.Specification, version.Change, version.Origin);

    internal static VersionContentResponse Describe(LambdaVersionContent content, string? folder = null)
        => new(content.Version, content.Created, content.Specification, content.Change, content.Origin, Below(LambdaSource.Parse(content.Code), folder));

    /// <summary>
    /// The files below a folder, or all of them when none is named.
    /// </summary>
    internal static IReadOnlyList<LambdaFile> Below(IReadOnlyList<LambdaFile> files, string? folder)
        => string.IsNullOrEmpty(folder) ? files : [.. files.Where(f => f.Name.StartsWith(folder, StringComparison.Ordinal))];

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

}
