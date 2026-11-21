using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Services.Hosting;

public sealed class LambdaRedirectConcernBuilder(IDomainRegistry registry, ILambdaAddresses addresses) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaRedirectConcern(content, registry, addresses);
}
