using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Api.Infrastructure;

internal sealed class UnbufferedConcernBuilder : IConcernBuilder
{
    public IConcern Build(IHandler content) => new UnbufferedConcern(content);
}
