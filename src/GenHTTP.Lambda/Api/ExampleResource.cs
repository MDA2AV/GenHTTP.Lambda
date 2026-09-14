using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The lambdas the installation keeps online for people to look at.
/// </summary>
/// <remarks>
/// Public, and read only in the strongest sense available here: what makes a
/// lambda editable is holding its editor key, and this never hands one out.
/// There is no write method to guard because there is nothing to write to -
/// somebody who wants to change an example clones it, which creates a lambda
/// of their own from the same template.
/// </remarks>
public sealed class ExampleResource(IMetaService meta)
{

    /// <summary>
    /// Every example, in the groups the editor offers.
    /// </summary>
    [ResourceMethod]
    public async ValueTask<ExampleListingResponse> Get()
    {
        var groups = new List<ExampleGroupResponse>();

        foreach (var group in ExampleCatalog.All.Where(e => !e.Hidden).GroupBy(e => e.GroupId))
        {
            var examples = new List<ExampleSummaryResponse>();

            foreach (var example in group)
            {
                var described = await DescribeAsync(example);

                examples.Add(new ExampleSummaryResponse(
                    described.Id,
                    described.Name,
                    described.Description,
                    described.PublicKey,
                    described.Path,
                    described.TryPath,
                    described.Socket,
                    described.Live
                ));
            }

            groups.Add(new ExampleGroupResponse(group.Key, group.First().GroupName, examples));
        }

        return new ExampleListingResponse(groups);
    }

    /// <summary>
    /// One example, with the code it is running.
    /// </summary>
    [ResourceMethod(":id")]
    public async ValueTask<ExampleResponse> GetOne(string id)
    {
        var example = ExampleCatalog.Find(id)
                   ?? throw LambdaException.NotFound($"There is no example called '{id}'.");

        return await DescribeAsync(example);
    }

    /// <summary>
    /// Whether the example is actually answering at the moment.
    /// </summary>
    /// <remarks>
    /// Read from the lambda rather than assumed: the examples are prepared in
    /// the background after startup, so for a few seconds after a restart they
    /// exist as code and not yet as something that answers.
    /// </remarks>
    private async ValueTask<ExampleResponse> DescribeAsync(LambdaExample example)
    {
        var status = await meta.GetStatusAsync(example.PublicKey);

        var files = TemplateCatalog.FilesFor(example.Id, example.PublicKey);

        return new ExampleResponse(
            example.Id,
            example.Name,
            example.Description,
            example.PublicKey,
            $"/lambda/{example.PublicKey}/",
            $"/lambda/{example.PublicKey}/{example.TryPath}",
            example.Socket,
            files[0].Code,
            files,
            status.Deployed
        );
    }

}
