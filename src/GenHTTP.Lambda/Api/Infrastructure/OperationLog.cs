using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// What the API and the MCP tools need to say what was done through them, in
/// the words of the operation rather than of the request.
/// </summary>
/// <remarks>
/// The engine already logs every request with its method, path and status,
/// which tells an operator that something was posted somewhere - not that the
/// source of a lambda was published, or which lambda that was. So every door
/// logs what it did once it has done it: the operation, the lambda by its
/// public key, and the arguments worth knowing.
///
/// What is left out is left out on purpose. The editor key is the only thing
/// standing between somebody reading the log and the ability to change the
/// lambda, and the key of a feature is the address of its preview; both are
/// named by what a person knows them by instead. The value of a secret never
/// appears, only its name. Code, file contents, pictures, specifications and
/// descriptions are too long to be worth a line and are left out as well - the
/// one exception being the prompts of the build agent, which are what tells
/// the operator what people ask this platform for.
///
/// A request that is refused is not logged here: the request line has its
/// status, and nothing happened.
/// </remarks>
public static class OperationLog
{

    #region Names

    /// <summary>
    /// The public key of the lambda of the editor key, to name it in a log line.
    /// </summary>
    public static async ValueTask<string> PublicKeyOfAsync(this IMetaService meta, string privateKey)
        => await meta.GetPublicKeyAsync(privateKey) ?? Unknown;

    /// <summary>
    /// The name of a feature, to name it in a log line.
    /// </summary>
    public static async ValueTask<string> NameOfAsync(this IFeatureService features, string privateKey, string feature)
    {
        var key = feature.Trim().ToLowerInvariant();

        return (await features.ListAsync(privateKey)).FirstOrDefault(f => f.Key == key)?.Name ?? Unknown;
    }

    /// <summary>
    /// What a lambda or a feature is called in a log line when it cannot be
    /// found any more.
    /// </summary>
    private const string Unknown = "(unknown)";

    #endregion

    #region Outcomes

    /// <summary>
    /// Logs how putting a version online went.
    /// </summary>
    /// <param name="version">The version that was asked for, or null for the newest</param>
    public static void Deployed(this ILogger logger, DeploymentResult result, string lambda, int? version)
    {
        if (result.Success)
        {
            logger.LogInformation("Put version {Version} of lambda {Lambda} online", result.Lambda?.ActiveVersion ?? version, lambda);
        }
        else
        {
            logger.LogInformation("Version {Version} of lambda {Lambda} did not go online: its code does not compile or its handler could not be built",
                                  version?.ToString() ?? "(newest)", lambda);
        }
    }

    /// <summary>
    /// Logs how putting the preview of a feature online went.
    /// </summary>
    public static void Previewed(this ILogger logger, FeatureDeployment result, string lambda)
    {
        if (result.Success)
        {
            logger.LogInformation("Put the preview of feature '{Feature}' of lambda {Lambda} online", result.Feature.Name, lambda);
        }
        else
        {
            logger.LogInformation("The preview of feature '{Feature}' of lambda {Lambda} did not go online: its code does not compile or its handler could not be built",
                                  result.Feature.Name, lambda);
        }
    }

    #endregion

}
