using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Source;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas themselves: made, read, renamed, removed.
/// </summary>
/// <remarks>
/// A lambda is addressed by its editor key rather than its public one, because
/// the key in the path is what authorizes the call - whoever has it may edit
/// the lambda. Its versions, deployment, files and code live in resources of
/// their own below the same path.
///
/// What is done here is logged by the lambda's public key, never by the key in
/// the path - so are the other resources below it.
/// </remarks>
public sealed class LambdaResource(IMetaService meta, ISecretService secrets, ISourceService sources, DatabaseVault databases, LambdaOptions options,
                                   ILogger<LambdaResource> logger)
{

    /// <summary>
    /// Creates a new lambda and returns its keys.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas")]
    public async ValueTask<Result<LambdaResponse>> Create(CreateLambdaRequest request)
    {
        if (!request.AcceptedTerms)
        {
            throw LambdaException.Invalid("The terms of service need to be accepted.");
        }

        var lambda = await meta.CreateAsync(request.PublicKey, request.Template, EditorViews.Parse(request.View) ?? EditorView.Full);

        logger.LogInformation("Created lambda {Lambda} from template {Template}, opening in the {View} view", lambda.PublicKey, request.Template ?? "(none)", lambda.View);

        return new Result<LambdaResponse>(LambdaDescription.Of(lambda)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// Reads a lambda by the private key of its editor.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey")]
    public async ValueTask<LambdaResponse> Get(string privateKey)
        => LambdaDescription.Of(await meta.RequireAsync(privateKey));

    /// <summary>
    /// Changes a lambda. What the request leaves out stays as it is.
    /// </summary>
    /// <remarks>
    /// Moving the public key moves the address the lambda answers at, and the
    /// old one is free for anybody to claim afterwards. The view is only what
    /// the editor opens in for somebody who has not picked one: whoever has
    /// keeps theirs, since that is remembered in their browser.
    ///
    /// The view is read before anything changes, so a request that names one
    /// that does not exist changes nothing at all rather than half of what it
    /// asked for.
    /// </remarks>
    [ResourceMethod(Method.Patch, "lambdas/:privateKey")]
    public async ValueTask<LambdaResponse> Update(string privateKey, UpdateLambdaRequest request)
    {
        var view = EditorViews.Parse(request.View);

        var lambda = await meta.RequireAsync(privateKey);

        if (request.PublicKey is { } publicKey)
        {
            var before = lambda.PublicKey;

            lambda = await meta.ChangeKeyAsync(privateKey, publicKey);

            logger.LogInformation("Moved lambda {Before} to the public key {Lambda}", before, lambda.PublicKey);
        }

        if (view is { } wanted)
        {
            lambda = await meta.ChangeViewAsync(privateKey, wanted);

            logger.LogInformation("Set the editor of lambda {Lambda} to open in the {View} view", lambda.PublicKey, lambda.View);
        }

        return LambdaDescription.Of(lambda);
    }

    /// <summary>
    /// Removes the lambda for good.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey")]
    public async ValueTask Delete(string privateKey)
    {
        var publicKey = await meta.PublicKeyOfAsync(privateKey);

        await meta.DeleteAsync(privateKey);

        logger.LogInformation("Deleted lambda {Lambda}", publicKey);
    }

    /// <summary>
    /// The newest version of the lambda as a .NET 10 project, with a
    /// Dockerfile, that can be opened and run without this platform - and its
    /// database, where it has one.
    /// </summary>
    /// <remarks>
    /// A way out. Whatever somebody writes here runs on a machine they do not
    /// own, for as long as it is left running; being able to take it away is
    /// the difference between a place to build something and a place it is
    /// stuck. The records a lambda keeps are part of what is taken: its
    /// database comes along as database/database.db, which the project opens
    /// as it opened it here. The workspace and the values of the secrets stay.
    /// </remarks>
    /// <param name="privateKey">The lambda being taken away</param>
    [ResourceMethod("lambdas/:privateKey/export")]
    public async ValueTask<IResponse> Export(string privateKey, IRequest request)
    {
        var lambda = await meta.RequireAsync(privateKey);

        if (lambda.LatestVersion is not { } latest)
        {
            throw LambdaException.NotFound("That lambda has nothing saved to take away yet.");
        }

        var content = await meta.GetVersionAsync(privateKey, latest);

        var address = options.PublicUrl is { } site ? $"{site}/lambda/{lambda.PublicKey}/" : null;

        // the names say which variables to set; the values stay here
        var kept = await secrets.ListAsync(privateKey);

        var names = kept.Secrets.Select(s => s.Name).Union(kept.Used, StringComparer.Ordinal).ToList();

        // a lambda whose source is published is taken away under the same
        // license everybody else downloads it under
        var published = await sources.GetAsync(privateKey) is { Published: true } source ? source : null;

        var license = published != null && SourceLicenses.Find(published.License) is { } found
            ? new ExportedLicense(found, SourceLicenses.Holder(published.Author, lambda.PublicKey),
                                  options.PublicUrl is { } root ? $"{root}/source/{lambda.PublicKey}" : null)
            : null;

        var exported = new ExportedLambda(lambda.PublicKey, content.Version, content.Created, content.Change, address, DateTime.UtcNow, names, license);

        var id = await meta.RequireIdAsync(privateKey);

        var archive = await Task.Run(() =>
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

        logger.LogInformation("Exported version {Version} of lambda {Lambda} as a project", content.Version, lambda.PublicKey);

        return request.Respond()
                      .Content(new ExportContent(archive))
                      .Header("Content-Disposition", $"attachment; filename=\"{lambda.PublicKey}.zip\"")
                      .Build();
    }

}
