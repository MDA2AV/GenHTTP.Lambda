namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// The kinds of integration an installation has one of each.
/// </summary>
public enum IntegrationKind
{

    /// <summary>
    /// The relational database: the platform's own records, its analytics and
    /// the database of every lambda that keeps one.
    /// </summary>
    Database,

    /// <summary>
    /// The files a lambda writes and reads as data - its workspace - and the
    /// platform's pictures.
    /// </summary>
    Files,

    /// <summary>
    /// The API keys and passwords a lambda reads and nobody sees.
    /// </summary>
    Secrets

}
