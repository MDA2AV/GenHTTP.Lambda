using GenHTTP.Lambda.Community;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations.PostgreSql;
using GenHTTP.Lambda.Integrations.Sqlite;
using GenHTTP.Lambda.Server;

namespace GenHTTP.Lambda.Tests.Integrations;

/// <summary>
/// Which database an installation keeps its data in, as its environment
/// chooses it and its host offers it: SQLite everywhere, PostgreSQL on the
/// community edition.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class DatabaseSettingsTests
{
    private static readonly string[] Variables = ["LAMBDA_DATABASE", "LAMBDA_POSTGRES_CONNECTION", "LAMBDA_POSTGRES_ROLE_PREFIX", "LAMBDA_POSTGRES_POOL_SIZE"];

    private readonly Dictionary<string, string?> _before = [];

    [TestInitialize]
    public void Remember()
    {
        foreach (var variable in Variables)
        {
            _before[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [TestCleanup]
    public void Restore()
    {
        foreach (var (variable, value) in _before)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }

    [TestMethod]
    public void WithoutBeingToldAnInstallationKeepsItsDataInSqlite()
    {
        var settings = LambdaSettings.FromEnvironment(LambdaAddressing.Paths, databases: CommunityDatabase.Offered);

        Assert.AreEqual(DatabaseSettings.Sqlite, settings.Database.Provider);
        Assert.IsInstanceOfType<SqliteDatabaseBuilder>(CommunityDatabase.Of(settings.Database));
    }

    [TestMethod]
    public void TheCommunityEditionKeepsItsDataInPostgreSqlWhenToldSo()
    {
        Environment.SetEnvironmentVariable("LAMBDA_DATABASE", "PostgreSQL");
        Environment.SetEnvironmentVariable("LAMBDA_POSTGRES_CONNECTION", "Host=db;Database=lambda;Username=lambda;Password=secret");
        Environment.SetEnvironmentVariable("LAMBDA_POSTGRES_ROLE_PREFIX", "apps");
        Environment.SetEnvironmentVariable("LAMBDA_POSTGRES_POOL_SIZE", "4");

        var settings = LambdaSettings.FromEnvironment(LambdaAddressing.Paths, databases: CommunityDatabase.Offered);

        Assert.AreEqual(DatabaseSettings.PostgreSql, settings.Database.Provider, "whatever case it was written in");
        Assert.AreEqual("apps", settings.Database.RolePrefix);
        Assert.AreEqual(4, settings.Database.PoolSize);

        Assert.IsInstanceOfType<PostgreSqlDatabaseBuilder>(CommunityDatabase.Of(settings.Database));
    }

    [TestMethod]
    public void PostgreSqlWithoutAConnectionIsRefusedAsTheInstallationStarts()
    {
        Environment.SetEnvironmentVariable("LAMBDA_DATABASE", "postgresql");

        var refused = Assert.ThrowsExactly<InvalidOperationException>(() => LambdaSettings.FromEnvironment(LambdaAddressing.Paths, databases: CommunityDatabase.Offered));

        Assert.Contains("LAMBDA_POSTGRES_CONNECTION", refused.Message);
    }

    [TestMethod]
    public void GenhttpDevOffersSqliteAlone()
    {
        Environment.SetEnvironmentVariable("LAMBDA_DATABASE", "postgresql");
        Environment.SetEnvironmentVariable("LAMBDA_POSTGRES_CONNECTION", "Host=db;Database=lambda;Username=lambda;Password=secret");

        // as the cloud's host reads it: the databases it offers left out
        var refused = Assert.ThrowsExactly<InvalidOperationException>(() => LambdaSettings.FromEnvironment());

        Assert.Contains("It offers: sqlite", refused.Message, "refused rather than quietly left for SQLite");
    }

    [TestMethod]
    public void ADatabaseNoHostOffersIsRefused()
    {
        Environment.SetEnvironmentVariable("LAMBDA_DATABASE", "mysql");

        Assert.ThrowsExactly<InvalidOperationException>(() => LambdaSettings.FromEnvironment(LambdaAddressing.Paths, databases: CommunityDatabase.Offered));
    }

    [TestMethod]
    public void ARolePrefixMustBeANameOnItsOwn()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PostgreSqlDatabase.Create("Host=db;Database=lambda").RolePrefix("Robert'); DROP ROLE x; --"));
    }

}
