using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Turns a key from the path into the lambda it names, or ends the request.
/// </summary>
/// <remarks>
/// For the resources under <c>/lambdas/{privateKey}</c> this is also the
/// authorization: a key that resolves is a caller who may edit the lambda.
/// Every one of them answers a key that resolves to nothing the same way, so a
/// guessed key and a deleted lambda cannot be told apart.
/// </remarks>
public static class LambdaAccess
{

    private const string Missing = "This lambda does not exist (or has been deleted).";

    /// <summary>
    /// The lambda the editor key belongs to.
    /// </summary>
    public static async ValueTask<LambdaInfo> RequireAsync(this IMetaService meta, string privateKey, CancellationToken cancellation = default)
        => await meta.GetAsync(privateKey, cancellation) ?? throw LambdaException.NotFound(Missing);

    /// <summary>
    /// The identity the lambda of the editor key is filed under, for the
    /// services that keep things per lambda.
    /// </summary>
    public static async ValueTask<long> RequireIdAsync(this IMetaService meta, string privateKey, CancellationToken cancellation = default)
        => await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound(Missing);

    /// <summary>
    /// The editor key of the lambda at a public key, for the operator.
    /// </summary>
    public static async ValueTask<string> RequirePrivateKeyAsync(this IMetaService meta, string publicKey, CancellationToken cancellation = default)
        => await meta.GetPrivateKeyAsync(publicKey, cancellation) ?? throw LambdaException.NotFound($"There is no lambda at '{publicKey}'.");

    /// <summary>
    /// The files of the newest version of a lambda, or none before its first.
    /// </summary>
    /// <remarks>
    /// What a change is applied to, through either door: the API's and the
    /// MCP tools' edits both start from here.
    /// </remarks>
    public static async ValueTask<IReadOnlyList<LambdaFile>> LatestFilesAsync(this IMetaService meta, string privateKey, CancellationToken cancellation = default)
    {
        var lambda = await meta.RequireAsync(privateKey, cancellation);

        if (lambda.LatestVersion is not { } latest)
        {
            return [];
        }

        return LambdaSource.Parse((await meta.GetVersionAsync(privateKey, latest, cancellation)).Code);
    }

}
