using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the operator decides about the agent, at <c>/admin/agent</c> behind
/// the admin token: whether it builds and changes apps here, which values of
/// its options are offered and to whom, and what a run may take - with what
/// the agent is, its runs and how it was used.
/// </summary>
/// <remarks>
/// The panel's Build agent section calls these. The password a value may be
/// behind is the installation's, an environment variable, so it is said
/// whether there is one and never what it is - nor is anything the agent
/// signs in with, of which its system says only the kind.
/// </remarks>
[Tag(ApiDocumentation.BuildAgent)]
[Summary("Whether the agent builds and changes apps on this installation, on which of its options and for how long - for whoever runs it.")]
[Remarks("""
         The agent behind `/build`, the workspace and the editor's Change section is the installation's agent integration, which
         names its options - such as the model it runs on and its effort - and says what it is, where it sends what, and what it
         does: files a message may come with, files it hands back, the web, questions back. The operator decides which values
         people may choose, which need the installation's password (`LAMBDA_AGENT_PASSWORD`), what a run takes where nobody chose,
         how long a run may work and how many steps it may take - for runs behind the password apart - and whether the agent
         builds and changes apps at all. `GET /system` (`build.offer`) says what the pages offer from it.
         """)]
public sealed class AgentSettingsResource(IAgentService agents, LambdaOptions options, ILogger<AgentSettingsResource> logger)
{

    [ResourceMethod]
    [Summary("Reads the settings of the agent")]
    [Remarks("""
             What the operator decided, what the agent is and offers, whether the installation has a password for the values behind
             one, and what the agent system says of itself - asked as this is read, with a few seconds to answer: whether it answers,
             how many of this installation's runs it works on and how many wait, and facts such as what it signs in with.
             """)]
    [return: Summary("The settings of the agent, and how its system is")]
    public async ValueTask<AgentSettingsResponse> Get()
        => AgentSettingsResponse.Of(agents, options.AgentUrl, await agents.SystemAsync());

    [ResourceMethod(Method.Put)]
    [Summary("Changes the settings of the agent")]
    [Remarks("""
             What is left out stays as it is. They apply to the next run asked for and the next page drawn; a run under way goes on
             as it was asked for. A value behind the password is offered only where the installation has a password.
             """)]
    [Response(ResponseStatus.BadRequest, """
                                         An option, a value, an access or a limit there is none of, or a default that is not offered to everybody
                                         without the password. Nothing was changed.
                                         """, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The settings, as they were saved")]
    public async ValueTask<AgentSettingsResponse> Change(AgentSettingsRequest body)
    {
        var saved = await agents.ChangeSettingsAsync(body.Change(agents.Capabilities));

        logger.AgentSettingsChanged(saved);

        // the agent system is not asked again: what it says of itself did not change
        return AgentSettingsResponse.Of(agents, options.AgentUrl, null);
    }

    [ResourceMethod("runs")]
    [Summary("Lists the newest runs of the agent")]
    [Remarks("""
             The newest first, while the platform keeps them - a while after they ended: what was asked, on which options, for how
             long and how it ended, the files it came with and handed back, and what it spent where the agent says. For looking into
             a run that went wrong; the prompts are in the log as well. `GET /attachments/{id}` answers with a file.
             """)]
    [return: Summary("The runs")]
    public async ValueTask<IReadOnlyList<RunListing>> Runs([Summary("How many, at most 100")] int? take = null)
        => await agents.NewestAsync(Math.Clamp(take ?? 20, 1, 100));

    [ResourceMethod("usage")]
    [Summary("Reads how the agent was used")]
    [Remarks("How many runs there were over the days asked for, today included, and how long they worked - in all, and by each value of each option. Never what they were about.")]
    [return: Summary("The runs and their time, by option")]
    public async ValueTask<AgentUsageSummary> Usage([Summary("Over how many days, today included: 1 to 30, 30 when left out")] int? days = null)
        => await agents.UsageAsync(Math.Clamp(days ?? 30, 1, 30));

}
