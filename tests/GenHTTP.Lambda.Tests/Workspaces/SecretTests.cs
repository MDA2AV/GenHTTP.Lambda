using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// The secrets of a lambda: API keys and passwords its code reads by name,
/// which are set by its owner or an agent and read back by nobody.
/// </summary>
[TestClass]
public sealed class SecretTests
{

    private const string Value = "not-a-real-key-0123456789";

    private const string Reader = """
        return Inline.Create()
                     .Get("read", () => Secret.Read("GREETING"))
                     .Get("exists", () => Secret.Exists("GREETING") ? "yes" : "no");
        """;

    [TestMethod]
    public async Task SecretsAreOffUntilSomebodySwitchesThemOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using (var data = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets"))
        {
            var store = await data.GetContentAsync<DataStoreResponse>();

            Assert.IsFalse(store.Enabled);
            Assert.IsFalse(store.Default);
            Assert.AreEqual(100, store.MaxItems);
        }

        using (var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/GREETING", new SecretRequest(Value)))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Contains("switched off", await refused.Content.ReadAsStringAsync());
        }

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        Assert.AreEqual("no", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/exists"), "Exists is false while they are off");

        using (var failed = await fixture.GetAsync($"http://{lambda.PublicKey}.localhost/read"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, failed.StatusCode);
        }

        await AssertProblemAsync(fixture, lambda.PrivateKey, "switched off");
    }

    [TestMethod]
    public async Task ALambdaReadsWhatItsOwnerSetsWithoutBeingDeployedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        using (var missing = await fixture.GetAsync($"http://{lambda.PublicKey}.localhost/read"))
        {
            Assert.AreEqual(HttpStatusCode.InternalServerError, missing.StatusCode);
        }

        await AssertProblemAsync(fixture, lambda.PrivateKey, "no secret called 'GREETING'");

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", "hello");

        Assert.AreEqual("hello", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/read"));
        Assert.AreEqual("yes", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/exists"));

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", "hello again");

        Assert.AreEqual("hello again", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/read"), "replaced, and read at once");
    }

    [TestMethod]
    public async Task SecretsAreReadFromEveryFileAndWhileTheLambdaIsBuilt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", "hello");

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions?deploy=true", new VersionRequest(
        [
            new LambdaFile(LambdaSource.EntryName, "var greeting = Secret.Read(\"GREETING\");\nreturn Inline.Create().Get(() => greeting + \" \" + Greeter.Name());"),
            new LambdaFile("Greeter.cs", "public static class Greeter { public static string Name() => Secret.Exists(\"NAME\") ? Secret.Read(\"NAME\") : \"nobody\"; }")
        ]));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsTrue(version.Deployment!.Success, string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));

        Assert.AreEqual("hello nobody", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/"));
    }

    [TestMethod]
    public async Task NobodyReadsAValueBack()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        using (var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/GREETING", new SecretRequest(Value)))
        {
            var text = await put.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, text);
            Assert.DoesNotContain(Value, text, "not even in the answer to storing it");
        }

        var answers = new List<string>();

        foreach (var path in new[] { "secrets", "data", "data/secrets", "", "summary", "logs" })
        {
            using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/{path}");

            answers.Add(await response.Content.ReadAsStringAsync());
        }

