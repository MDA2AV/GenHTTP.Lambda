using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Every lambda on this installation, for whoever runs it.
/// </summary>
/// <remarks>
/// Everything else in this API is authorized by holding the editor link of the
/// one lambda it touches. This resource is the exception: it reads code that
/// belongs to other people and can take their lambdas down, so it asks for a
/// token instead - and answers nothing at all until one is configured, because
/// a panel that is off cannot be left open by accident.
/// </remarks>
public sealed class AdminResource(IMetaService meta, LambdaOptions options)
{

    #region Functionality

    /// <summary>
    /// Every lambda, newest first.
    /// </summary>
    [ResourceMethod("lambdas")]
    public async ValueTask<AdminListingResponse> GetLambdas(IRequest request)
    {
        Authorize(request);

        var lambdas = await meta.ListAsync();

        return new AdminListingResponse(lambdas, lambdas.Count, lambdas.Count(l => l.ActiveVersion != null));
    }

    /// <summary>
    /// The code of one lambda, at the given version or the newest one.
    /// </summary>
    [ResourceMethod("lambdas/:publicKey/code")]
    public async ValueTask<VersionContentResponse> GetCode(string publicKey, int? version, IRequest request)
    {
        Authorize(request);

        var privateKey = await RequireAsync(publicKey);

        var target = version ?? (await meta.GetAsync(privateKey))?.LatestVersion
                  ?? throw LambdaException.NotFound("This lambda has no versions.");

        var content = await meta.GetVersionAsync(privateKey, target);

        return new VersionContentResponse(content.Version, content.Created, content.Code);
    }

    /// <summary>
    /// Takes a lambda offline without removing it.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:publicKey/deployment")]
    public async ValueTask<LambdaOverviewResponse> Undeploy(string publicKey, IRequest request)
    {
        Authorize(request);

        var lambda = await meta.UndeployAsync(await RequireAsync(publicKey));

        return Summarize(lambda);
    }

    /// <summary>
    /// Removes a lambda and everything stored for it.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:publicKey")]
    public async ValueTask Delete(string publicKey, IRequest request)
    {
        Authorize(request);

        await meta.DeleteAsync(await RequireAsync(publicKey));
    }

    /// <summary>
    /// Refuses the request unless it carries the configured token.
    /// </summary>
    /// <remarks>
    /// Compared in constant time: a token checked with an ordinary string
    /// comparison tells anyone patient enough how much of their guess was
    /// right. Answers not found rather than unauthorized, so an installation
    /// with no panel is indistinguishable from one that simply has no such
    /// route.
    /// </remarks>
    private void Authorize(IRequest request)
    {
        if (!options.Administrable)
        {
            throw LambdaException.NotFound("This installation has no administration panel.");
        }

        var presented = request.Header.Headers.GetEntry("X-Admin-Token");

        var expected = Encoding.UTF8.GetBytes(options.AdminToken!);

        var actual = Encoding.UTF8.GetBytes(presented ?? string.Empty);

        if (actual.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(actual, expected))
        {
            throw LambdaException.NotFound("This installation has no administration panel.");
        }
    }

    private async ValueTask<string> RequireAsync(string publicKey)
        => await meta.GetPrivateKeyAsync(publicKey)
        ?? throw LambdaException.NotFound($"There is no lambda at '{publicKey}'.");

    private static LambdaOverviewResponse Summarize(LambdaInfo lambda)
        => new(lambda.PublicKey, lambda.ActiveVersion, lambda.DeployedUntil);

    #endregion

}
