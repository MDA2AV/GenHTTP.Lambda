using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas the installation keeps online for people and agents to read.
/// </summary>
/// <remarks>
/// Only the listing lives here. A demo is read like any other lambda - its
/// editor key is announced with it - so its versions, files and logs are the
/// same endpoints below <c>/lambdas/{privateKey}/</c> that an agent will use
/// on its own lambda a minute later. Everything that would change it is
/// refused by its tier.
/// </remarks>
public sealed class DemoResource(IMetaService meta)
{

    /// <summary>
    /// Every demo, with what reading it teaches and the key to read it with.
    /// </summary>
    [ResourceMethod]
    public async ValueTask<IReadOnlyList<DemoResponse>> Get()
    {
        var demos = new List<DemoResponse>();

        foreach (var demo in DemoCatalog.All)
        {
            // read from the lambda rather than assumed: the demos are prepared
            // in the background after startup, so for a few seconds after a
            // restart they exist as code and not yet as something that answers
            var status = await meta.DescribeKeyAsync(demo.Key);

            demos.Add(new DemoResponse(
                demo.Id,
                demo.Name,
                demo.Description,
                demo.Shows,
                demo.ReadWhen,
                demo.Key,
                demo.Key,
                $"/lambda/{demo.Key}/",
                [.. DemoCatalog.FilesFor(demo).Select(f => f.Name)],
                status.Deployed
            ));
        }

        return demos;
    }

}
