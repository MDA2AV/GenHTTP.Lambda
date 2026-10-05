using System.Net;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

namespace GenHTTP.Lambda.Tests.Git;

/// <summary>
/// What the tests of a lambda's repository do through the API, beside git.
/// </summary>
internal static class Repository
{

    /// <summary>
    /// Where whoever holds the editor key clones the lambda from.
    /// </summary>
    public static string EditorUrl(this LambdaFixture fixture, LambdaResponse lambda) => fixture.Host.GetUrl($"/editor/{lambda.PrivateKey}/{lambda.PublicKey}.git");

    /// <summary>
    /// Where anybody clones a published source from.
    /// </summary>
    public static string SourceUrl(this LambdaFixture fixture, string publicKey) => fixture.Host.GetUrl($"/source/{publicKey}.git");

    /// <summary>
    /// A lambda that says something, and nothing else.
    /// </summary>
    public static string Says(string text) => $"return Content.From(Resource.FromString(\"{text}\"));\n";

    /// <summary>
    /// Saves a version through the API, the way the editor does.
    /// </summary>
    public static async Task<int> SaveAsync(this LambdaFixture fixture, LambdaResponse lambda, string change, params LambdaFile[] files)
    {
        using var saved = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/versions", new VersionRequest(files, null, change));

        Assert.AreEqual(HttpStatusCode.Created, saved.StatusCode, await saved.Content.ReadAsStringAsync());

        return (await saved.GetContentAsync<VersionResponse>()).Version;
    }

    public static async Task<List<VersionResponse>> VersionsAsync(this LambdaFixture fixture, LambdaResponse lambda)
        => await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions")).GetContentAsync<List<VersionResponse>>();

    public static async Task<VersionContentResponse> VersionAsync(this LambdaFixture fixture, LambdaResponse lambda, int version)
        => await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/versions/{version}")).GetContentAsync<VersionContentResponse>();

    public static async Task<List<FeatureResponse>> FeaturesAsync(this LambdaFixture fixture, LambdaResponse lambda)
        => await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features")).GetContentAsync<List<FeatureResponse>>();

    public static async Task<FeatureContentResponse> FeatureAsync(this LambdaFixture fixture, LambdaResponse lambda, string key)
        => await (await fixture.GetAsync($"/api/v1/lambdas/{lambda.PrivateKey}/features/{key}")).GetContentAsync<FeatureContentResponse>();

    public static async Task<FeatureResponse> CreateFeatureAsync(this LambdaFixture fixture, LambdaResponse lambda, string name)
    {
        using var created = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/features", new CreateFeatureRequest(name));

        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode, await created.Content.ReadAsStringAsync());

        return await created.GetContentAsync<FeatureResponse>();
    }

    public static async Task PutFeatureAsync(this LambdaFixture fixture, LambdaResponse lambda, string key, string change, params LambdaFile[] files)
    {
        using var put = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/features/{key}/files", new FeatureFilesRequest(files, Change: change));

        Assert.AreEqual(HttpStatusCode.OK, put.StatusCode, await put.Content.ReadAsStringAsync());
    }

    public static async Task PublishAsync(this LambdaFixture fixture, LambdaResponse lambda, string? license = null)
    {
        using var published = await fixture.SendAsync(HttpMethod.Put, $"/api/v1/lambdas/{lambda.PrivateKey}/source", new SourceRequest(license, null));

        Assert.AreEqual(HttpStatusCode.OK, published.StatusCode, await published.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// What the lambda answers right now.
    /// </summary>
    public static async Task<string> CallAsync(this LambdaFixture fixture, string path)
    {
        using var response = await fixture.GetAsync(path);

        return await response.Content.ReadAsStringAsync();
    }

}
