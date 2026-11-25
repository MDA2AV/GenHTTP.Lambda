using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Guide;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.SharedFiles;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Infrastructure;

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
    public static async ValueTask<string> PublicKeyOfAsync(this IMetaService meta, string privateKey)
        => await meta.GetPublicKeyAsync(privateKey) ?? Unknown;

    /// <summary>
    /// The name of a feature, to name it in a log line where nothing at hand
    /// has it already.
    /// </summary>
    public static async ValueTask<string> NameOfAsync(this IFeatureService features, string privateKey, string feature)
        => await features.GetNameAsync(privateKey, feature) ?? Unknown;

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

    #region The operator's

    // what the operator did, through the panel's API or the operator's MCP
    // endpoint alike - one line per case, so a search finds either door's

    public static void TierChanged(this ILogger logger, string lambda, string tier)
        => logger.LogInformation("Set tier of lambda {Lambda} to {Tier} by operator", lambda, tier);

    public static void DomainChanged(this ILogger logger, string lambda, string? domain)
        => logger.LogInformation("Set domain of lambda {Lambda} to {Domain} by operator", lambda, domain ?? "(none)");

    public static void Undeployed(this ILogger logger, string lambda)
        => logger.LogInformation("Undeployed lambda {Lambda} by operator", lambda);

    public static void Deleted(this ILogger logger, string lambda)
        => logger.LogInformation("Deleted lambda {Lambda} by operator", lambda);

    public static void Blocked(this ILogger logger, string lambda, int addresses)
        => logger.LogInformation("Blocked lambda {Lambda} addresses {Addresses} by operator", lambda, addresses);

    public static void Unblocked(this ILogger logger, string lambda)
        => logger.LogInformation("Unblocked lambda {Lambda} by operator", lambda);

    public static void ConnectivityDecided(this ILogger logger, string lambda, bool allowed)
    {
        if (allowed)
        {
            logger.LogInformation("Allowed connectivity for lambda {Lambda} by operator", lambda);
        }
        else
        {
            logger.LogInformation("Declined connectivity for lambda {Lambda} by operator", lambda);
        }
    }

    public static void AddressBlockRemoved(this ILogger logger, long block)
        => logger.LogInformation("Removed address block #{Block} by operator", block);

    public static void SettingsChanged(this ILogger logger, SiteSettings saved)
        => logger.LogInformation("Updated settings optimizeDatabases {OptimizeDatabases} profile {Profile} by operator", saved.OptimizeDatabases,
                                 InstallationProfiles.NameOf(saved.Profile));

    /// <summary>
    /// A line per limit a save changed, none for those it left as they were.
    /// </summary>
    public static void LimitsChanged(this ILogger logger, ProductLimits previous, ProductLimits saved)
    {
        foreach (var (key, before, value) in LimitsService.Changes(previous, saved))
        {
            logger.LogInformation("Changed limit {Limit} from {Previous} to {Value} by operator", key, before ?? "(none)", value);
        }
    }

    public static void AgentSettingsChanged(this ILogger logger, AgentSettings saved)
        => logger.LogInformation("Updated agent settings builds {Builds} changes {Changes} models {Models} model {Model} efforts {Efforts} effort {Effort} by operator",
                                 saved.Builds, saved.Changes,
                                 string.Join(',', saved.Models.Select(m => $"{AgentCatalog.NameOf(m.Model)}:{AgentCatalog.NameOf(m.Access)}")),
                                 AgentCatalog.NameOf(saved.DefaultModel),
                                 string.Join(',', saved.Efforts.Select(AgentCatalog.NameOf)),
                                 AgentCatalog.NameOf(saved.DefaultEffort));

    public static void GuideSectionChanged(this ILogger logger, GuideSection section)
        => logger.LogInformation("Changed guide section '{Title}' bytes {Bytes} by operator", section.Title, section.Bytes);

    public static void GuideSectionRemoved(this ILogger logger, GuideSection removed)
        => logger.LogInformation("Removed guide section '{Title}' by operator", removed.Title);

    /// <summary>
    /// Logs a new name - and nothing where the name stayed what it was.
    /// </summary>
    public static void BrandingRenamed(this ILogger logger, string? previous, Brand brand)
    {
        if (previous != brand.Given)
        {
            logger.LogInformation("Changed branding name from '{Previous}' to '{Name}' by operator", previous ?? Brand.PlatformName, brand.Name);
        }
    }

    public static void BrandingReset(this ILogger logger)
        => logger.LogInformation("Reset branding by operator");

    public static void LogoUploaded(this ILogger logger, Brand brand)
        => logger.LogInformation("Uploaded branding logo type {Type} bytes {Bytes} by operator", brand.Logo!.Type, brand.Logo.Content.Length);

    public static void LogoRemoved(this ILogger logger)
        => logger.LogInformation("Removed branding logo by operator");

    /// <summary>
    /// Logs other colours - and nothing where they stayed what they were.
    /// </summary>
    public static void BrandingColored(this ILogger logger, BrandLook previous, Brand brand)
    {
        if (previous.Accent != brand.Look.Accent || previous.Second != brand.Look.Second)
        {
            logger.LogInformation("Changed branding colors accent {Accent} second {Second} by operator", brand.Look.Accent?.Hex ?? "none", brand.Look.Second?.Hex ?? "none");
        }
    }

    /// <summary>
    /// Logs other fonts - and nothing where they stayed what they were.
    /// </summary>
    public static void BrandingFonts(this ILogger logger, BrandLook previous, Brand brand)
    {
        var (text, headings) = (Names(brand.Look.TextFont), Names(brand.Look.HeadingFont));

        if (Names(previous.TextFont) != text || Names(previous.HeadingFont) != headings)
        {
            logger.LogInformation("Changed branding fonts text {Text} headings {Headings} by operator", text, headings);
        }

        static string Names(IReadOnlyList<SharedFile> files) => files.Count == 0 ? "none" : string.Join(',', files.Select(f => f.Name));
    }

    public static void SharedFileWritten(this ILogger logger, SharedFile file, bool replaced)
    {
        if (replaced)
        {
            logger.LogInformation("Replaced shared file {Name} bytes {Bytes} by operator", file.Name, file.Bytes);
        }
        else
        {
            logger.LogInformation("Shared file {Name} bytes {Bytes} by operator", file.Name, file.Bytes);
        }
    }

    public static void SharedFileDeleted(this ILogger logger, SharedFile file)
        => logger.LogInformation("Deleted shared file {Name} bytes {Bytes} by operator", file.Name, file.Bytes);

    #endregion

}
