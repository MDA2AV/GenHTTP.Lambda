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
/// Every line has one shape, the one backends log in: the verb in the past
/// tense first, then what it was done to and its key, then the arguments as
/// pairs - <c>Deployed lambda {Lambda} version {Version}</c>,
/// <c>Wrote workspace file {Path} of feature '{Feature}' of lambda {Lambda} size {Size}</c>,
/// <c>Failed to merge feature '{Feature}' of lambda {Lambda}</c>. A line says
/// what is known at that point and does not explain or guess why. The
/// properties have the same name in every line - <c>{Lambda}</c> is a public
/// key, <c>{Feature}</c> a feature's name, <c>{Version}</c>, <c>{Path}</c> -
/// so a search or a structured sink can filter on them; a service that only
/// has the id of a lambda says <c>lambda #{LambdaId}</c>. A name that may
/// hold spaces, a feature's or a title, is quoted.
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
/// So a model is never logged whole. The generated <c>ToString()</c> of a
/// record prints every property, and the models carry exactly what is left
/// out: one <c>{Lambda}</c> given a <see cref="LambdaInfo"/> would put its
/// editor key into the log. A line names the properties it logs, one by one.
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
    public static string PublicKeyOf(this IMetaService meta, string privateKey)
        => meta.GetPublicKey(privateKey) ?? Unknown;

    /// <summary>
    /// The name of a feature, to name it in a log line where nothing at hand
    /// has it already.
    /// </summary>
    public static string NameOf(this IFeatureService features, string privateKey, string feature)
        => features.GetName(privateKey, feature) ?? Unknown;

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
            logger.LogInformation("Deployed lambda {Lambda} version {Version}", lambda, result.Lambda?.ActiveVersion ?? version);
        }
        else
        {
            logger.LogInformation("Failed to deploy lambda {Lambda} version {Version}", lambda, version?.ToString() ?? "(newest)");
        }
    }

    /// <summary>
    /// Logs how putting the preview of a feature online went.
    /// </summary>
    public static void Previewed(this ILogger logger, FeatureDeployment result, string lambda)
    {
        if (result.Success)
        {
            logger.LogInformation("Deployed feature '{Feature}' of lambda {Lambda}", result.Feature.Name, lambda);
        }
        else
        {
            logger.LogInformation("Failed to deploy feature '{Feature}' of lambda {Lambda}", result.Feature.Name, lambda);
        }
    }

    #endregion

}
