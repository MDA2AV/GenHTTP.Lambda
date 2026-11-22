using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// The switches of the build agent were here until its models could be
/// chosen; they are <c>/admin/agent</c> now, beside them. Every field may be
/// left out, so a client that knows of one setting changes nothing it does
/// not know of.
/// </remarks>
[Summary("What the operator changes about the whole installation: what is left out stays as it is.")]
public sealed record SettingsModel(
    [Summary("Whether the platform optimizes the databases itself - its own records and analytics, and every lambda's - each at most once a day, in its quiet hours (see `GET /admin/optimization`)")]
    bool? OptimizeDatabases = null,
    [Summary("Who the installation is for: one of `profiles` as `GET /admin/settings` lists them")]
    string? Profile = null
);
