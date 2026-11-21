using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Hosting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Blocking;

public sealed class AddressBlockConcernBuilder(IAddressBlocks blocks, IDomainRegistry registry) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AddressBlockConcern(content, blocks, registry);
}
