using GenHTTP.Api.Content.Services;
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

/// <remarks>
/// Versions are never changed once they are stored and never removed one by
/// one, so there is nothing to put or delete here: a change to the code is a
/// new version, and the history goes with the lambda.
///
/// Every way of storing one takes an optional specification and change: what
/// the user wanted and what was done about it, which the code alone cannot say.
/// </remarks>
[Tag(ApiDocumentation.Versions)]
[Summary("The saved versions of a lambda: its program, which never changes once it is saved.")]
[Remarks("""
         A version is the program: its code and its resources, and what is written about it (`docs/`, `tests/`). It is
         numbered from 1 and never changes; a change to the lambda is a new version. Saving one does not put it online -
         pass `?deploy=true`, or deploy it afterwards. Data (database, workspace, secrets) is not part of a version.

         Every save may say what the user wanted (`specification`) and what it changes (`change`), which the history shows.
         Only so many versions are kept, as the tier of the lambda allows; with every save the oldest beyond that go, but
         never the version online or one a feature is based on.

         A lambda that exists is best changed in a feature (see *Features*) rather than by saving versions directly: the
         feature can be tried at its own address first.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class VersionResource(IMetaService meta, ILimitsService limits, ILogger<VersionResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/versions")]
    [Summary("Lists the versions that are kept, newest first")]
    [Remarks("Without their files. `GET …/versions/{version}` reads one with them.")]
    [return: Summary("The versions, newest first")]
    public List<VersionResponse> List([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var versions = meta.GetVersions(privateKey);

        return versions.Select(Describe).ToList();
    }

    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    [Summary("Reads a version with its files")]
    [Remarks("""
             Every file, `lambda.cs` first, unless a `folder` is named: `?folder=docs/` reads the documentation of the version
             and `?folder=tests/` its tests, without every resource coming along.
             """)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no such version - it never existed, or it was not kept.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The version with its files")]
    public VersionContentResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.VersionNumber)] int version,
                                      [Summary("Only the files whose name starts with this, such as `docs/` or `resources/`")] string? folder)
        => Describe(meta.GetVersion(privateKey, version), folder);

    [ResourceMethod("lambdas/:privateKey/versions/:version/zip")]
    [Summary("Downloads a version as a zip archive")]
    [Remarks("""
             The archive holds the files as the lambda names them, so it can be changed locally and saved again as a new version
             with `POST …/versions/zip`. With `?layout=project` it holds them where a clone of the lambda's git repository has
             them instead, so a .NET tool set up in a clone finds them where it looks. Unlike the export, it holds no project
             around them and no data.
             """)]
    [Response(ResponseStatus.Ok, "The files, as a zip archive", ContentType = ApiDocumentation.Zip)]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, named after the public key and the version: `quiz-v3.zip`, or `quiz-v3-project.zip`")]
    [Response(ResponseStatus.BadRequest, "There is no such layout.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no such version.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public IResponse GetArchive([Summary(ApiDocumentation.PrivateKey)] string privateKey, [Summary(ApiDocumentation.VersionNumber)] int version,
                                [Summary(ApiDocumentation.Layout)] string? layout, IRequest request)
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

    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/zip")]
    [Summary("Saves a zip archive as a new version")]
    [Remarks("""
             The archive replaces every file, so it has to hold all of them: a file it leaves out is not in the new version.

             It is read as a commit of the files would be. A single folder around everything is removed. A git repository is
             skipped, and so is what the `.gitignore` files in the archive leave out - except files the newest version already
             has, which are kept as git keeps what it tracks. Other dot files of the code are kept; hidden files among the
             resources are skipped. Text files are kept as text, anything that is not UTF-8 as base64. With `?layout=project`
             the archive is read as a clone of the lambda's git repository has it, and the platform's own files are left out.

             The archive is counted as it is sent, before anything is left out, against what a version may come to. Zip what a
             commit would hold - never what a build installed (`node_modules`, for example).
             """)]
    [RequestBody("A zip archive of every file of the lambda", ContentType = ApiDocumentation.Zip)]
    [Response(ResponseStatus.Created, "The version was saved. If it was to be deployed as well, `deployment` says whether that worked.")]
    [Response(ResponseStatus.BadRequest, "The body is not a zip archive, it is larger than a version may be, it has no `lambda.cs`, or a file in it cannot be part of a lambda. The message says which.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<SavedVersionResponse>> CreateFromArchive([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                                                           [Summary(ApiDocumentation.Deploy)] bool? deploy,
                                                                           [Summary(ApiDocumentation.Specification)] string? specification,
                                                                           [Summary(ApiDocumentation.Change)] string? change,
                                                                           [Summary(ApiDocumentation.Layout)] string? layout,
                                                                           Stream body)
    {
        var laid = LayoutOf(layout);

        // looked up before the body is read, because how much of it may be
        // read depends on the tier - and a key that names nothing needs none
        var lambda = meta.Require(privateKey);

        var tier = Enum.Parse<LambdaTier>(lambda.Tier);

        var files = await UnpackAsync(body, limits.BuildOf(tier), laid, Latest(meta, privateKey));

        return await SaveAsync(privateKey, files, deploy, specification, change);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions/changes")]
    [Summary("Saves changes to some files as a new version")]
    [Remarks("""
             Files that are not named stay as they are, so a small change does not send every file again. The changes are
             applied in this order: `remove` takes files out, `files` adds files or replaces those of the same name, and `edits`
             replaces text within files - each `find` has to occur exactly once in its file, or nothing is saved. What results
             has to be a valid lambda, as for any save.
             """)]
    [Response(ResponseStatus.Created, "The version was saved. If it was to be deployed as well, `deployment` says whether that worked.")]
    [Response(ResponseStatus.BadRequest, "Nothing to change, a file to remove or edit that does not exist, a `find` that occurs never or more than once, or a result that cannot be a lambda. Nothing was saved.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<SavedVersionResponse>> Change([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                                                [Summary(ApiDocumentation.Deploy)] bool? deploy, VersionChangeRequest request)
    {
        var files = LambdaChanges.Apply(Latest(meta, privateKey), request.Files, request.Remove, request.Edits);

        return await SaveAsync(privateKey, files, deploy, request.Specification, request.Change);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions")]
    [Summary("Saves every file of the lambda as a new version")]
    [Remarks("""
             `files` is the whole lambda, `lambda.cs` first: a file the newest version has and this request does not is not in
             the new version. To change a few files, `POST …/versions/changes` sends only those.

             A file name may be refused - a C# file has to be named as C# names its files, a resource needs an extension, and
             no name may hold spaces or be one Windows reserves; the message says what is wrong with which. The files together
             may not come to more than the tier of the lambda allows a version (see the limits). Whether the code compiles is
             not checked unless it is deployed: `POST …/code/check` compiles it without saving.
             """)]
    [Response(ResponseStatus.Created, "The version was saved. If it was to be deployed as well, `deployment` says whether that worked.")]
    [Response(ResponseStatus.BadRequest, "There are no files, `lambda.cs` is not the first, a name or an encoding cannot be used, or the files are larger than a version may be. Nothing was saved.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<Result<SavedVersionResponse>> Create([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                                                [Summary(ApiDocumentation.Deploy)] bool? deploy, VersionRequest request)
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
