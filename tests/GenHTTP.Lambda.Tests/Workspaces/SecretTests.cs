using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Workspaces;

/// <summary>
/// Secrets: the tokens and credentials a lambda reads by name and nobody reads
/// back. Off until switched on, encrypted, shared by every version and copied
/// to the features.
/// </summary>
[TestClass]
public sealed class SecretTests
{

    private const string Reader = """
        return Inline.Create()
                     .Get("token", () => Secret.Read("API_TOKEN"))
                     .Get("maybe", () => Secret.TryRead("API_TOKEN") ?? "nothing")
                     .Get("there", () => Secret.Exists("API_TOKEN").ToString())
                     .Get("on", () => Secret.Enabled.ToString());
        """;

    private const string Value = "example-token-9f2c1a7e5b4d8c3a";

    #region Switching them on

    [TestMethod]
    public async Task SecretsAreOffUntilSomebodySwitchesThemOn()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var secrets = (await ListAsync(fixture, lambda.PrivateKey)).Single(s => s.Kind == "secrets");

        Assert.IsFalse(secrets.Enabled);
        Assert.IsFalse(secrets.Default);
        Assert.AreEqual(0, secrets.Items);
        Assert.AreEqual(fixture.Options.MaxSecrets, secrets.Limit);

        using var refused = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        Assert.AreEqual(HttpStatusCode.Conflict, refused.StatusCode, "there is nothing to put it in while they are off");
        Assert.Contains("switched off", await refused.Content.ReadAsStringAsync());

        var listing = await ReadListingAsync(fixture, lambda.PrivateKey);

