using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Web;

public sealed class HeadersConcernBuilder(IReadOnlyList<(string Name, string Value)> headers) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new HeadersConcern(content, headers);

}
