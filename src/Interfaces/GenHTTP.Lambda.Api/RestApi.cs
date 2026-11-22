namespace GenHTTP.Lambda.Api;

/// <summary>
/// The REST API of the platform, at <c>/api</c>: everything the editor, the
/// panel, the pages and an agent with HTTP do, described by its OpenAPI
/// document and browsable at <c>/api/v1/scalar/</c>.
/// </summary>
/// <example>
/// <code>
/// Layout.Create().Add("api", RestApi.Create(platform));
/// </code>
/// </example>
public static class RestApi
{

    /// <summary>
    /// The API over the given platform.
    /// </summary>
    public static RestApiBuilder Create(LambdaPlatform platform) => new(platform);

}
