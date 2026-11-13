namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Where the components of the platform are found on its host, and the
/// addresses of what they serve.
/// </summary>
/// <remarks>
/// A host mounts each component at the name given here, and nowhere else: the
/// components link to one another - the API hands out the editor's address,
/// the editor links to the published source, a clone keeps the git address it
/// was made from - so a component mounted elsewhere would be linked to where
/// it is not. They are fixed rather than configured for the same reason the
/// addresses a lambda answers at are kept when it changes: whoever saved one
/// still has it.
///
/// A lambda's own address comes from <see cref="ILambdaAddresses"/>, not from
/// here: where it answers depends on the installation.
/// </remarks>
public static class PlatformRoutes
{

    /// <summary>The REST API, its OpenAPI document and Scalar.</summary>
    public const string Api = "api";

    /// <summary>The MCP endpoint, for agents.</summary>
    public const string Mcp = "mcp";

    /// <summary>The editor, the page creating a lambda and the owner's git repositories.</summary>
    public const string Editor = "editor";

    /// <summary>The operator's panel.</summary>
    public const string Admin = "admin";

    /// <summary>The published sources and their public git repositories.</summary>
    public const string Source = "source";

    /// <summary>The previews of features, served like the lambdas they belong to.</summary>
    public const string Features = "features";

    /// <summary>The lambdas below the platform's own host, where they answer by path.</summary>
    public const string Lambdas = "lambda";

    /// <summary>
    /// The base of the REST API, the version included: <c>/api/v1</c>.
    /// </summary>
    public const string ApiBase = "/" + Api + "/v1";

    /// <summary>
    /// Where the lambdas answer by path, both slashes included: <c>/lambda/</c>.
    /// </summary>
    public const string LambdaBase = "/" + Lambdas + "/";

    /// <summary>
    /// The editor of a lambda, for whoever holds its editor key.
    /// </summary>
    public static string EditorOf(string privateKey) => $"/{Editor}/{privateKey}";

    /// <summary>
    /// Where whoever holds the editor key clones the lambda: the editor's
    /// path, with a name for the folder a clone makes.
    /// </summary>
    public static string RepositoryOf(string privateKey, string publicKey) => $"/{Editor}/{privateKey}/{publicKey}.git";

    /// <summary>
    /// The page of a published source, in no language - the pages send it on
    /// to the visitor's.
    /// </summary>
    public static string SourceOf(string publicKey) => $"/{Source}/{publicKey}";

    /// <summary>
    /// Where anybody clones a published source.
    /// </summary>
    public static string PublishedRepositoryOf(string publicKey) => $"/{Source}/{publicKey}.git";

    /// <summary>
    /// Where the preview of a feature answers.
    /// </summary>
    public static string PreviewOf(string featureKey) => $"/{Features}/{featureKey}/";

}
