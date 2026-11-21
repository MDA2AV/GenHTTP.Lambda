using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Services.Protection;

public sealed class LambdaResolutionConcernBuilder(ILambdaLocator locator) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaResolutionConcern(content, locator);
}
