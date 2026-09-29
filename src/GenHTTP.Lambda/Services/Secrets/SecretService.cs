using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Secrets;

/// <inheritdoc cref="ISecretService" />
public sealed class SecretService(IDbContextFactory<LambdaDbContext> databases, IMetaService meta, IFeatureService features, ISecretVault vault,
                                  LambdaOptions options, ILogger<SecretService> logger) : ISecretService
{

    public async ValueTask<SecretListing> ListAsync(string privateKey, string? feature = null, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, false, cancellation);

        var enabled = await EnabledAsync(lambdaId, cancellation);

        return new SecretListing(enabled, options.MaxSecrets, enabled ? await vault.ListAsync(lambdaId, featureId, cancellation) : []);
    }

    public async ValueTask<SecretInfo> SetAsync(string privateKey, string? feature, string name, string value, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, true, cancellation);

        if (!await EnabledAsync(lambdaId, cancellation))
        {
            throw LambdaException.Conflict(DataKinds.SecretsOff);
        }

        var set = await vault.SetAsync(lambdaId, featureId, name, value, cancellation);

        // the name, never the value: nothing that is written down here may hold one
        logger.LogInformation("Secret {Name} of lambda {LambdaId}{Feature} was set", set.Name, lambdaId, featureId != null ? $", feature {featureId}," : "");

        return set;
    }

    public async ValueTask DeleteAsync(string privateKey, string? feature, string name, CancellationToken cancellation = default)
    {
        var (lambdaId, featureId) = await ResolveAsync(privateKey, feature, true, cancellation);

        if (!await vault.DeleteAsync(lambdaId, featureId, name, cancellation))
        {
            throw LambdaException.NotFound($"This lambda has no secret called '{name.Trim()}'.");
        }

        logger.LogInformation("Secret {Name} of lambda {LambdaId}{Feature} was deleted", name.Trim(), lambdaId, featureId != null ? $", feature {featureId}," : "");
    }

    #region Helpers

    /// <summary>
    /// The lambda a key opens, and the feature named with it - refusing a demo
    /// where the call would change something.
    /// </summary>
    private async ValueTask<(long LambdaId, long? FeatureId)> ResolveAsync(string privateKey, string? feature, bool editable, CancellationToken cancellation)
    {
        if (feature != null)
        {
            var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, editable, cancellation);

            return (lambdaId, featureId);
        }

        var id = editable
            ? await meta.RequireEditableAsync(privateKey, cancellation)
            : await meta.GetIdAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        return (id, null);
    }

    private async ValueTask<bool> EnabledAsync(long lambdaId, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await DataSwitches.IsEnabledAsync(database, lambdaId, DataKinds.Secrets, cancellation);
    }

    #endregion

}
