using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Execution;

public sealed class TrailingSlashConcernBuilder(IDomainRegistry registry) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new TrailingSlashConcern(content, registry);

}
