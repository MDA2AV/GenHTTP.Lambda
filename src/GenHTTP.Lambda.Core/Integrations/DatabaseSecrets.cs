using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.Sealed;

/// <summary>
/// Keeps the secrets sealed in the platform's own database - what every
/// installation does unless it chooses another place for them.
/// </summary>
/// <remarks>
/// A secrets integration built on the database integration: the values are
/// rows of the platform's records (<c>secrets</c>), which are wherever the
/// database integration keeps them - in PostgreSQL on an installation that
/// keeps its databases there. So it is in Core, beside the platform's
/// records, rather than a project of its own.
///
/// A value is sealed by <see cref="SecretCipher"/> under a key made of the
/// installation's key, which is never in the database, and a salt of the
/// lambda's, which is in its row. A feature's copy is copied sealed: it is the
/// lambda's, under the lambda's key, so nothing is opened to make it. What a
/// lambda reads is held sealed until it reads it.
///
/// Asks for the rows as the platform asks for all of its records: awaited.
/// Where the database integration answers at once - SQLite - every task this
/// returns has completed already and the thread that asks is the one that
/// reads the row; on PostgreSQL it waits for the server without holding a
/// thread.
/// </remarks>
public sealed class DatabaseSecrets(IDbContextFactory<LambdaDbContext> databases, SecretCipher cipher, ILogger<DatabaseSecrets> logger) : ISecretProvider
{
    private const string FingerprintSetting = "secrets-key";

    private int _fingerprinted;

    #region Functionality

    public async ValueTask<ISecretSet> LoadAsync(DataScope scope, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var values = await database.Secrets.AsNoTracking()
                                   .Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId)
                                   .ToDictionaryAsync(s => s.Name, s => s.Value, StringComparer.Ordinal, cancellation);

        if (values.Count == 0)
        {
            return new SealedSet(cipher, null, values);
        }

        await CheckFingerprintAsync(database);

        var salt = await database.Lambdas.AsNoTracking()
                                 .Where(l => l.Id == scope.LambdaId)
                                 .Select(l => l.SecretSalt)
                                 .FirstOrDefaultAsync(cancellation);

        return new SealedSet(cipher, salt != null ? cipher.KeyOf(salt) : null, values);
    }

    public async ValueTask<IReadOnlyList<SecretEntry>> ListAsync(DataScope scope, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Secrets.AsNoTracking()
                             .Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId)
                             .OrderBy(s => s.Name)
                             .Select(s => new SecretEntry(s.Name, s.Created, s.Changed))
                             .ToListAsync(cancellation);
    }

    public async ValueTask<SecretEntry> StoreAsync(DataScope scope, string name, string value, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var lambda = await database.Lambdas.FirstOrDefaultAsync(l => l.Id == scope.LambdaId, cancellation)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        // the lambda's half of its key, made the first time it needs one
        lambda.SecretSalt ??= SecretCipher.NewSalt();

        await CheckFingerprintAsync(database);

        var sealedValue = cipher.Seal(cipher.KeyOf(lambda.SecretSalt), name, value);

        var existing = await database.Secrets.FirstOrDefaultAsync(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId && s.Name == name, cancellation);

        var now = DateTime.UtcNow;

        if (existing == null)
        {
            existing = new SecretEntity { LambdaId = scope.LambdaId, FeatureId = scope.FeatureId, Name = name, Value = sealedValue, Created = now, Changed = now };

            database.Secrets.Add(existing);
        }
        else
        {
            existing.Value = sealedValue;
            existing.Changed = now;
        }

        await database.SaveChangesAsync(cancellation);

        return new SecretEntry(existing.Name, existing.Created, existing.Changed);
    }

    public async ValueTask<bool> DeleteAsync(DataScope scope, string name, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.Secrets.Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId && s.Name == name)
                             .ExecuteDeleteAsync(cancellation) > 0;
    }

    /// <remarks>
    /// In one transaction, so a copy stopped half way leaves the one before
    /// rather than none.
    /// </remarks>
    public async ValueTask CopyAsync(DataScope source, DataScope target, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await using var transaction = await database.Database.BeginTransactionAsync(cancellation);

        await database.Secrets.Where(s => s.LambdaId == target.LambdaId && s.FeatureId == target.FeatureId).ExecuteDeleteAsync(cancellation);

        var own = await database.Secrets.AsNoTracking()
                                .Where(s => s.LambdaId == source.LambdaId && s.FeatureId == source.FeatureId)
                                .ToListAsync(cancellation);

        database.Secrets.AddRange(own.Select(s => new SecretEntity
        {
            LambdaId = target.LambdaId,
            FeatureId = target.FeatureId,
            Name = s.Name,
            Value = s.Value,
            Created = s.Created,
            Changed = s.Changed
        }));

        await database.SaveChangesAsync(cancellation);

        await transaction.CommitAsync(cancellation);
    }

    public async ValueTask DeleteAsync(DataScope scope, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.Secrets.Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId).ExecuteDeleteAsync(cancellation);
    }

    /// <remarks>
    /// The lambda's half of its key goes with them, so secrets stored later
    /// are sealed with a new one.
    /// </remarks>
    public async ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.Secrets.Where(s => s.LambdaId == lambdaId).ExecuteDeleteAsync(cancellation);

        // the secrets are gone, so the half of the key that opened them goes as well
        await database.Lambdas.Where(l => l.Id == lambdaId)
                              .ExecuteUpdateAsync(u => u.SetProperty(l => l.SecretSalt, (byte[]?)null), CancellationToken.None);
    }

    public IntegrationInfo Describe() => new(IntegrationKind.Secrets, "Database",
                                             [
                                                 new IntegrationDetail("Sealed", "AES-GCM, a key per lambda"),
                                                 new IntegrationDetail("Installation key", cipher.Source)
                                             ]);

    public string? Guide(GuideContext context) => null;

    #endregion

    #region Key

    /// <summary>
    /// Remembers which installation key sealed what the database holds, and
    /// says so loudly when the server runs with another one.
    /// </summary>
    /// <remarks>
    /// Without this, a server moved without its key would find out one secret
    /// at a time, in the logs of the lambdas that read them.
    /// </remarks>
    private async ValueTask CheckFingerprintAsync(LambdaDbContext database)
    {
        if (Interlocked.Exchange(ref _fingerprinted, 1) == 1)
        {
            return;
        }

        try
        {
            var mine = cipher.Fingerprint();

            var stored = await database.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == FingerprintSetting);

            if (stored == null)
            {
                await using var writer = await databases.CreateDbContextAsync();

                writer.Settings.Add(new SettingEntity { Key = FingerprintSetting, Value = mine });
                await writer.SaveChangesAsync();
            }
            else if (stored.Value != mine)
            {
                logger.LogError("The secrets in this database were sealed with another key than the one this server has (LAMBDA_SECRETS_KEY or secrets.key), "
                              + "so none of them can be read. Start the server with the key the database was written with.");
            }
        }
        catch (Exception e)
        {
            // the check is a courtesy; reading the secrets says the same thing later
            logger.LogWarning(e, "Failed to check which key the secrets were sealed with");
        }
    }

    #endregion

    /// <summary>
    /// The secrets of a lambda or feature, sealed until one is read.
    /// </summary>
    /// <param name="Key">The key of the lambda, while it has anything to open with it</param>
    private sealed class SealedSet(SecretCipher cipher, byte[]? key, IReadOnlyDictionary<string, byte[]> values) : ISecretSet
    {

        public int Count => values.Count;

        public string? Read(string name)
            => values.TryGetValue(name, out var sealedValue) && key != null ? cipher.Open(key, name, sealedValue) : null;

    }

}
