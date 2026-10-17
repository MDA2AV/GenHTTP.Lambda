using System.Reflection;

using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.OpenApi.Handler;

using NSwag;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// The OpenAPI document of the API, as whoever calls it reads it - in Scalar
/// at <c>/api/v1/scalar/</c>, or as <c>/api/v1/openapi.json</c>.
/// </summary>
/// <remarks>
/// The operations, their parameters, responses and models document
/// themselves with the framework's documentation attributes, beside the code
/// they describe. What belongs to no single operation is here: the title, the
/// introduction everybody reads first (<c>Resources/Api/Introduction.md</c>),
/// and the tags in the order and the groups a reader goes through them.
///
/// A tag names a concept somebody calls the API about, never the class that
/// answers it - several resources share one where they serve the same thing
/// (the showcase, the open sources, the workspace). Every tag in use is in a
/// group, which <c>ApiDocumentationTests</c> checks.
/// </remarks>
public static class ApiDocumentation
{

    #region Tags

    public const string Platform = "Platform";

    public const string Keys = "Keys";

    public const string Demos = "Demos";

    public const string Lambdas = "Lambdas";

    public const string Versions = "Versions";

    public const string Deployment = "Deployment";

    public const string Features = "Features";

    public const string Code = "Code analysis";

    public const string Data = "Data";

    public const string Database = "Database";

    public const string Workspace = "Workspace";

    public const string Secrets = "Secrets";

    public const string Monitoring = "Monitoring";

    public const string Analytics = "Analytics";

    public const string Domain = "Domain";

    public const string Showcase = "Showcase";

    public const string OpenSource = "Open source";

    public const string Builds = "Builds";

    public const string Changes = "Changes";

    public const string Administration = "Administration";

    public const string Telemetry = "Telemetry";

    public const string ServerLog = "Server log";

    /// <summary>
    /// The tags in the order they are read, in the groups Scalar shows them in.
    /// </summary>
    internal static readonly IReadOnlyList<(string Group, string[] Tags)> Groups =
    [
        ("Getting started", [Platform, Keys, Demos, Lambdas]),
        ("The program", [Versions, Deployment, Features, Code]),
        ("What it keeps", [Data, Database, Workspace, Secrets]),
        ("How it is doing", [Monitoring, Analytics]),
        ("Sharing it", [Domain, Showcase, OpenSource]),
        ("The build agent", [Builds, Changes]),
        ("Operating the installation", [Administration, Telemetry, ServerLog])
    ];

    #endregion

    #region Shared descriptions

    // said by many operations, so they are said the same way by all of them

    public const string Json = "application/json";

    public const string Zip = "application/zip";

    public const string PrivateKey = "The editor key of the lambda. Holding it is what allows the call.";

    public const string FeatureKey = "The key of the feature, as `key` in the answer that started it";

    public const string VersionNumber = "The number of the version, counting from 1";

    public const string WorkspacePath = "The path within the workspace, sent as one segment with its slashes encoded: `logs%2Ftoday.txt`";

    public const string Deploy = "Whether to put the new version online as well. The version is saved either way; whether it went online is in `deployment`.";

    public const string Layout = "How the files are laid out: `lambda` (the default) names them as the lambda does, `project` as a clone of its git repository has them - "
                               + "the snippet as `Project.cs`, the other C# files named the .NET way, the platform's own files left out";

    public const string Specification = "What the user wants from this version and why, in their words where possible. Kept with the version, cut past 4000 characters.";

    public const string Change = "What this version changes, in one line for the owner. Kept with the version, cut past 500 characters.";

    public const string NoLambda = "No lambda has this editor key. A key that never existed and the key of a deleted lambda are answered alike.";

    public const string NoFeature = "No lambda has this editor key, or the lambda has no feature with this key: it never existed, or it was merged or deleted.";

    public const string Demo = "The lambda is a demo, which anybody may read and nobody may change. Create a lambda of your own from it instead: "
                             + "`POST /lambdas` with the demo's id as `template`.";

    public const string Diagnostics = "The code does not compile. `diagnostics` holds what the compiler said, with the file, line and column of each problem; "
                                    + "nothing was put online, and whatever was online before still is.";

    public const string Deleted = "Done. There is no body.";

    #endregion

    #region Functionality

    /// <summary>
    /// The concern that serves the OpenAPI document of what it is added to.
    /// </summary>
    public static OpenApiConcernBuilder Create()
        => ApiDescription.Create()
                         .Title("GenHTTP Lambda API")
                         .Version("v1")
                         .PostProcessor((_, document) => Complete(document));

    private static void Complete(OpenApiDocument document)
    {
        document.Info.Description = Introduction.Value;

        // the tags in reading order, then whatever no group names - which the
        // tests refuse, but a reader should still find it rather than lose it
        var order = Groups.SelectMany(g => g.Tags).ToList();

        var sorted = document.Tags.OrderBy(t => order.IndexOf(t.Name) is var index and >= 0 ? index : int.MaxValue).ToList();

        document.Tags.Clear();

        foreach (var tag in sorted)
        {
            document.Tags.Add(tag);
        }

        document.ExtensionData ??= new Dictionary<string, object?>();

        document.ExtensionData["x-tagGroups"] = Groups.Select(g => new Dictionary<string, object>
        {
            ["name"] = g.Group,
            ["tags"] = g.Tags
        }).ToList();
    }

    private static readonly Lazy<string> Introduction = new(() =>
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GenHTTP.Lambda.Resources.Api.Introduction.md")
                        ?? throw new InvalidOperationException("The introduction of the API is missing from the assembly.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd().ReplaceLineEndings("\n");
    });

    #endregion

}
