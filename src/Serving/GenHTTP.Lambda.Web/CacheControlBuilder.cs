using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Builds a <see cref="CacheControlConcern" /> for a handler.
/// </summary>
public sealed class CacheControlBuilder : IConcernBuilder
{

    private readonly string _value;

    public CacheControlBuilder(string value)
    {
        _value = value;
    }

    public IConcern Build(IHandler content) => new CacheControlConcern(content, _value);

}
