namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// One fact about an integration, such as the server it talks to.
/// </summary>
public sealed record IntegrationDetail(string Label, string Value);
