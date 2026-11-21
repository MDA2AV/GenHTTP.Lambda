using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.SinglePageApplications;

namespace GenHTTP.Lambda.Web;

public sealed class HeadersConcernBuilder(IReadOnlyList<(string Name, string Value)> headers) : IConcernBuilder
{

    public IConcern Build(IHandler content) => new HeadersConcern(content, headers);

}