        Assert.IsFalse(listing.Enabled);
        Assert.IsEmpty(listing.Secrets);
    }

    #endregion

    #region What the code sees

    [TestMethod]
    public async Task ALambdaReadsASecretByNameAndSeesAChangeWithoutBeingDeployedAgain()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        Assert.AreEqual(Value, await ServedAsync(fixture, lambda.PublicKey, "token"));
        Assert.AreEqual("True", await ServedAsync(fixture, lambda.PublicKey, "there"));

        // what the simple view decides by whether to show the section
        using (var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary"))
        {
            var said = await summary.GetContentAsync<LambdaSummaryResponse>();

            Assert.IsTrue(said.Storage.SecretsEnabled);
            Assert.AreEqual(1, said.Storage.SecretCount);
            Assert.IsTrue(said.Storage.UsesSecrets);
        }

        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", "rotated");

        Assert.AreEqual("rotated", await ServedAsync(fixture, lambda.PublicKey, "token"), "the next request reads what is there now");

        using var removed = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/API_TOKEN");

        Assert.AreEqual(HttpStatusCode.NoContent, removed.StatusCode);

        Assert.AreEqual("nothing", await ServedAsync(fixture, lambda.PublicKey, "maybe"));

        using var missing = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/token");

        Assert.AreEqual(HttpStatusCode.InternalServerError, missing.StatusCode, "reading one that is not there fails");
    }

    [TestMethod]
    public async Task EveryFileOfALambdaCanReadSecrets()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest([
            new LambdaFile(LambdaSource.EntryName, "return Inline.Create().Get(() => Vault.Token());"),
            new LambdaFile("Vault.cs", "static class Vault { public static string Token() => Secret.Read(\"API_TOKEN\"); }")
        ]));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        await fixture.DeployAsync(lambda.PrivateKey);

        Assert.AreEqual(Value, await ServedAsync(fixture, lambda.PublicKey, ""));
    }

    [TestMethod]
    public async Task ALambdaWhoseSecretsAreOffIsToldSoAndCanStillAskForOne()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        Assert.AreEqual("False", await ServedAsync(fixture, lambda.PublicKey, "on"));
        Assert.AreEqual("nothing", await ServedAsync(fixture, lambda.PublicKey, "maybe"), "code with a fallback carries on");

        using var refused = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/token");

        Assert.AreEqual(HttpStatusCode.InternalServerError, refused.StatusCode);

        using var summary = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/summary");

        var said = await summary.GetContentAsync<LambdaSummaryResponse>();

        Assert.IsTrue(said.RecentProblems.Any(p => (p.Text + p.Detail).Contains("switched off")),
                      "the owner reads why in the log: " + string.Join(" | ", said.RecentProblems.Select(p => p.Text)));

        Assert.IsFalse(said.Storage.SecretsEnabled);
        Assert.IsTrue(said.Storage.UsesSecrets, "which is what the editor warns about before it is switched off");
    }

    [TestMethod]
    public async Task SwitchingSecretsOffDeletesThemAndTheCopiesFeaturesHold()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual(1, await CountAsync(fixture, lambda.PrivateKey, feature));

        using var off = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets");

        var store = await off.GetContentAsync<DataStoreResponse>();

        Assert.IsFalse(store.Enabled);
        Assert.AreEqual(0, store.Items);

        Assert.AreEqual(0, await CountAsync(fixture, lambda.PrivateKey, null));
        Assert.AreEqual(0, await CountAsync(fixture, lambda.PrivateKey, feature), "a copy is still the data");

        // and switched on again, they start empty
        await EnableAsync(fixture, lambda.PrivateKey);

        Assert.IsEmpty((await ReadListingAsync(fixture, lambda.PrivateKey)).Secrets);
    }

    #endregion

    #region Nobody reads a value back

    [TestMethod]
    public async Task NothingAnswersWithAValue()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        using var put = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        var body = await put.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, body);
        Assert.IsFalse(body.Contains(Value, StringComparison.Ordinal), "not even the answer to setting it");
        Assert.Contains("API_TOKEN", body);

        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey);

        foreach (var path in new[]
                 {
                     $"/api/v1/lambdas/{lambda.PrivateKey}/secrets",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/data",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/summary",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/secrets",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/data",
                     $"/api/v1/lambdas/{lambda.PrivateKey}/export"
                 })
        {
            using var response = await fixture.GetAsync(path);

            var text = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());

            Assert.IsFalse(text.Contains(Value, StringComparison.Ordinal), $"{path} gave the value away");
        }

        using var single = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/secrets/API_TOKEN");

        Assert.AreNotEqual(HttpStatusCode.OK, single.StatusCode, "there is no way to ask for one");

        var listing = await ReadListingAsync(fixture, lambda.PrivateKey);

        Assert.AreEqual("API_TOKEN", listing.Secrets.Single().Name);
    }

    [TestMethod]
    public async Task AValueIsNeverWrittenDownAsItWasGiven()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var first = await fixture.CreateLambdaAsync();
        var second = await fixture.CreateLambdaAsync();

        foreach (var lambda in new[] { first, second })
        {
            await EnableAsync(fixture, lambda.PrivateKey);
            await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);
        }

        var sealedValues = new List<byte[]>();

        await using (var connection = new SqliteConnection($"Data Source={fixture.Options.DatabaseFile};Mode=ReadOnly;Pooling=False"))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT value FROM secrets ORDER BY id";

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sealedValues.Add((byte[])reader["value"]);
            }
        }

        Assert.HasCount(2, sealedValues);

        foreach (var stored in sealedValues)
        {
            Assert.IsFalse(Contains(stored, Encoding.UTF8.GetBytes(Value)), "encrypted, not hidden");
        }

        CollectionAssert.AreNotEqual(sealedValues[0], sealedValues[1], "the same value of two lambdas is two different rows");

        // and nowhere else on disk either: the code, the workspaces, the logs of the server
        foreach (var file in Directory.EnumerateFiles(fixture.Options.DataDirectory, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith("lambda.db", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.IsFalse(Contains(await ReadSharedAsync(file), Encoding.UTF8.GetBytes(Value)), $"{file} holds it");
        }
    }

    [TestMethod]
    public async Task AnAgentSeesNamesAndNeverValues()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        var refused = Structured(await CallToolAsync(fixture, "set_secret", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey, ["name"] = "API_TOKEN", ["value"] = Value
        }));

        Assert.IsFalse(refused["ok"]?.GetValue<bool>() ?? false, "off, it says how to switch them on");

        var answer = await CallToolAsync(fixture, "enable_data", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["kind"] = "secrets" });

        Assert.IsTrue(Structured(answer)["enabled"]!.GetValue<bool>());

        answer = await CallToolAsync(fixture, "set_secret", new JsonObject
        {
            ["privateKey"] = lambda.PrivateKey, ["name"] = "API_TOKEN", ["value"] = Value
        });

        Assert.IsFalse(answer.ToJsonString().Contains(Value, StringComparison.Ordinal));
        Assert.AreEqual("API_TOKEN", Structured(answer)["name"]!.GetValue<string>());

        answer = await CallToolAsync(fixture, "list_secrets", new JsonObject { ["privateKey"] = lambda.PrivateKey });

        Assert.IsFalse(answer.ToJsonString().Contains(Value, StringComparison.Ordinal));
        Assert.AreEqual("API_TOKEN", ((JsonArray)Structured(answer)["secrets"]!).Single()!["name"]!.GetValue<string>());

        answer = await CallToolAsync(fixture, "read_lambda", new JsonObject { ["privateKey"] = lambda.PrivateKey });

        Assert.IsFalse(answer.ToJsonString().Contains(Value, StringComparison.Ordinal), "the overview of a lambda has none either");

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        Assert.AreEqual(Value, await ServedAsync(fixture, lambda.PublicKey, "token"), "what the agent set is what the code reads");

        answer = await CallToolAsync(fixture, "delete_secret", new JsonObject { ["privateKey"] = lambda.PrivateKey, ["name"] = "API_TOKEN" });

        Assert.IsTrue(Structured(answer)["ok"]!.GetValue<bool>());
        Assert.AreEqual("nothing", await ServedAsync(fixture, lambda.PublicKey, "maybe"));
    }

    [TestMethod]
    public async Task AnAgentCanSwitchDataOnAndHasNoWayToSwitchItOff()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var tools = (JsonArray)(await CallAsync(fixture, "tools/list", new JsonObject()))["result"]!["tools"]!;

        var names = tools.Select(t => t!["name"]!.GetValue<string>()).ToList();

        Assert.Contains("enable_data", names);
        Assert.DoesNotContain("disable_data", names, "switching one off deletes what it held, which is the owner's to do");

        var guide = Structured(await CallToolAsync(fixture, "platform_guide", new JsonObject()));

        Assert.Contains("Secret.Read", guide["secrets"]!["surface"]!.ToJsonString());
        Assert.Contains("set_secret", guide["lifecycle"]!["whereThingsGo"]!["credentials"]!.GetValue<string>());
    }

    #endregion

    #region Their key

    [TestMethod]
    public async Task AKeyOfTheInstallationIsMadeWhenNoneIsConfigured()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        Assert.IsTrue(File.Exists(fixture.Options.SecretsKeyFile), "made beside the database when the first secret needed it");
    }

    [TestMethod]
    public async Task AKeyThatIsTooShortIsRefusedAtStartup()
    {
        var failure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await using var _ = await LambdaFixture.CreateAsync(o => o with { SecretsKey = "short" });
        });

        Assert.Contains("LAMBDA_SECRETS_KEY", failure.Message);
    }

    [TestMethod]
    public async Task ADatabaseAloneReadsNothingButWithItsKeyItMovesToAnotherServer()
    {
        const string key = "a-key-of-the-installation-that-is-long-enough";

        var moved = Path.Combine(Path.GetTempPath(), "genhttp-lambda-tests", Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(moved);

        try
        {
            long id;

            await using (var old = await LambdaFixture.CreateAsync(o => o with { SecretsKey = key }))
            {
                var lambda = await old.CreateLambdaAsync();

                await EnableAsync(old, lambda.PrivateKey);
                await SetOkAsync(old, lambda.PrivateKey, "API_TOKEN", Value);

                id = (await old.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

                // a backup, the way SQLite makes one while the server runs
                await using var source = new SqliteConnection($"Data Source={old.Options.DatabaseFile};Pooling=False");
                await using var target = new SqliteConnection($"Data Source={Path.Combine(moved, "lambda.db")};Pooling=False");

                await source.OpenAsync();
                await target.OpenAsync();

                source.BackupDatabase(target);
            }

            // the database without the key: a server of its own, and a key of its own
            await using (var without = await LambdaFixture.CreateAsync(o => o with { DataDirectory = moved }))
            {
                var access = without.Application.Services.GetRequiredService<ISecretVault>().AccessFor(id);

                var failure = Assert.ThrowsExactly<InvalidOperationException>(() => access.Read("API_TOKEN"));

                Assert.Contains("cannot be opened", failure.Message);
            }

            // and with it: nothing else has to travel
            File.Delete(Path.Combine(moved, "secrets.key"));

            await using (var restored = await LambdaFixture.CreateAsync(o => o with { DataDirectory = moved, SecretsKey = key }))
            {
                var access = restored.Application.Services.GetRequiredService<ISecretVault>().AccessFor(id);

                Assert.AreEqual(Value, access.Read("API_TOKEN"));
            }
        }
        finally
        {
            try
            {
                Directory.Delete(moved, true);
            }
            catch (IOException)
            {
                // left for the temporary directory to be cleaned
            }
        }
    }

    #endregion

    #region The demo

    [TestMethod]
    public async Task ACopyOfTheRegistrationDemoIsClosedToWhoHasNoInviteOnceItHasASecret()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("invite-only", "demo-registration");

        await fixture.DeployAsync(lambda.PrivateKey);

        using (var open = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/invite", "application/json"))
        {
            Assert.IsFalse(JsonNode.Parse(await open.Content.ReadAsStringAsync())!["required"]!.GetValue<bool>(), "as it comes, anybody registers");
        }

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "INVITE_CODE", "open-sesame");

        using (var closed = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/invite", "application/json"))
        {
            Assert.IsTrue(JsonNode.Parse(await closed.Content.ReadAsStringAsync())!["required"]!.GetValue<bool>(), "and with the secret, not without an invite");
        }

        using (var without = await fixture.SendAsync(HttpMethod.Post, $"/lambda/{lambda.PublicKey}/register",
                                                     new { name = "ada", password = "correct horse" }, "application/json"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, without.StatusCode);
        }

        using (var wrong = await fixture.SendAsync(HttpMethod.Post, $"/lambda/{lambda.PublicKey}/register",
                                                   new { name = "ada", password = "correct horse", inviteCode = "guess" }, "application/json"))
        {
            Assert.AreEqual(HttpStatusCode.Forbidden, wrong.StatusCode);
        }

        using var invited = await fixture.SendAsync(HttpMethod.Post, $"/lambda/{lambda.PublicKey}/register",
                                                    new { name = "ada", password = "correct horse", inviteCode = "open-sesame" }, "application/json");

        Assert.AreEqual(HttpStatusCode.OK, invited.StatusCode, await invited.Content.ReadAsStringAsync());
    }

    #endregion

    #region Features

    [TestMethod]
    public async Task AFeatureStartsWithACopyAndItsPreviewReadsIt()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", "the-live-one");

        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey);

        var copy = await ReadListingAsync(fixture, lambda.PrivateKey, feature);

        Assert.AreEqual("API_TOKEN", copy.Secrets.Single().Name);

        // changed in the copy only
        using (var put = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", "the-test-one", feature))
        {
            Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
        }

        using (var deployed = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/files?deploy=true",
                                                      new FeatureFilesRequest(LambdaFixture.Version(Reader).Files)))
        {
            Assert.AreEqual(HttpStatusCode.OK, deployed.StatusCode, await deployed.Content.ReadAsStringAsync());
        }

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        using var preview = await fixture.GetAsync($"/features/{feature}/token");
        using var live = await fixture.GetAsync($"/lambda/{lambda.PublicKey}/token");

        Assert.AreEqual("the-test-one", await preview.GetContentAsync(), "the preview reads its own");
        Assert.AreEqual("the-live-one", await live.GetContentAsync(), "and the lambda is not touched");

        // taking the lambda's again
        using var refreshed = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}/data/refresh");

        Assert.AreEqual(HttpStatusCode.OK, refreshed.StatusCode);

        using var again = await fixture.GetAsync($"/features/{feature}/token");

        Assert.AreEqual("the-live-one", await again.GetContentAsync());
    }

    [TestMethod]
    public async Task ACopyGoesWithItsFeature()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        var feature = await CreateFeatureAsync(fixture, lambda.PrivateKey);

        using (var deleted = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{feature}"))
        {
            Assert.AreEqual(HttpStatusCode.NoContent, deleted.StatusCode);
        }

        var vault = fixture.Application.Services.GetRequiredService<ISecretVault>();

        var id = (await fixture.Meta.GetIdAsync(lambda.PrivateKey))!.Value;

        Assert.AreEqual(1, await vault.CountAsync(id), "the lambda's own are still there");

        await using var connection = new SqliteConnection($"Data Source={fixture.Options.DatabaseFile};Mode=ReadOnly;Pooling=False");

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM secrets WHERE feature_id IS NOT NULL";

        Assert.AreEqual(0L, await command.ExecuteScalarAsync());
    }

    #endregion

    #region The rules

    [TestMethod]
    public async Task ANameIsTheShapeOfAnEnvironmentVariable()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        foreach (var bad in new[] { "stripe key", "1TOKEN", "a-b", "with.dot", new string('A', 65) })
        {
            using var response = await SetAsync(fixture, lambda.PrivateKey, Uri.EscapeDataString(bad), Value);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, $"'{bad}' is not a name");
        }

        foreach (var good in new[] { "STRIPE_API_KEY", "_x", "lower_case1", new string('A', 64) })
        {
            using var response = await SetAsync(fixture, lambda.PrivateKey, good, Value);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }

    [TestMethod]
    public async Task AValueNeedsToBeThereAndFitAndKeepsNoTrailingLineBreak()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxSecretLength = 10 });

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        using (var empty = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", ""))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        }

        using (var lines = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", "\r\n"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, lines.StatusCode, "a value that is only what a paste brought along is empty");
        }

        using (var long_ = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", new string('x', 11)))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, long_.StatusCode);
        }

        using (var pasted = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", "abc\n"))
        {
            Assert.AreEqual(HttpStatusCode.OK, pasted.StatusCode);
        }

        await fixture.DeployAsync(lambda.PrivateKey, Reader);

        Assert.AreEqual("abc", await ServedAsync(fixture, lambda.PublicKey, "token"));
    }

    [TestMethod]
    public async Task ThereIsALimitToHowManyASecretsALambdaKeeps()
    {
        await using var fixture = await LambdaFixture.CreateAsync(o => o with { MaxSecrets = 2 });

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        await SetOkAsync(fixture, lambda.PrivateKey, "ONE", Value);
        await SetOkAsync(fixture, lambda.PrivateKey, "TWO", Value);

        using var third = await SetAsync(fixture, lambda.PrivateKey, "THREE", Value);

        Assert.AreEqual(HttpStatusCode.Conflict, third.StatusCode);

        // replacing one is not adding one
        await SetOkAsync(fixture, lambda.PrivateKey, "TWO", "again");
    }

    [TestMethod]
    public async Task DeletingASecretThereIsNotIsNotFound()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await EnableAsync(fixture, lambda.PrivateKey);

        using var response = await fixture.SendAsync(HttpMethod.Delete, $"/api/v1/lambdas/{lambda.PrivateKey}/secrets/NOTHING");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ADemoHasNoSecretsToChange()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("secrets-of-a-demo");

        await fixture.MakeDemoAsync(lambda.PublicKey);

        using var put = await SetAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        Assert.AreEqual(HttpStatusCode.Forbidden, put.StatusCode);

        using var on = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/data/secrets");

        Assert.AreEqual(HttpStatusCode.Forbidden, on.StatusCode);
    }

    [TestMethod]
    public async Task AKeyThatNamesNothingHasNoSecrets()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        using var response = await fixture.GetAsync("/api/v1/lambdas/nobody-has-this-key/secrets");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Taking it away

    [TestMethod]
    public async Task AnExportReadsSecretsFromTheEnvironmentAndNeverContainsThem()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("with-secrets");

        await EnableAsync(fixture, lambda.PrivateKey);
        await SetOkAsync(fixture, lambda.PrivateKey, "API_TOKEN", Value);

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", LambdaFixture.Version(Reader));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode);

        using var export = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/export");

        using var zip = new ZipArchive(new MemoryStream(await export.Content.ReadAsByteArrayAsync()));

        string Read(string name)
        {
            using var reader = new StreamReader(zip.GetEntry($"with-secrets/{name}")!.Open());

            return reader.ReadToEnd();
        }

        Assert.Contains("Environment.GetEnvironmentVariable(name)", Read("Lambda.cs"));
        Assert.Contains("public static Secrets Secret", Read("Lambda.cs"));
        Assert.Contains("private static Secrets Secret => Lambda.Secret;", Read("Program.cs"));
        Assert.Contains("`API_TOKEN`", Read("README.md"), "it says which ones to set");

        foreach (var entry in zip.Entries)
        {
            using var stream = entry.Open();
            using var copy = new MemoryStream();

            stream.CopyTo(copy);

            Assert.IsFalse(Contains(copy.ToArray(), Encoding.UTF8.GetBytes(Value)), $"{entry.FullName} holds a secret");
        }
    }

    [TestMethod]
    public void AnExportOfCodeWithoutSecretsSaysSo()
    {
        var files = new List<LambdaFile> { new(LambdaSource.EntryName, "return Content.From(Resource.FromString(\"hi\"));") };

        using var zip = new ZipArchive(new MemoryStream(ProjectPacker.Pack("plain", files)));

        using var reader = new StreamReader(zip.GetEntry("plain/README.md")!.Open());

        Assert.Contains("did not read any secret", reader.ReadToEnd());
    }

    #endregion

    #region Plumbing

    private static async Task<List<DataStoreResponse>> ListAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.GetAsync($"/api/v1/lambdas/{privateKey}/data");

        return await response.GetContentAsync<List<DataStoreResponse>>();
    }

    private static async Task<SecretListingResponse> ReadListingAsync(LambdaFixture fixture, string privateKey, string? feature = null)
    {
        using var response = await fixture.GetAsync(feature == null
            ? $"/api/v1/lambdas/{privateKey}/secrets"
            : $"/api/v1/lambdas/{privateKey}/features/{feature}/secrets");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<SecretListingResponse>();
    }

    private static async Task<int> CountAsync(LambdaFixture fixture, string privateKey, string? feature)
        => (await ReadListingAsync(fixture, privateKey, feature)).Secrets.Count;

    private static async Task EnableAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{privateKey}/data/secrets");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        Assert.IsTrue((await response.GetContentAsync<DataStoreResponse>()).Enabled);
    }

    private static Task<HttpResponseMessage> SetAsync(LambdaFixture fixture, string privateKey, string name, string value, string? feature = null)
        => fixture.SendAsync(HttpMethod.Put, feature == null
                                 ? $"/api/v1/lambdas/{privateKey}/secrets/{name}"
                                 : $"/api/v1/lambdas/{privateKey}/features/{feature}/secrets/{name}", new SecretRequest(value));

    private static async Task SetOkAsync(LambdaFixture fixture, string privateKey, string name, string value)
    {
        using var response = await SetAsync(fixture, privateKey, name, value);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private static async Task<string> CreateFeatureAsync(LambdaFixture fixture, string privateKey)
    {
        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{privateKey}/features", new CreateFeatureRequest("Try"));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());

        return (await response.GetContentAsync<FeatureResponse>()).Key;
    }

    private static async Task<string> ServedAsync(LambdaFixture fixture, string publicKey, string path)
    {
        using var response = await fixture.GetAsync($"/lambda/{publicKey}/{path}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync();
    }

    private static bool Contains(byte[] haystack, byte[] needle) => haystack.AsSpan().IndexOf(needle) >= 0;

    private static async Task<byte[]> ReadSharedAsync(string file)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var copy = new MemoryStream();

        await stream.CopyToAsync(copy);

        return copy.ToArray();
    }

    private static JsonObject Structured(JsonObject answer) => (JsonObject)answer["result"]!["structuredContent"]!;

    private static Task<JsonObject> CallToolAsync(LambdaFixture fixture, string tool, JsonObject arguments)
        => CallAsync(fixture, "tools/call", new JsonObject { ["name"] = tool, ["arguments"] = arguments });

    private static async Task<JsonObject> CallAsync(LambdaFixture fixture, string method, JsonObject parameters)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 1,
            ["method"] = method,
            ["params"] = parameters
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        return (JsonObject)JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
    }

    #endregion

}
