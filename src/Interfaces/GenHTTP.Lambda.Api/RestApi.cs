using GenHTTP.Api.Content;

using GenHTTP.Lambda.Api.Infrastructure;

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

/// <summary>
/// Configures the REST API.
/// </summary>
/// <remarks>
/// The operator's routes - the panel's lambdas, settings, limits, blocks,
/// telemetry and log - are there only where the platform has an admin token,
/// and answer only to it (see <c>AdminAuthentication</c>).
/// </remarks>
public sealed class RestApiBuilder(LambdaPlatform platform) : IHandlerBuilder<RestApiBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private bool _compiled = true;

    #region Functionality

    /// <summary>
    /// Whether the resources are compiled into delegates as the server
    /// starts (the default), or called through reflection - which starts in
    /// milliseconds rather than a second, for tests.
    /// </summary>
    public RestApiBuilder Compiled(bool compiled = true)
    {
        _compiled = compiled;
        return this;
    }

    public RestApiBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build() => Concerns.Chain(_concerns, ApiLayout.Create(platform.Options, _compiled).Build());

    #endregion

}