        answers.Add((await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey })).ToJsonString());
        answers.Add((await CallToolAsync(fixture, "list_secrets", new JsonObject { ["privateKey"] = lambda.PrivateKey })).ToJsonString());

        foreach (var answer in answers)
        {
            Assert.DoesNotContain(Value, answer);
        }

        Assert.IsTrue(answers[0].Contains("GREETING"), "the name is there to be seen");
        Assert.IsFalse(Logged(fixture).Any(l => l.Contains(Value)), "and the server logged the name at most");
    }

    [TestMethod]
    public async Task ValuesAreNotStoredAsTheyWereGiven()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", Value);

        var databases = fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using (var database = await databases.CreateDbContextAsync())
        {
            var row = await database.Secrets.SingleAsync();

            Assert.AreEqual("GREETING", row.Name);
            Assert.AreEqual(-1, IndexOf(row.Value, Encoding.UTF8.GetBytes(Value)));
        }

        // nor anywhere in the files of the database, the log of its writes included
        foreach (var file in Directory.GetFiles(fixture.Options.DataDirectory, "lambda.db*"))
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            using var copy = new MemoryStream();

            await stream.CopyToAsync(copy);

            Assert.AreEqual(-1, IndexOf(copy.ToArray(), Encoding.UTF8.GetBytes(Value)), file);
        }

        Assert.IsTrue(File.Exists(fixture.Options.SecretsKeyFile), "the key they are sealed with is kept apart from the database");
    }

    [TestMethod]
    public async Task TheDatabaseAndItsKeyAreAllItTakesToMoveTheSecrets()
    {
        var directory = Scratch();

        const string key = "a passphrase that is long enough to be a key, for the tests";

        string publicKey;

        await using (var first = await LambdaFixture.CreateAsync(o => o with { DataDirectory = directory, SecretsKey = key }))
        {
            var lambda = await first.CreateLambdaAsync();

            publicKey = lambda.PublicKey;

            await EnableAsync(first, lambda.PrivateKey);
            await SetAsync(first, lambda.PrivateKey, "GREETING", "moved along");

            await first.DeployAsync(lambda.PrivateKey, Reader);
        }

        SqliteConnection.ClearAllPools();

        // what a move to another server is: its files - the database and the
        // code, nothing that was built from them - and the key given to it
        var moved = Move(directory);

        await using (var second = await LambdaFixture.CreateAsync(o => o with { DataDirectory = moved, SecretsKey = key }))
        {
            Assert.AreEqual("moved along", await ServedAsync(second, $"http://{publicKey}.localhost/read"));
        }

        SqliteConnection.ClearAllPools();

        // and without the key, nothing opens
        var stolen = Move(directory);

        await using (var third = await LambdaFixture.CreateAsync(o => o with { DataDirectory = stolen, SecretsKey = "another passphrase, which opens none of what was sealed" }))
        {
            using var refused = await third.GetAsync($"http://{publicKey}.localhost/read");

            Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode);
            Assert.IsTrue(Logged(third).Any(l => l.Contains("another key")), "and the server says why");
        }
    }

    [TestMethod]
    public async Task TheListingSaysWhichValuesTheCodeIsWaitingFor()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await fixture.DeployAsync(lambda.PrivateKey, """
            return Inline.Create()
                         .Get("pay", () => Secret.Read("STRIPE_KEY"))
                         .Get("mail", () => Secret.Exists("SMTP_PASSWORD") ? "mail" : "no mail");
            """);

        await SetAsync(fixture, lambda.PrivateKey, "UNUSED", "left over");

        var listing = await ListAsync(fixture, lambda.PrivateKey);

        CollectionAssert.AreEqual(new[] { "SMTP_PASSWORD", "STRIPE_KEY" }, listing.Used);
        CollectionAssert.AreEqual(new[] { "STRIPE_KEY" }, listing.Missing, "read without asking first, so reading fails");
        CollectionAssert.AreEqual(new[] { "SMTP_PASSWORD" }, listing.Optional, "asked about first, so it does without");
        Assert.IsFalse(listing.Secrets.Single().Used, "and nothing reads what is left over");

        await SetAsync(fixture, lambda.PrivateKey, "STRIPE_KEY", Value);

        listing = await ListAsync(fixture, lambda.PrivateKey);

        Assert.IsEmpty(listing.Missing);
        Assert.IsTrue(listing.Secrets.Single(s => s.Name == "STRIPE_KEY").Used);
    }

    [TestMethod]
    public async Task AFeatureWorksOnACopyOfTheSecrets()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", "live");

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        FeatureResponse feature;

        using (var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Sandbox")))
        {
            feature = await created.GetContentAsync<FeatureResponse>();
        }

        using (var copy = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/secrets"))
        {
            Assert.AreEqual("GREETING", (await copy.GetContentAsync<SecretListingResponse>()).Secrets.Single().Name, "it starts with a copy");
        }

        using (var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/secrets/GREETING",
                                                  new SecretRequest("sandbox")))
        {
            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
        }

        using (var started = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/preview/start"))
        {
            Assert.AreEqual(HttpStatusCode.OK, started.StatusCode, await started.Content.ReadAsStringAsync());
        }

        Assert.AreEqual("sandbox", await ServedAsync(fixture, $"{feature.PreviewPath}read"));
        Assert.AreEqual("live", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/read"), "the lambda's own is as it was");

        // a fresh copy of the lambda's
        using (var _ = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/data/refresh")) { }

        Assert.AreEqual("live", await ServedAsync(fixture, $"{feature.PreviewPath}read"));

        using (var removed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}"))
        {
            Assert.IsTrue(removed.IsSuccessStatusCode);
        }

        var databases = fixture.Application.Services.GetRequiredService<IDbContextFactory<LambdaDbContext>>();

        await using var database = await databases.CreateDbContextAsync();

        Assert.AreEqual(1, await database.Secrets.CountAsync(), "the copy went with the feature");
    }

    [TestMethod]
    public async Task SwitchingSecretsOffDeletesThemAndTheCopies()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", "hello");

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        FeatureResponse feature;

        using (var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest("Copy")))
        {
            feature = await created.GetContentAsync<FeatureResponse>();
        }

        Assert.AreEqual("hello", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/read"));

        using (var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets"))
        {
            var store = await off.GetContentAsync<DataStoreResponse>();

            Assert.IsFalse(store.Enabled);
            Assert.AreEqual(0, store.Items);
        }

        Assert.AreEqual("no", await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/exists"), "gone from the lambda at once");

        using (var copy = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature.Key}/secrets"))
        {
            Assert.IsEmpty((await copy.GetContentAsync<SecretListingResponse>()).Secrets, "and from the copies");
        }

        await EnableAsync(fixture, lambda.PrivateKey);

        Assert.IsEmpty((await ListAsync(fixture, lambda.PrivateKey)).Secrets, "switched on again, they start empty");
    }

    [TestMethod]
    public async Task NamesAreThoseOfEnvironmentVariables()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        foreach (var name in new[] { "1ST", "WITH-DASH", "with%20space" })
        {
            using var refused = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/{name}", new SecretRequest(Value));

            Assert.AreEqual(HttpStatusCode.BadRequest, refused.StatusCode, name);
        }

        using (var empty = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/EMPTY", new SecretRequest("")))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode, "a secret needs a value");
        }

        using (var nothing = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/NOTHING"))
        {
            Assert.AreEqual(HttpStatusCode.NotFound, nothing.StatusCode);
        }
    }

    [TestMethod]
    public async Task AnAgentSwitchesSecretsOnAndStoresOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, "return Inline.Create().Get(\"read\", () => Secret.Read(\"GREETING\"));");

        var refused = await CallToolAsync(fixture, "set_secret", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "GREETING", ["value"] = Value });

        Assert.Contains("enable_data", refused.ToJsonString(), "told how to switch them on");

        var listed = Structured(await CallToolAsync(fixture, "list_secrets", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.AreEqual("GREETING", listed["missing"]![0]!.GetValue<string>(), "what the code waits for is known before there is anything");

        var enabled = Structured(await CallToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["kind"] = "secrets" }));

        Assert.IsTrue(enabled["enabled"]!.GetValue<bool>());

        var stored = await CallToolAsync(fixture, "set_secret", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "GREETING", ["value"] = Value });

        Assert.IsTrue(Structured(stored)["usedByCode"]!.GetValue<bool>());
        Assert.DoesNotContain(Value, stored.ToJsonString(), "the value is in the conversation once, where the user put it");

        Assert.AreEqual(Value, await ServedAsync(fixture, $"http://{lambda.PublicKey}.localhost/read"));

        var read = Structured(await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey }));

        Assert.AreEqual("GREETING", read["data"]!["secrets"]!["names"]![0]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TheExportedProjectReadsSecretsFromTheEnvironment()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetAsync(fixture, lambda.PrivateKey, "GREETING", Value);

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        using var response = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        using var archive = new ZipArchive(await response.Content.ReadAsStreamAsync());

        var texts = new Dictionary<string, string>();

        foreach (var entry in archive.Entries)
        {
            using var reader = new StreamReader(entry.Open());

            texts[entry.Name] = await reader.ReadToEndAsync();
        }

        Assert.IsFalse(texts.Values.Any(t => t.Contains(Value)), "the values stay behind");
        Assert.Contains("GetEnvironmentVariable(name)", texts["Secrets.cs"]);
        Assert.Contains("//   GREETING", texts["Program.cs"], "and the project says which to set");
        Assert.Contains("-e GREETING", texts["Program.cs"], "for docker run as well");
    }

    [TestMethod]
    public async Task TheSecretsOfADemoAreReadAndNotChanged()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        await fixture.SeedDemosAsync();

        var listing = await ListAsync(fixture, "demo-registration");

        Assert.IsTrue(listing.Enabled);
        Assert.AreEqual("PASSWORD_PEPPER", listing.Secrets.Single().Name);
        Assert.IsTrue(listing.Secrets.Single().Used, "its code reads it");

        using var refused = await fixture.SendAsync(HttpMethod.Put, "/api/v1/lambdas/demo-registration/secrets/PASSWORD_PEPPER", new SecretRequest(Value));

        Assert.AreEqual(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    #region Helpers

    private static async Task EnableAsync(LambdaFixture fixture, string privateKey)
    {
        using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/data/secrets");

        Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
    }

    private static async Task SetAsync(LambdaFixture fixture, string privateKey, string name, string value)
    {
        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/secrets/{name}", new SecretRequest(value));

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
    }

    private static async Task<SecretListingResponse> ListAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/secrets");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<SecretListingResponse>();
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string path)
    {
        using var served = await fixture.GetAsync(path);

        var content = await served.GetContentAsync();

        Assert.AreEqual(HttpStatusCode.OK, served.StatusCode, content);

        return content;
    }

    /// <summary>
    /// That the owner reads what went wrong in the problems of the lambda.
    /// </summary>
    private static async Task AssertProblemAsync(LambdaFixture fixture, string privateKey, string text)
    {
        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/summary");

        var said = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.IsTrue(said.RecentProblems.Any(p => (p.Text + p.Detail).Contains(text)),
                      string.Join(" | ", said.RecentProblems.Select(p => p.Text + p.Detail)));
    }

    private static int IndexOf(byte[] haystack, byte[] needle) => haystack.AsSpan().IndexOf(needle);

    private static string Scratch() => Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

    /// <summary>
    /// Copies the database and the code of an installation somewhere else,
    /// and says where.
    /// </summary>
    private static string Move(string from)
    {
        var to = Scratch();

        Directory.CreateDirectory(to);

        foreach (var file in Directory.GetFiles(from, "lambda.db*"))
        {
            File.Copy(file, Path.Combine(to, Path.GetFileName(file)));
        }

        foreach (var file in Directory.GetFiles(Path.Combine(from, "code"), "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(to, Path.GetRelativePath(from, file));

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);

            File.Copy(file, target);
        }

        return to;
    }

    private static IEnumerable<string> Logged(LambdaFixture fixture)
        => fixture.Book.Read(0, null, LogLevel.Trace, 50_000).Lines.Select(l => l.Text + " " + l.Detail);

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static async Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");

        var message = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments }
        };

        request.Content = new StringContent(message.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    #endregion

}
