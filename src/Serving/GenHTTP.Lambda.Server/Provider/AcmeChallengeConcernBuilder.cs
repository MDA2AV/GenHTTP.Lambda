using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Server;

public sealed class AcmeChallengeConcernBuilder(string root) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AcmeChallengeConcern(content, root);
}
