using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

using GenHTTP.Modules.IO;
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
public sealed class VersionResource(IMetaService meta, ILimitsService limits, ILogger<VersionResource> logger)
{

    /// <summary>
    /// Lists the stored versions, newest first.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions")]
    public List<VersionResponse> List(string privateKey)
    {
        var versions = meta.GetVersions(privateKey);

        return versions.Select(Describe).ToList();
    }

    /// <summary>
    /// Reads the files of a single version.
    /// </summary>
    /// <remarks>
    /// Every file unless a folder is named. Its documentation is
    /// <c>?folder=.lambda/docs/</c>, its tests <c>?folder=.lambda/tests/</c>
    /// and its build folder - what its assets are built from -
    /// <c>?folder=.lambda/build/</c>, which is how they are read without every
    /// asset of the version coming along.
    /// </remarks>
    /// <param name="folder">Only the files below this folder, such as <c>.lambda/docs/</c></param>
    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    public VersionContentResponse Get(string privateKey, int version, string? folder)
        => Describe(meta.GetVersion(privateKey, version), folder);

    /// <summary>
    /// Downloads the files of a single version as a zip archive.
    /// </summary>
    /// <remarks>
    /// The archive holds the files as they are named in the lambda, so it can
    /// be changed locally and uploaded again as a new version. With
    /// <c>?layout=project</c> it holds them where a clone has them instead -
    /// <c>Project.cs</c>, <c>assets/</c>, <c>docs/</c>, <c>tests/</c>,
    /// <c>build/</c> - so a build set up in a clone writes to the same place.
    /// </remarks>
    /// <param name="layout"><c>lambda</c> (the default) or <c>project</c></param>
    [ResourceMethod("lambdas/:privateKey/versions/:version/zip")]
    public IResponse GetArchive(string privateKey, int version, string? layout, IRequest request)
    {
        var laid = LayoutOf(layout);

        var lambda = meta.Require(privateKey);

        var content = meta.GetVersion(privateKey, version);

        var zip = LambdaArchive.Pack(LambdaSource.Parse(content.Code), laid);

        logger.LogInformation("Downloaded lambda {Lambda} version {Version} layout {Layout}", lambda.PublicKey, version, laid);

        return request.Respond()
                      .Content(zip, new ContentType("application/zip"))
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}-v{version}{(laid == ArchiveLayout.Project ? "-project" : string.Empty)}.zip\"")
                      .Build();
    }

    /// <summary>
    /// Stores the files of a zip archive as a new version.
    /// </summary>
    /// <remarks>
    /// The archive replaces the whole set of files, so it has to hold all of
    /// them - a file left out is gone from the new version. Hidden files and
    /// folders are skipped, and a single top level folder is removed. The
    /// build folder in <c>.lambda/build/</c> keeps its dot files and
    /// leaves out what its own <c>.gitignore</c> files ignore, as git does.
    /// With <c>?layout=project</c>, the archive is laid out as a clone and read
    /// as a commit of it would be: the platform's files are left out, and so
    /// is what the repository ignores, <c>bin/</c> and <c>obj/</c> among it.
    /// The archive is counted as it is sent, before anything is left out, so
    /// it holds what a commit would - never what a build installed.
    /// </remarks>
    /// <param name="deploy">Whether to put the new version online as well</param>
    /// <param name="specification">What the user wants from this version and why</param>
    /// <param name="change">What this version changes, in a line</param>
    /// <param name="layout"><c>lambda</c> (the default) or <c>project</c></param>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/zip")]
    public async ValueTask<Result<SavedVersionResponse>> CreateFromArchive(string privateKey, bool? deploy, string? specification, string? change, string? layout,
                                                                           Stream body)
    {
        var laid = LayoutOf(layout);

        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = meta.Require(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        var files = await UnpackAsync(body, limits.MaxCodeLengthOf(tier) * 4L + limits.MaxAssetBytesOf(tier), laid, Latest(meta, privateKey));

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
        var files = LambdaChanges.Apply(Latest(meta, privateKey), request.Files, request.Remove, request.Edits);

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

        var version = meta.Save(privateKey, Serialize(files), note);

        var publicKey = meta.PublicKeyOf(privateKey);

        logger.LogInformation("Saved lambda {Lambda} version {Version} files {Files}", publicKey, version.Version, files!.Count);

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
    /// The layout an archive is asked for in.
    /// </summary>
    internal static ArchiveLayout LayoutOf(string? layout) => layout?.Trim().ToLowerInvariant() switch
    {
        null or "" or "lambda" => ArchiveLayout.Lambda,
        "project" => ArchiveLayout.Project,
        _ => throw LambdaException.Invalid($"There is no layout '{layout}': 'lambda', the default, names the files as the lambda does, 'project' lays them out as a clone does.")
    };

    /// <summary>
    /// The files of an uploaded archive in the given layout.
    /// </summary>
    /// <param name="basis">The files it was made from: what it already has is kept whatever a .gitignore says, and a project's names are read against them</param>
    internal static ValueTask<IReadOnlyList<LambdaFile>> UnpackAsync(Stream body, long maxBytes, ArchiveLayout layout, IReadOnlyList<LambdaFile> basis)
        => layout == ArchiveLayout.Project ? LambdaArchive.UnpackProjectAsync(body, maxBytes, basis) : LambdaArchive.UnpackAsync(body, maxBytes, basis);

    /// <summary>
    /// The files of the newest version of a lambda.
    /// </summary>
    internal static IReadOnlyList<LambdaFile> Latest(IMetaService meta, string privateKey)
    {
        var lambda = meta.Require(privateKey);

        if (lambda.LatestVersion is not { } latest)
        {
            return [];
        }

        return LambdaSource.Parse((meta.GetVersion(privateKey, latest)).Code);
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
