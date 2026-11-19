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
/// </remarks>
public sealed class DatabaseSecrets(IDbContextFactory<LambdaDbContext> databases, SecretCipher cipher, ILogger<DatabaseSecrets> logger) : ISecretProvider
{
    private const string FingerprintSetting = "secrets-key";

    private int _fingerprinted;

    #region Functionality

    public ISecretSet Load(DataScope scope)
    {
        using var database = databases.CreateDbContext();

        var values = database.Secrets.AsNoTracking()
                             .Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId)
                             .ToDictionary(s => s.Name, s => s.Value, StringComparer.Ordinal);

        if (values.Count == 0)
        {
            return new SealedSet(cipher, null, values);
        }

        CheckFingerprint(database);

        var salt = database.Lambdas.AsNoTracking()
                           .Where(l => l.Id == scope.LambdaId)
                           .Select(l => l.SecretSalt)
                           .FirstOrDefault();

        return new SealedSet(cipher, salt != null ? cipher.KeyOf(salt) : null, values);
    }

    public IReadOnlyList<SecretEntry> List(DataScope scope)
    {
        using var database = databases.CreateDbContext();

        return database.Secrets.AsNoTracking()
                       .Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId)
                       .OrderBy(s => s.Name)
                       .Select(s => new SecretEntry(s.Name, s.Created, s.Changed))
                       .ToList();
    }

    public SecretEntry Store(DataScope scope, string name, string value)
    {
        using var database = databases.CreateDbContext();

        var lambda = database.Lambdas.FirstOrDefault(l => l.Id == scope.LambdaId)
                  ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

        // the lambda's half of its key, made the first time it needs one
        lambda.SecretSalt ??= SecretCipher.NewSalt();

        CheckFingerprint(database);

        var sealedValue = cipher.Seal(cipher.KeyOf(lambda.SecretSalt), name, value);

        var existing = database.Secrets.FirstOrDefault(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId && s.Name == name);

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

        database.SaveChanges();

        return new SecretEntry(existing.Name, existing.Created, existing.Changed);
    }

    public bool Delete(DataScope scope, string name)
    {
        using var database = databases.CreateDbContext();

        return database.Secrets.Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId && s.Name == name)
                       .ExecuteDelete() > 0;
    }

    public void Copy(DataScope source, DataScope target)
    {
        using var database = databases.CreateDbContext();

        database.Secrets.Where(s => s.LambdaId == target.LambdaId && s.FeatureId == target.FeatureId).ExecuteDelete();

        var own = database.Secrets.AsNoTracking()
                          .Where(s => s.LambdaId == source.LambdaId && s.FeatureId == source.FeatureId)
                          .ToList();

        database.Secrets.AddRange(own.Select(s => new SecretEntity
        {
            LambdaId = target.LambdaId,
            FeatureId = target.FeatureId,
            Name = s.Name,
            Value = s.Value,
            Created = s.Created,
            Changed = s.Changed
        }));

        database.SaveChanges();
    }

    public void Delete(DataScope scope)
    {
        using var database = databases.CreateDbContext();

        database.Secrets.Where(s => s.LambdaId == scope.LambdaId && s.FeatureId == scope.FeatureId).ExecuteDelete();
    }

    /// <remarks>
    /// The lambda's half of its key goes with them, so secrets stored later
    /// are sealed with a new one.
    /// </remarks>
    public void DeleteAll(long lambdaId)
    {
        using var database = databases.CreateDbContext();

        database.Secrets.Where(s => s.LambdaId == lambdaId).ExecuteDelete();

        database.Lambdas.Where(l => l.Id == lambdaId)
                        .ExecuteUpdate(u => u.SetProperty(l => l.SecretSalt, (byte[]?)null));
    }

    public IntegrationInfo Describe() => new(IntegrationKinds.Secrets, "Database",
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
    private void CheckFingerprint(LambdaDbContext database)
    {
        if (Interlocked.Exchange(ref _fingerprinted, 1) == 1)
        {
            return;
        }

        try
        {
            var mine = cipher.Fingerprint();

            var stored = database.Settings.AsNoTracking().FirstOrDefault(s => s.Key == FingerprintSetting);

            if (stored == null)
            {
                using var writer = databases.CreateDbContext();

                writer.Settings.Add(new SettingEntity { Key = FingerprintSetting, Value = mine });
                writer.SaveChanges();
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
