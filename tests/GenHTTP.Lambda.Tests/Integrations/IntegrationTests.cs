using System.Net;
using System.Text.Json;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.UsageReports;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Integrations;

/// <summary>
/// What the installation keeps its data in, as the operator, an agent and a
/// lambda meet it - on the integrations every installation has unless its
/// host chooses others: SQLite, local files, secrets sealed in the platform's
/// database.
/// </summary>
[TestClass]
public sealed class IntegrationTests
{

    [TestMethod]
    public async Task TheOperatorSeesWhatTheInstallationKeepsItsDataIn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var response = await fixture.GetAsOperatorAsync("/api/v1/admin/integrations");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        var integrations = await response.GetContentAsync<List<IntegrationResponse>>();

        CollectionAssert.AreEqual(new[] { "database", "files", "secrets" }, integrations.Select(i => i.Kind).ToArray(), "in that order");
        CollectionAssert.AreEqual(new[] { "SQLite", "Local", "Internal" }, integrations.Select(i => i.Name).ToArray());

        Assert.IsTrue(integrations.All(i => i.Default), "the platform's own, since the host chose none");

        var database = integrations[0];

        Assert.IsTrue(database.Details.Any(d => d.Value.EndsWith("lambda.db", StringComparison.Ordinal)), "where the platform's records are");
    }

    [TestMethod]
    public async Task OnlyTheOperatorIsToldWhatTheDataIsKeptIn()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var response = await fixture.GetAsync("/api/v1/admin/integrations");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ALambdaKeepsItsRecordsWithoutNamingItsDatabase()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync();

        await NeutralNotes.SaveAsync(fixture, lambda.PrivateKey);

        var address = $"http://{lambda.PublicKey}.localhost";

        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, $"{address}/add?text=milk"));
        Assert.AreEqual("1", await NeutralNotes.ServedAsync(fixture, $"{address}/add?text=bread"));

        Assert.AreEqual("milk,bread", await NeutralNotes.ServedAsync(fixture, $"{address}/notes"));
        Assert.AreEqual("bread", await NeutralNotes.ServedAsync(fixture, $"{address}/done"), "a flag kept as BOOLEAN reads as one");

        using var tables = await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/database");

        var database = await tables.GetContentAsync<DatabaseResponse>();

        Assert.IsTrue(database.Tables.Any(t => t.Name == "notes"), "what the owner reads is the same database");
    }

    [TestMethod]
    public async Task ALambdaCannotNameTheClientOfADatabaseItIsNotOn()
    {
        await using var fixture = await LambdaFixture.CreateCommunityAsync();

        var lambda = await fixture.CreateLambdaAsync();

        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions?deploy=true",
                                                  LambdaFixture.Version("return Inline.Create().Get(() => typeof(Npgsql.NpgsqlConnection).Name);"));

        var version = await saved.GetContentAsync<SavedVersionResponse>();

        Assert.IsFalse(version.Deployment!.Success, "the community host carries PostgreSQL's client, which a lambda on SQLite is not handed");
        Assert.IsTrue(version.Deployment.Diagnostics.Any(d => d.Id is "CS0246" or "CS0234"), string.Join(" | ", version.Deployment.Diagnostics.Select(d => d.Message)));
    }

    [TestMethod]
    public async Task TheGuideSaysWhatKeepsTheDatabaseAndTeachesCodeThatNamesNone()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var database = Guide.Section(await Guide.ReadAsync(fixture), "database");

        Assert.Contains("options.UseDatabase(connection)", database);
        Assert.Contains("DbConnection", database);
        Assert.Contains("Plain SQL", database);
        Assert.Contains("Here: SQLite", database, "what the integration adds to its section");

        var workspace = Guide.Section(await Guide.ReadAsync(fixture), "workspace");

        Assert.DoesNotContain("Workspace.Root", workspace, "a workspace need not be a directory");
    }

    /// <summary>
    /// The demos teach what an agent writes, so they name no database: their
    /// code runs on whichever the installation keeps.
    /// </summary>
    [TestMethod]
    public void EveryDemoNamesNoDatabase()
    {
        foreach (var demo in DemoCatalog.All)
        {
            var files = LambdaSource.Parse(TemplateCatalog.ForKey(demo.Id, demo.Key, demo: true));

            foreach (var file in files.Where(f => f.IsCompiled))
            {
                Assert.DoesNotContain("Sqlite", file.Code, $"{demo.Id}: {file.Name}");
                Assert.DoesNotContain("Npgsql", file.Code, $"{demo.Id}: {file.Name}");
            }

            if (files.Any(f => f.IsCompiled && f.Code.Contains("DbContext", StringComparison.Ordinal)))
            {
                Assert.IsTrue(files.Any(f => f.Code.Contains("options.UseDatabase(connection)", StringComparison.Ordinal)), demo.Id);
            }
        }
    }

    [TestMethod]
    public async Task TheUsageReportNamesTheIntegrationsAndNothingAboutThem()
    {
        await using var collector = FakeCollector.Start();

        await using var fixture = await LambdaFixture.CreateCommunityAsync(o => o with { UsageReportUrl = collector.Url });

        await fixture.Platform.Services.GetRequiredService<IUsageReportService>().SendAsync();

        using var report = JsonDocument.Parse(collector.Reports.Single().Body);

        var configuration = report.RootElement.GetProperty("configuration");

        Assert.AreEqual("sqlite", configuration.GetProperty("database").GetString());
        Assert.AreEqual("local", configuration.GetProperty("files").GetString());
        Assert.AreEqual("internal", configuration.GetProperty("secrets").GetString());

        Assert.DoesNotContain(fixture.Options.DataDirectory, collector.Reports.Single().Body, "which, never where");
    }

}
