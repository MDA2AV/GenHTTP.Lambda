namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// The kinds of integration an installation has one of each.
/// </summary>
public static class IntegrationKinds
{

    /// <summary>
    /// The relational database: the platform's own records, its analytics and
    /// the database of every lambda that keeps one.
    /// </summary>
    public const string Database = "database";

    /// <summary>
    /// The files a lambda writes and reads as data - its workspace - and the
    /// platform's pictures.
    /// </summary>
    public const string Files = "files";

    /// <summary>
    /// The API keys and passwords a lambda reads and nobody sees.
    /// </summary>
    public const string Secrets = "secrets";

}

/// <summary>
/// What the operator is shown about an integration.
/// </summary>
/// <param name="Kind">Which kind it is, one of <see cref="IntegrationKinds"/></param>
/// <param name="Name">What it is called, such as SQLite or PostgreSQL</param>
/// <param name="Details">Where it keeps what it keeps, as label and value - nothing that grants access</param>
public sealed record IntegrationInfo(string Kind, string Name, IReadOnlyList<IntegrationDetail> Details);

/// <summary>
/// One fact about an integration, such as the server it talks to.
/// </summary>
public sealed record IntegrationDetail(string Label, string Value);
