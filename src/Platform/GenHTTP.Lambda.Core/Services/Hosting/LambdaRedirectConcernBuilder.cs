using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Infrastructure;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Hosting;

public sealed class LambdaRedirectConcernBuilder(IDomainRegistry registry, ILambdaAddresses addresses) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaRedirectConcern(content, registry, addresses);
}
