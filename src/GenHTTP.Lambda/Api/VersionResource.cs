using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The saved versions of a lambda's code.
/// </summary>
/// <remarks>
/// Versions are never changed once they are stored and never removed one by
/// one, so there is nothing to put or delete here: a change to the code is a
/// new version, and the history goes with the lambda.
/// </remarks>
public sealed class VersionResource(IMetaService meta, LambdaOptions options)
{

    /// <summary>
    /// Lists the stored versions, newest first.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions")]
    public async ValueTask<List<VersionResponse>> List(string privateKey)
    {
        var versions = await meta.GetVersionsAsync(privateKey);

        return versions.Select(v => new VersionResponse(v.Version, v.Created)).ToList();
    }

    /// <summary>
    /// Reads the code of a single version.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    public async ValueTask<VersionContentResponse> Get(string privateKey, int version)
    {
        var content = await meta.GetVersionAsync(privateKey, version);

        return new VersionContentResponse(content.Version, content.Created, LambdaSource.Parse(content.Code));
    }

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

        return request.Respond()
                      .Content(new ZippedProject(zip))
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
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/zip")]
    public async ValueTask<Result<SavedVersionResponse>> CreateFromArchive(string privateKey, bool? deploy, Stream body)
    {
        var files = await LambdaArchive.UnpackAsync(body, options.MaxCodeLength * 4L + options.MaxAssetBytes);

        return await SaveAsync(privateKey, files, deploy);
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

        return await SaveAsync(privateKey, files, deploy);
    }

    /// <summary>
    /// Stores the code as a new version.
    /// </summary>
    /// <param name="deploy">Whether to put the new version online as well</param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions")]
    public async ValueTask<Result<SavedVersionResponse>> Create(string privateKey, bool? deploy, VersionRequest request)
        => await SaveAsync(privateKey, request.Files, deploy);

    /// <summary>
    /// Stores the files as a new version and deploys it if asked to.
    /// </summary>
    /// <remarks>
    /// Answers with 201 either way, because the version was stored; whether
    /// it went online is in the deployment it carries.
    /// </remarks>
    private async ValueTask<Result<SavedVersionResponse>> SaveAsync(string privateKey, IReadOnlyList<LambdaFile>? files, bool? deploy)
    {
        var version = await meta.SaveAsync(privateKey, Serialize(files));

        DeploymentOutcomeResponse? deployment = null;

        if (deploy == true)
        {
            var result = await meta.DeployAsync(privateKey, version.Version);

            deployment = new DeploymentOutcomeResponse(result.Success, result.Lambda == null ? null : LambdaDescription.Of(result.Lambda), result.Diagnostics);
        }

        return new Result<SavedVersionResponse>(new SavedVersionResponse(version.Version, version.Created, deployment)).Status(ResponseStatus.Created);
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
