using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Execution;

public sealed class TrailingSlashConcernBuilder(IDomainRegistry registry) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new TrailingSlashConcern(content, registry);

}
