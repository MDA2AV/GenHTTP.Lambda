using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// How many lambdas use each of the platform's features.
/// </summary>
[Summary("How many lambdas use each of the platform's features, the demos left out.")]
public sealed record CensusUses(
    [Summary("How many have their database switched on")]
    int Database,
    [Summary("How many have their workspace switched on")]
    int Workspace,
    [Summary("How many have their secrets switched on")]
    int Secrets,
    [Summary("How many have their visits counted")]
    int Analytics,
    [Summary("How many are in the showcase")]
    int Showcase,
    [Summary("How many have their code published")]
    int Source,
    [Summary("How many have a domain of their own configured")]
    int Domain,
    [Summary("How many have a feature (a draft) open")]
    int Drafts,
    [Summary("How many may connect to other servers, by their tier's default or the operator's decision")]
    int Connects,
    [Summary("How many ask to, and wait for a decision")]
    int Requested
);
