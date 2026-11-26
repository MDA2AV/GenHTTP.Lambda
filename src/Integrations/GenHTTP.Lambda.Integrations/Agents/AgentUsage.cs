namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// What a run spent, where the service behind the agent says.
/// </summary>
/// <param name="InputTokens">The tokens it read, cached ones included</param>
/// <param name="OutputTokens">The tokens it wrote</param>
/// <param name="Cost">What it cost, where the service says</param>
/// <param name="Currency">What the cost is in, as an ISO 4217 code such as <c>USD</c></param>
public sealed record AgentUsage(long? InputTokens = null, long? OutputTokens = null, decimal? Cost = null, string? Currency = null);
