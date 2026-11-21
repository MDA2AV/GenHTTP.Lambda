namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// What the operator is shown about an integration.
/// </summary>
/// <param name="Kind">Which kind it is</param>
/// <param name="Name">What it is called, such as SQLite or PostgreSQL</param>
/// <param name="Details">Where it keeps what it keeps, as label and value - nothing that grants access</param>
public sealed record IntegrationInfo(IntegrationKind Kind, string Name, IReadOnlyList<IntegrationDetail> Details);
