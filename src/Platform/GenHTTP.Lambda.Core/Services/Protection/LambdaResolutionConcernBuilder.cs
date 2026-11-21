using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Protection;

public sealed class LambdaResolutionConcernBuilder(ILambdaLocator locator) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaResolutionConcern(content, locator);
}
