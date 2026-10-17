using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Only the listing lives here. A demo is read like any other lambda - its
/// editor key is announced with it - so its versions, files and logs are the
/// same endpoints below <c>/lambdas/{privateKey}/</c> that an agent will use
/// on its own lambda a minute later. Everything that would change it is
/// refused by its tier.
/// </remarks>
[Tag(ApiDocumentation.Demos)]
[Summary("Finished lambdas the installation keeps online, to be read and started from.")]
[Remarks("""
         Each demo shows how something is done the GenHTTP way - a REST API with a database, sign-up and log-in, a multiplayer
         game over websockets, uploads, live updates - with its documentation and tests. Its editor key is public, so it is
         read with the same calls as any lambda (`GET /lambdas/{privateKey}/versions/1`, its logs, its database), and every
         change is refused with `403`. A lambda of your own starts as a copy of one with `POST /lambdas` and its `id` as
         `template`.
         """)]
public sealed class DemoResource(IMetaService meta, ILambdaAddresses addresses)
{

    [ResourceMethod]
    [Summary("Lists the demos")]
    [Remarks("Public. A few seconds after the server started, a demo may not be online yet (`live`).")]
    [return: Summary("Every demo")]
    public IReadOnlyList<DemoResponse> Get()
    {
        var demos = new List<DemoResponse>();

        foreach (var demo in DemoCatalog.All)
        {
            // read from the lambda rather than assumed: the demos are prepared
            // in the background after startup, so for a few seconds after a
            // restart they exist as code and not yet as something that answers
            var status = meta.DescribeKey(demo.Key);

            demos.Add(new DemoResponse(
                demo.Id,
                demo.Name,
                demo.Description,
                demo.Shows,
                demo.ReadWhen,
                demo.Key,
                demo.Key,
                addresses.Of(demo.Key),
                [.. DemoCatalog.FilesFor(demo).Select(f => f.Name)],
                status.Deployed
            ));
        }

        return demos;
    }

}
