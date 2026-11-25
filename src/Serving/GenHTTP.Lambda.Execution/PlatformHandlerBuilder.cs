using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Lambda.Infrastructure;

namespace GenHTTP.Lambda.Execution;

/// <summary>
/// Configures a platform as a handler.
/// </summary>
public sealed class PlatformHandlerBuilder(LambdaPlatform platform) : IHandlerBuilder<PlatformHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IHandlerBuilder? _handler;

    #region Functionality

    /// <summary>
    /// What answers every request to the platform - usually the runner around
    /// the site.
    /// </summary>
    public PlatformHandlerBuilder Handler(IHandlerBuilder handler)
    {
        _handler = handler;
        return this;
    }

    /// <summary>
    /// A concern around everything the platform does with a request.
    /// </summary>
    public PlatformHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var handler = _handler ?? throw new BuilderMissingPropertyException("Handler");

        return Concerns.Chain([.. PlatformConcerns.Inner(platform), .. PlatformConcerns.Outer(platform), .. _concerns], handler.Build());
    }

    #endregion

}
