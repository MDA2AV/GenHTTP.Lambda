using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Puts together what a run hands the agent: the message, the conversation
/// before it, the files, whom it works for and how it reaches the platform.
/// </summary>
/// <remarks>
/// The agent keeps nothing between runs, so everything it needs is in here,
/// from the platform's own records: the conversation is the app's earlier
/// runs as the platform keeps them - the page's own only where it keeps
/// none - and the files are those of this message and of the earlier ones,
/// so "the logo from before" can be found.
/// </remarks>
public sealed class AgentContext(AgentRunRecords records, AgentAttachments attachments, AgentOutcome outcome, ISettingsService settings,
                                 IBrandingService branding, LambdaOptions options)
{

    /// <summary>
    /// The run as the agent is handed it.
    /// </summary>
    /// <param name="credential">The run's credential, which its calls carry</param>
    /// <param name="lambda">The app it works on; nothing for a build</param>
    /// <param name="history">The conversation the page sent, taken where the platform keeps no earlier run of the app</param>
    public async ValueTask<AgentRun> MakeAsync(AgentRunEntity run, string credential, AgentLambda? lambda, IReadOnlyList<ConversationTurn>? history,
                                               AgentRunEntity? previous, CancellationToken cancellation = default)
    {
        var conversation = run.LambdaId is { } id ? await ConversationOfAsync(id, run.Id, cancellation) : [];

        if (conversation.Count == 0)
        {
            conversation = [.. (AgentInput.History(history) ?? []).Select(t => new AgentTurn(t.Prompt ?? "", t.Reply))];
        }

        var files = await attachments.OfferedAsync(run.Id, run.LambdaId, await CarriedAsync(previous, cancellation), cancellation);

        var audience = new AgentAudience(run.Language ?? "en",
                                         run.View == EditorView.Simple ? AgentView.Simple : AgentView.Full,
                                         settings.Get().Profile switch
                                         {
                                             InstallationProfile.Cloud => AgentInstallationKind.Public,
                                             InstallationProfile.Enterprise => AgentInstallationKind.Enterprise,
                                             _ => AgentInstallationKind.Home
                                         },
                                         branding.Current.Name);

        var home = options.PublicUrl?.TrimEnd('/') ?? "";

        var access = new AgentAccess($"{home}/{PlatformRoutes.Mcp}", $"{home}{PlatformRoutes.ApiBase}", IAgentService.CredentialHeader, credential);

        var limits = new AgentLimits(run.TimeLimit is { } seconds ? TimeSpan.FromSeconds(seconds) : null, run.StepLimit);

        return new AgentRun(run.Id, run.Kind == AgentRunKind.Build ? AgentTask.Build : AgentTask.Change, run.Prompt, files, conversation, audience, access,
                            lambda, StoredOptions.Read(run.Options), limits, await ContinuationOfAsync(run, previous, cancellation));
    }

    /// <summary>
    /// The earlier runs of an app as turns of a conversation, oldest first,
    /// cut as the page's own would be.
    /// </summary>
    private async ValueTask<IReadOnlyList<AgentTurn>> ConversationOfAsync(long lambda, string run, CancellationToken cancellation)
    {
        var earlier = (await records.OfLambdaAsync(lambda, AgentInput.MostTurns + 1, cancellation)).Where(r => r.Id != run).ToList();

        if (earlier.Count == 0)
        {
            return [];
        }

        var files = await records.FilesOfAsync([.. earlier.Select(r => r.Id)], cancellation);

        var turns = AgentInput.Cut([.. earlier.AsEnumerable().Reverse()], r => r.Prompt, r => r.Message);

        // the files each message came with, by name, so a later one can say
        // which it means
        return [.. turns.Select(t => new AgentTurn(t.Prompt, t.Reply,
                    NamesOf(files.Where(f => f.Direction == AgentFileDirection.Attached && f.RunId == t.Turn.Id))))];
    }

    /// <summary>
    /// The runs a run carries on from, newest first - a few, as a
    /// conversation that went back and forth does not go back further.
    /// </summary>
    private async ValueTask<IReadOnlyList<string>> CarriedAsync(AgentRunEntity? previous, CancellationToken cancellation)
    {
        var carried = new List<string>();

        for (var run = previous; run != null && carried.Count < AgentInput.MostTurns; run = run.Continues is { } earlier ? await records.GetAsync(earlier, cancellation) : null)
        {
            carried.Add(run.Id);
        }

        return carried;
    }

    private static IReadOnlyList<string>? NamesOf(IEnumerable<AgentFileEntity> files)
    {
        var names = files.Select(f => f.Name).ToList();

        return names.Count > 0 ? names : null;
    }

    /// <summary>
    /// What a run carries on from, as the agent is told it - the earlier run
    /// asked, the question it ended with, and what the platform recorded of it.
    /// </summary>
    private async ValueTask<AgentContinuation?> ContinuationOfAsync(AgentRunEntity run, AgentRunEntity? previous, CancellationToken cancellation)
    {
        if (previous == null || run.Continuation is not { } kind)
        {
            return null;
        }

        var deeds = AgentOutcome.DeedsOf(await outcome.ReadAsync(previous, cancellation));

        return new AgentContinuation(kind, previous.Id, previous.Prompt, previous.Ending == AgentEnding.Asked ? previous.Message : null, FactsOf(deeds));
    }

    /// <summary>
    /// What a run did, as the records say it, in plain English for the agent
    /// that carries on - or nothing, where it did nothing the records know.
    /// </summary>
    private static string? FactsOf(AgentOutcome.Deeds deeds)
    {
        var facts = new List<string>();

        if (deeds.Feature != null)
        {
            facts.Add($"it worked in the draft '{deeds.FeatureName ?? deeds.Feature}' (feature {deeds.Feature})");
        }

        if (deeds.Saved is { } saved)
        {
            facts.Add($"it saved version {saved}");
        }

        if (deeds.Online is { } online)
        {
            facts.Add($"it put version {online} online");
        }

        if (deeds.Settings.Count > 0)
        {
            facts.Add($"it changed these settings: {string.Join(", ", deeds.Settings)}");
        }

        if (!deeds.Wrote && facts.Count == 0)
        {
            return "it saved nothing.";
        }

        return string.Join("; ", facts) + ".";
    }

}
