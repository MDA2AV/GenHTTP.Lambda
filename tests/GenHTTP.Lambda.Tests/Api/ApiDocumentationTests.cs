using System.Net;
using System.Text.Json;

using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// The OpenAPI document as whoever calls the API reads it: every operation,
/// parameter and field says what it is, and every answer says what it means.
/// </summary>
/// <remarks>
/// What is checked is the document as it is served, so these fail for an
/// endpoint or a field that was added without saying anything about itself -
/// which is how a description goes missing.
/// </remarks>
[TestClass]
public sealed class ApiDocumentationTests
{

    #region Operations

    [TestMethod]
    public async Task EveryOperationSaysWhatItDoes()
    {
        using var document = await ReadAsync();

        var missing = Operations(document).Where(o => string.IsNullOrWhiteSpace(Text(o.Operation, "summary")))
                                          .Select(o => o.Name)
                                          .ToList();

        Assert.IsEmpty(missing, $"Without a summary: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public async Task EveryParameterIsDescribed()
    {
        using var document = await ReadAsync();

        var missing = new List<string>();

        foreach (var (name, operation) in Operations(document))
        {
            if (!operation.TryGetProperty("parameters", out var parameters))
            {
                continue;
            }

            missing.AddRange(parameters.EnumerateArray()
                                       .Where(p => string.IsNullOrWhiteSpace(Text(p, "description")))
                                       .Select(p => $"{name} {p.GetProperty("name").GetString()}"));
        }

        Assert.IsEmpty(missing, $"Without a description: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public async Task EveryAnswerSaysWhatItMeans()
    {
        using var document = await ReadAsync();

        var missing = new List<string>();

        foreach (var (name, operation) in Operations(document))
        {
            foreach (var response in operation.GetProperty("responses").EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(Text(response.Value, "description")))
                {
                    missing.Add($"{name} {response.Name}");
                }
            }
        }

        Assert.IsEmpty(missing, $"Without a description: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public async Task EveryCallAboutALambdaSaysHowAnUnknownKeyIsAnswered()
    {
        using var document = await ReadAsync();

        var missing = Operations(document).Where(o => o.Name.Contains("/lambdas/{privateKey}", StringComparison.Ordinal))
                                          .Where(o => !Refusal(o.Operation, "404"))
                                          .Select(o => o.Name)
                                          .ToList();

        Assert.IsEmpty(missing, $"Without a 404 carrying an error: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public async Task WhatOnlyTheOperatorMayCallAsksForTheToken()
    {
        using var document = await ReadAsync();

        var operations = Operations(document).Where(o => o.Name.Contains(" /admin", StringComparison.Ordinal)
                                                      || o.Name.Contains(" /telemetry", StringComparison.Ordinal)
                                                      || o.Name.Contains(" /logs", StringComparison.Ordinal))
                                             .ToList();

        Assert.IsNotEmpty(operations, "an installation with a token describes its panel");

        foreach (var (name, operation) in operations)
        {
            var token = operation.GetProperty("parameters").EnumerateArray()
                                 .FirstOrDefault(p => p.GetProperty("name").GetString() == "X-Admin-Token");

            Assert.AreEqual("header", token.GetProperty("in").GetString(), $"{name} asks for the token in a header");
            Assert.IsTrue(token.GetProperty("required").GetBoolean(), $"{name} requires the token");

            var responses = operation.GetProperty("responses");

            Assert.IsTrue(responses.TryGetProperty("401", out _), $"{name} says how a missing token is answered");
            Assert.IsTrue(responses.TryGetProperty("403", out _), $"{name} says how a wrong token is answered");
        }
    }

    #endregion

    #region What is answered

    [TestMethod]
    public async Task WhatIsCreatedIsAnsweredWithCreated()
    {
        using var document = await ReadAsync();

        foreach (var name in new[] { "post /lambdas", "post /lambdas/{privateKey}/versions", "post /lambdas/{privateKey}/versions/zip",
                                     "post /lambdas/{privateKey}/versions/changes", "post /lambdas/{privateKey}/features" })
        {
            var responses = Operation(document, name).GetProperty("responses");

            Assert.IsTrue(responses.TryGetProperty("201", out var created), $"{name} answers with 201");
            Assert.IsTrue(created.TryGetProperty("content", out _), $"{name} says what it answers 201 with");

            Assert.IsFalse(responses.TryGetProperty("200", out _), $"{name} never answers with 200");
        }
    }

    [TestMethod]
    public async Task WhatTheAgentWillDoIsAnsweredWithAccepted()
    {
        using var document = await ReadAsync();

        foreach (var name in new[] { "post /builds/", "post /lambdas/{privateKey}/agent/start" })
        {
            var responses = Operation(document, name).GetProperty("responses");

            Assert.IsTrue(responses.TryGetProperty("202", out _), $"{name} answers with 202");
            Assert.IsFalse(responses.TryGetProperty("200", out _), $"{name} never answers with 200");
        }
    }

    [TestMethod]
    public async Task CodeThatDoesNotCompileIsAnsweredWithWhatTheCompilerSaid()
    {
        using var document = await ReadAsync();

        foreach (var name in new[] { "post /lambdas/{privateKey}/deployment/start", "post /lambdas/{privateKey}/features/{feature}/preview/start",
                                     "post /lambdas/{privateKey}/features/{feature}/merge" })
        {
            var responses = Operation(document, name).GetProperty("responses");

            Assert.IsTrue(responses.TryGetProperty("422", out var refused), $"{name} answers code that does not compile with 422");
            Assert.IsTrue(refused.TryGetProperty("content", out _), $"{name} says what the compiler said");
        }
    }

    [TestMethod]
    public async Task ArchivesAreDescribedAsArchives()
    {
        using var document = await ReadAsync();

        foreach (var name in new[] { "get /lambdas/{privateKey}/export", "get /lambdas/{privateKey}/versions/{version}/zip",
                                     "get /lambdas/{privateKey}/features/{feature}/zip", "get /sources/{publicKey}/versions/{version}/zip" })
        {
            var ok = Operation(document, name).GetProperty("responses").GetProperty("200");

            Assert.IsTrue(ok.GetProperty("content").TryGetProperty("application/zip", out _), $"{name} answers with a zip");
        }

        foreach (var name in new[] { "post /lambdas/{privateKey}/versions/zip", "put /lambdas/{privateKey}/features/{feature}/zip" })
        {
            var body = Operation(document, name).GetProperty("requestBody");

            Assert.IsTrue(body.GetProperty("content").TryGetProperty("application/zip", out _), $"{name} takes a zip");
        }
    }

    [TestMethod]
    public async Task FilesTakenAsTheyAreAreDescribedAsBytes()
    {
        using var document = await ReadAsync();

        foreach (var name in new[] { "put /lambdas/{privateKey}/files/{path}/content", "put /lambdas/{privateKey}/features/{feature}/workspace/{path}/content" })
        {
            var body = Operation(document, name).GetProperty("requestBody");

            Assert.IsTrue(body.GetProperty("content").TryGetProperty("application/octet-stream", out _), $"{name} takes the bytes of the file");
        }
    }

    #endregion

    #region Models

    [TestMethod]
    public async Task EveryModelAndFieldIsDescribed()
    {
        using var document = await ReadAsync();

        var missing = new List<string>();

        foreach (var schema in document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(Text(schema.Value, "description")))
            {
                missing.Add(schema.Name);
            }

            if (!schema.Value.TryGetProperty("properties", out var properties))
            {
                continue;
            }

            missing.AddRange(properties.EnumerateObject()
                                       .Where(p => string.IsNullOrWhiteSpace(Text(p.Value, "description")))
                                       .Select(p => $"{schema.Name}.{p.Name}")
                                       .Where(p => !NeverSent.Contains(p)));
        }

        Assert.IsEmpty(missing, $"Without a description: {string.Join(", ", missing)}");
    }

    #endregion

    #region Reading order

    [TestMethod]
    public async Task TheDocumentOpensWithAnIntroduction()
    {
        using var document = await ReadAsync();

        var info = document.RootElement.GetProperty("info");

        Assert.AreEqual("GenHTTP Lambda API", info.GetProperty("title").GetString());
        Assert.AreEqual("v1", info.GetProperty("version").GetString());

        var introduction = info.GetProperty("description").GetString()!;

        foreach (var section in new[] { "## Getting started", "## Who may do what", "## Files", "## Limits", "## Answers and errors" })
        {
            Assert.Contains(section, introduction);
        }
    }

    [TestMethod]
    public async Task EveryTagIsDescribedAndInAGroup()
    {
        using var document = await ReadAsync();

        var grouped = document.RootElement.GetProperty("x-tagGroups").EnumerateArray()
                              .SelectMany(g => g.GetProperty("tags").EnumerateArray().Select(t => t.GetString()))
                              .ToHashSet();

        var used = Operations(document).SelectMany(o => o.Operation.GetProperty("tags").EnumerateArray().Select(t => t.GetString()!))
                                       .ToHashSet();

        var ungrouped = used.Where(t => !grouped.Contains(t)).ToList();

        Assert.IsEmpty(ungrouped, $"In no group: {string.Join(", ", ungrouped)}");

        var tags = document.RootElement.GetProperty("tags").EnumerateArray().ToList();

        var undescribed = tags.Where(t => string.IsNullOrWhiteSpace(Text(t, "description"))).Select(t => t.GetProperty("name").GetString()).ToList();

        Assert.IsEmpty(undescribed, $"Without a description: {string.Join(", ", undescribed)}");

        var unused = grouped.Where(t => !used.Contains(t!)).ToList();

        Assert.IsEmpty(unused, $"Grouped but never used: {string.Join(", ", unused)}");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Properties the schemas list although they are never sent.
    /// </summary>
    /// <remarks>
    /// GenHTTP 11.1 generates the schemas with Newtonsoft's view of a type,
    /// while the API serializes with System.Text.Json - so a property marked
    /// <c>[JsonIgnore]</c> for the one is listed by the other. Reported
    /// upstream; describing them would document fields nobody receives.
    /// </remarks>
    private static readonly HashSet<string> NeverSent = ["LambdaFile.IsCompiled", "LambdaFile.IsCode", "LambdaFile.IsResource", "LambdaFile.Bytes"];

    /// <summary>
    /// The document of an installation with a panel, which describes the
    /// operator's endpoints as well.
    /// </summary>
    private static async Task<JsonDocument> ReadAsync()
    {
        await using var fixture = await LambdaFixture.CreateAsync(LambdaFixture.WithPanel);

        using var response = await fixture.GetAsync("/api/v1/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        return JsonDocument.Parse(await response.GetContentAsync());
    }

    /// <summary>
    /// Every operation, named by its method and its path: <c>get /lambdas/{privateKey}</c>.
    /// </summary>
    private static IEnumerable<(string Name, JsonElement Operation)> Operations(JsonDocument document)
    {
        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                yield return ($"{operation.Name} {path.Name}", operation.Value);
            }
        }
    }

    private static JsonElement Operation(JsonDocument document, string name)
        => Operations(document).FirstOrDefault(o => o.Name == name).Operation is { ValueKind: JsonValueKind.Object } found
         ? found
         : throw new AssertFailedException($"There is no operation '{name}'.");

    /// <summary>
    /// Whether the operation says it answers with this status and the body of an error.
    /// </summary>
    private static bool Refusal(JsonElement operation, string status)
        => operation.GetProperty("responses").TryGetProperty(status, out var response)
        && response.TryGetProperty("content", out var content)
        && content.TryGetProperty("application/json", out var json)
        && json.GetProperty("schema").GetRawText().Contains("ErrorResponse", StringComparison.Ordinal);

    private static string? Text(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    #endregion

}
