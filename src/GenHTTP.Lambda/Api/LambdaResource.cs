using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// A lambda is addressed by its editor key rather than its public one, because
/// the key in the path is what authorizes the call - whoever has it may edit
/// the lambda. Its versions, deployment, files and code live in resources of
/// their own below the same path.
///
/// What is done here is logged by the lambda's public key, never by the key in
/// the path - so are the other resources below it.
/// </remarks>
[Tag(ApiDocumentation.Lambdas)]
[Summary("The lambdas themselves: created, read, moved to another key, exported and deleted.")]
[Remarks("""
         A lambda is addressed by its **editor key** (`privateKey`), which is also what authorizes every call below
         `/lambdas/{privateKey}`. Its versions, deployment, data and everything else about it are below the same path.

         Where a lambda answers: `publicUrl` is its address below the hosting domain, named after its public key.
         `address` is where to link to it - its own domain while it answers at one (premium only), `publicUrl` otherwise.
         """)]
public sealed class LambdaResource(IMetaService meta, ISecretService secrets, ISourceService sources, IDatabaseService databases, LambdaOptions options,
                                   ILogger<LambdaResource> logger)
{

    [ResourceMethod(Method.Post, "lambdas")]
    [Summary("Creates a lambda")]
    [Remarks("""
             The new lambda is in the free tier and has one version: a copy of the `template`, or a snippet that says hello if
             none is given. Nothing is online until a version is deployed (`POST /lambdas/{privateKey}/deployment/start`, or a
             save with `?deploy=true`). A copy of a demo that keeps records starts with an empty database of its own, switched on;
             the demo's records are not copied.

             The answer holds the editor key (`privateKey`). It is handed out this once and cannot be recovered: whoever loses it
             loses the lambda, and whoever finds it may change it.

             A `publicKey` that is asked for is trimmed and lower cased, and must be 3 to 40 characters of `a-z`, `0-9` and `-`,
             neither starting nor ending with a dash. It may not start with `demo-` or `xn--`, and a few names are reserved for
             the platform (`www`, `api`, `admin`, `mail` and the like). `GET /keys/{publicKey}` says whether a key can be claimed
             before trying. Left out, a random key of eight characters is chosen.
             """)]
    [Response(ResponseStatus.Created, "The lambda was created. Keep `privateKey`: it is not handed out again.")]
    [Response(ResponseStatus.BadRequest, "The terms were not accepted, or the key, the template or the view cannot be used. The message says which.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Another lambda has the public key that was asked for.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public Result<LambdaResponse> Create(CreateLambdaRequest request)
    {
        if (!request.AcceptedTerms)
        {
            throw LambdaException.Invalid("The terms of service need to be accepted.");
        }

        var lambda = meta.Create(request.PublicKey, request.Template, EditorViews.Parse(request.View) ?? EditorView.Full);

        logger.LogInformation("Created lambda {Lambda} template {Template} view {View}", lambda.PublicKey, request.Template ?? "(none)", lambda.View);

        return new Result<LambdaResponse>(LambdaDescription.Of(lambda)).Status(ResponseStatus.Created);
    }

    [ResourceMethod("lambdas/:privateKey")]
    [Summary("Reads a lambda")]
    [Remarks("Its keys and tier, its newest version and the one online, until when it stays online and is kept, and where it answers.")]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda")]
    public LambdaResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => LambdaDescription.Of(meta.Require(privateKey));

    /// <remarks>
    /// The view is read before anything changes, so a request that names one
    /// that does not exist changes nothing at all rather than half of what it
    /// asked for.
    /// </remarks>
    [ResourceMethod(Method.Patch, "lambdas/:privateKey")]
    [Summary("Changes the public key or the view of a lambda")]
    [Remarks("""
             What the request leaves out stays as it is.

             A new `publicKey` follows the rules of `POST /lambdas` and moves the address the lambda answers at, at once: nothing
             answers at the old address any more, and anybody may claim it. A domain of the lambda's own is not affected.

             The `view` is only what the editor opens in for somebody who has not picked a view of their own.
             """)]
    [Response(ResponseStatus.BadRequest, "The key or the view cannot be used. Nothing was changed.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Conflict, "Another lambda has the public key that was asked for.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The lambda afterwards")]
    public LambdaResponse Update([Summary(ApiDocumentation.PrivateKey)] string privateKey, UpdateLambdaRequest request)
    {
        var view = EditorViews.Parse(request.View);

        var lambda = meta.Require(privateKey);

        if (request.PublicKey is { } publicKey)
        {
            var before = lambda.PublicKey;

            lambda = meta.ChangeKey(privateKey, publicKey);

            logger.LogInformation("Changed public key of lambda {Before} to {Lambda}", before, lambda.PublicKey);
        }

        if (view is { } wanted)
        {
            lambda = meta.ChangeView(privateKey, wanted);

            logger.LogInformation("Set view of lambda {Lambda} to {View}", lambda.PublicKey, lambda.View);
        }

        return LambdaDescription.Of(lambda);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey")]
    [Summary("Deletes a lambda for good")]
    [Remarks("""
             Takes it offline and removes everything kept for it: every version, every feature with its preview, its database,
             workspace and secrets, its showcase entry and published source, and what was counted of its visitors. This cannot be
             undone, and its public key is free for anybody to claim afterwards. `GET …/export` takes a copy first.
             """)]
    [Response(ResponseStatus.NoContent, "The lambda and everything kept for it are gone.")]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public void Delete([Summary(ApiDocumentation.PrivateKey)] string privateKey)
    {
        var publicKey = meta.PublicKeyOf(privateKey);

        meta.Delete(privateKey);

        logger.LogInformation("Deleted lambda {Lambda}", publicKey);
    }

    /// <remarks>
    /// A way out. Whatever somebody writes here runs on a machine they do not
    /// own, for as long as it is left running; being able to take it away is
    /// the difference between a place to build something and a place it is
    /// stuck.
    /// </remarks>
    [ResourceMethod("lambdas/:privateKey/export")]
    [Summary("Exports the lambda as a .NET project")]
    [Remarks("""
             A zip of the **newest** version - not necessarily the one online - as a .NET 10 project on GenHTTP: a `Program.cs`
             that hosts the snippet, stand-ins for what the platform provides in `Platform/`, and a `Dockerfile`. `dotnet run` or
             `docker build` it, and it serves what the lambda serves.

             The database comes along as `database/database.db`, a copy taken while the request is answered, which the project
             opens as the lambda opened it here. The workspace does not come along, and neither do the values of the secrets: the
             project reads them from environment variables of the same names, which `Program.cs` lists. A published source
             carries its `LICENSE`. Exporting changes nothing about the lambda.
             """)]
    [Response(ResponseStatus.Ok, "The project, as a zip archive", ContentType = ApiDocumentation.Zip)]
    [ResponseHeader(ResponseStatus.Ok, "Content-Disposition", "`attachment`, named after the public key: `quiz.zip`")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or it has no version to export.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    public async ValueTask<IResponse> Export([Summary(ApiDocumentation.PrivateKey)] string privateKey, IRequest request)
    {
        var lambda = meta.Require(privateKey);

        if (lambda.LatestVersion is not { } latest)
        {
            throw LambdaException.NotFound("That lambda has nothing saved to take away yet.");
        }

        var content = meta.GetVersion(privateKey, latest);

        var address = lambda.Address;

        // the names say which variables to set; the values stay here
        var kept = secrets.List(privateKey);

        var names = kept.Secrets.Select(s => s.Name).Union(kept.Used, StringComparer.Ordinal).ToList();

        // a lambda whose source is published is taken away under the same
        // license everybody else downloads it under
        var published = sources.Get(privateKey) is { Published: true } source ? source : null;

        var license = published != null && SourceLicenses.Find(published.License) is { } found
            ? new ExportedLicense(found, SourceLicenses.Holder(published.Author, lambda.PublicKey),
                                  options.PublicUrl is { } root ? $"{root}/source/{lambda.PublicKey}" : null)
            : null;

        var exported = new ExportedLambda(lambda.PublicKey, content.Version, content.Created, content.Change, address, DateTime.UtcNow, names, license);

        var id = meta.RequireId(privateKey);

        var archive = await Offload.Run(() =>
        {
            // a copy of what the lambda keeps, packed along and then let go
            var database = databases.Export(id);

            var target = Path.Combine(Path.GetTempPath(), $"genhttp-lambda-export-{Guid.NewGuid():N}.zip");

            try
            {
                using var stream = File.Create(target);

                ProjectPacker.Pack(exported, LambdaSource.Parse(content.Code), stream, database);

                return target;
            }
            catch
            {
                File.Delete(target);
                throw;
            }
            finally
            {
                if (database != null)
                {
                    File.Delete(database);
                }
            }
        });

        logger.LogInformation("Exported lambda {Lambda} version {Version}", lambda.PublicKey, content.Version);

        // packed into a file rather than into memory, because a project carries
        // the database along, which may be as large as the tier allows - and
        // deleted once it has been sent
        var stream = new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, BufferSize.Write, FileOptions.DeleteOnClose);

        return request.Respond()
                      .Content(stream, new ContentType("application/zip"), (ulong)stream.Length)
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}.zip\"")
                      .Build();
    }

}
