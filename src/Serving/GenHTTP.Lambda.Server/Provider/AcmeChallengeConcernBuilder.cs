using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Server;

public sealed class AcmeChallengeConcernBuilder(string root) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AcmeChallengeConcern(content, root);
}
