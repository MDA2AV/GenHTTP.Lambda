using System.IO.Compression;
using System.Net;

using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// What the server is allowed to do to a response on the way out.
/// </summary>
/// <remarks>
/// Brotli and zstd used to truncate generated content, fixed in GenHTTP
/// 11.0.3. Every browser asks for brotli first and curl asks for nothing, so
/// the broken response was the one people got and the whole one was what
/// anybody checking by hand saw - which is how it went unnoticed until a menu
/// quietly stopped filling in. Kept so it cannot come back unseen.
/// </remarks>
[TestClass]
public sealed class CompressionTests
{

    [TestMethod]
    [DataRow("br")]
    [DataRow("zstd")]
    [DataRow("gzip, deflate, br, zstd")]
    [DataRow("gzip")]
    public async Task ALargeAnswerSurvivesWhateverTheClientAsksFor(string encodings)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var lambda = await fixture.CreateLambdaAsync("bulky");

        // comfortably past where the truncation began
        await fixture.DeployAsync(lambda.PrivateKey,
            "return Inline.Create().Get(() => new Blob(new string('x', 80_000)));\n\nrecord Blob(string Data);");

        using var request = fixture.Host.GetRequest($"/lambda/{lambda.PublicKey}/", HttpMethod.Get);

        request.Headers.Add("Accept-Encoding", encodings);

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await ReadAsync(response);

        Assert.Contains("\"data\"", body, $"the answer was cut short for '{encodings}'");
        Assert.IsTrue(body.Length > 80_000, $"only {body.Length} characters arrived for '{encodings}'");
    }

    /// <summary>
    /// Reads the body, undoing whatever the server compressed it with.
    /// </summary>
    /// <remarks>
    /// The test client does not decompress by itself, and a truncated stream
    /// is exactly what this is looking for - so the decompression has to
    /// happen here rather than be taken on trust.
    /// </remarks>
    private static async Task<string> ReadAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();

        var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

        await using Stream reading = encoding switch
        {
            "gzip" => new GZipStream(stream, CompressionMode.Decompress),
            "br" => new BrotliStream(stream, CompressionMode.Decompress),
            "zstd" => new ZstandardStream(stream, CompressionMode.Decompress),
            _ => stream
        };

        using var reader = new StreamReader(reading);

        return await reader.ReadToEndAsync();
    }

}
