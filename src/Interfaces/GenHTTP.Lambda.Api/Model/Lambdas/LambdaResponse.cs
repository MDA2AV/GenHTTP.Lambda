using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A lambda, as whoever holds its editor key sees it.")]
public sealed record LambdaResponse(
    [Summary("Names its public address. Not a secret; its owner may change it.")]
    string PublicKey,
    [Summary("The editor key, which authorizes every call about the lambda. Treat it like a password.")]
    string PrivateKey,
    [Summary("`Sandbox`, `Production`, `System` or `Demo`, assigned by the operator - what the lambda is for. Decides its limits, whether it is kept online while nobody uses it, and whether it may answer at a domain of its own; `tierPolicy` says how, on this installation.")]
    string Tier,
    [Summary("What its tier lets it have and how it is kept, as this installation set it, with what the installation calls the tier and the one above")]
    TierResponse TierPolicy,
    [Summary("When it was created")]
    DateTime Created,
    [Summary("When something about it last changed: a save, a deployment, a setting")]
    DateTime Modified,
    [Summary("The version that is online; absent while nothing is")]
    int? ActiveVersion,
    [Summary("The newest version saved, which may be ahead of the one online")]
    int? LatestVersion,
    [Summary("Its public address, named after its public key: `https://quiz.genhttp.run/`, or `https://lambda.example.com/lambda/quiz/` where the lambdas answer by path. While it answers at a domain of its own, this address sends its visitors on to the domain.")]
    string PublicUrl,
    [Summary("Where its editor is on this site: `/editor/{privateKey}`. Holds the editor key.")]
    string EditorPath,
    [Summary("When the version online went online; absent while nothing is")]
    DateTime? DeployedAt,
    [Summary("When it is taken offline unless it is visited or edited before then - each visit and each edit moves this on. Absent while it is offline, and for a tier that keeps it online.")]
    DateTime? DeployedUntil,
    [Summary("When it is deleted unless it is visited or edited before then. Absent for a tier that keeps it.")]
    DateTime? KeptUntil,
    [Summary("The domain of its own it is configured to answer at, whether or not its tier lets it")]
    string? Domain,
    [Summary("Whether it answers at that domain: it has one, and its tier includes it")]
    bool DomainServed,
    [Summary("Where to link to it: its domain while that is served, `publicUrl` otherwise")]
    string Address,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Full` or `Simple`")]
    string View,
    [Summary("Where it is cloned with git and pushed to, on this site. The versions are the commits of `main`, the features the other branches. Holds the editor key, like the editor's path.")]
    string GitPath,
    [Summary("Since when the operator has blocked it; absent while it is not. A blocked lambda is offline, and every change to it is refused with `Blocked` (403) until the operator lifts the block. It can still be read and exported.")]
    DateTime? Blocked = null,
    [Summary("Why the operator blocked it, in their words, as the editor shows it to the owner. Absent while it is not blocked, and when they gave no reason.")]
    string? BlockReason = null,
    [Summary("Whether its code may connect to other servers, and where the request for that stands. Without it, code that does is refused as it compiles; see `PUT /lambdas/{privateKey}/connectivity`.")]
    ConnectivityResponse? Connectivity = null
);
