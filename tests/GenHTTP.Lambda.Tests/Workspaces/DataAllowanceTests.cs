using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// The room of a lambda's data, which its database and its workspace share:
/// what one takes, the other cannot - asked of it by the owner, an agent or
/// the lambda itself.
/// </summary>
[TestClass]
public sealed class DataAllowanceTests
{
    private const long Room = 1024 * 1024;

    /// <summary>
    /// Grows its database by four hundred kilobytes, and writes as much into
    /// its workspace as it is asked to.
    /// </summary>
    private const string Grower = """
        return Inline.Create()
                     .Get("grow", () =>
                     {
                         using var db = Database.GetConnection();
                         using var command = db.CreateCommand();

                         command.CommandText = "CREATE TABLE IF NOT EXISTS blobs (content BLOB); INSERT INTO blobs SELECT randomblob(100000) FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4)";

                         try
                         {
                             command.ExecuteNonQuery();
                             return "grew";
                         }
                         catch (SqliteException e)
                         {
                             return e.Message;
                         }
                     })
                     .Get("write", (int size) =>
                     {
                         try
                         {
                             Workspace.WriteBytes("written.bin", new byte[size]);
                             return "written";
                         }
                         catch (InvalidOperationException e)
                         {
                             return e.Message;
                         }
                     });
        """;

    [TestMethod]
    public async Task TheDatabaseGrowsOnlyIntoWhatTheWorkspaceLeaves()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        var lambda = await PrepareAsync(fixture, "crowded");

        await PutAsync(fixture, lambda.PrivateKey, "upload.bin", 900 * 1024, HttpStatusCode.OK);

        StringAssert.Contains(await ServedAsync(fixture, "http://crowded.localhost/grow"), "full", "what the workspace takes is not there for the database");

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/files/upload.bin"))
        {
            Assert.IsTrue(deleted.IsSuccessStatusCode);
        }

        Assert.AreEqual("grew", await ServedAsync(fixture, "http://crowded.localhost/grow"), "and is once the workspace lets go of it");

        await PutAsync(fixture, lambda.PrivateKey, "upload.bin", 900 * 1024, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task TheLambdaCountsItsDatabaseWhereItWritesItsWorkspace()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        await PrepareAsync(fixture, "keeper");

        Assert.AreEqual("grew", await ServedAsync(fixture, "http://keeper.localhost/grow"));

        StringAssert.Contains(await ServedAsync(fixture, $"http://keeper.localhost/write?size={800 * 1024}"), "its workspace and its database together",
                              "the lambda is told what its data may come to");

        Assert.AreEqual("written", await ServedAsync(fixture, $"http://keeper.localhost/write?size={300 * 1024}"), "and what fits beside the database is written");

        StringAssert.Contains(await ServedAsync(fixture, "http://keeper.localhost/grow"), "full",
                              "which the next connection to the database knows of, without a deploy");
    }

    [TestMethod]
    public async Task TheOverviewSaysHowFarEachKindMayGrow()
    {
        await using var fixture = await LambdaFixture.CreateAsync(Small);

        var lambda = await PrepareAsync(fixture, "measured");

        await PutAsync(fixture, lambda.PrivateKey, "upload.bin", 100 * 1024, HttpStatusCode.OK);

        Assert.AreEqual("grew", await ServedAsync(fixture, "http://measured.localhost/grow"));

        using var listed = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/data");

        var stores = await listed.GetContentAsync<List<DataStoreResponse>>();

        var workspace = stores.Single(s => s.Kind == "workspace");
        var database = stores.Single(s => s.Kind == "database");

        Assert.AreEqual(Room - database.UsedBytes, workspace.QuotaBytes, "the workspace may take what the database leaves");
        Assert.AreEqual(Room - workspace.UsedBytes, database.QuotaBytes, "and the database what the workspace leaves");

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        Assert.AreEqual(Room, (await summary.GetContentAsync<LambdaSummaryResponse>()).Limits.DataBytes, "the room they share");
    }

    #region Helpers

    private static Configuration.LambdaOptions Small(Configuration.LambdaOptions options) => options with { DataBytes = Room, PremiumDataBytes = Room };

    private static async Task<LambdaResponse> PrepareAsync(LambdaFixture fixture, string publicKey)
    {
        var lambda = await fixture.CreateLambdaAsync(publicKey);

        using (var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/database"))
        {
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode);
        }

        await fixture.DeployAsync(lambda.PrivateKey, Grower);

        return lambda;
    }

    private static async Task PutAsync(LambdaFixture fixture, string privateKey, string path, int size, HttpStatusCode expected)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/files/{path}", new FileRequest(Convert.ToBase64String(new byte[size])));

        Assert.AreEqual(expected, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string path)
    {
        using var served = await fixture.GetAsync(path);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    #endregion

}
