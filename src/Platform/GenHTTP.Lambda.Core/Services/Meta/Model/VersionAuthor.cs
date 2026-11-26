using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Who wrote a version or what a draft holds, or changed something of a
/// lambda - and on which options of the agent.
/// </summary>
/// <remarks>
/// Known for a run of the installation's agent only, from the credential its
/// calls carry - never from what the agent says about itself. The options are
/// the values the run was asked for on, by the integration's names
/// (<c>model=opus</c>), not what the agent system ran them as, which may
/// change with it. An agent of somebody's own has no author.
/// </remarks>
/// <param name="Writer">Who wrote it</param>
/// <param name="Options">The value of each option the run ran on, by option</param>
/// <param name="Run">The run, where it is one the platform keeps - nothing for a job of the build agent from before runs were kept</param>
public sealed record VersionAuthor(VersionWriter Writer, IReadOnlyDictionary<string, string> Options, string? Run = null);
