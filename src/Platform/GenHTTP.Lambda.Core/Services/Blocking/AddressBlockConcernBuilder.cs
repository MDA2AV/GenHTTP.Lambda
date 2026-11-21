using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.Blocking;

public sealed class AddressBlockConcernBuilder(IAddressBlocks blocks, IDomainRegistry registry) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AddressBlockConcern(content, blocks, registry);
}
