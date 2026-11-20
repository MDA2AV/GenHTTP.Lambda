using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Building;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// What the operator decides about the build agent, at <c>/admin/agent</c>
/// behind the admin token: whether it builds and changes apps here, and on
/// which models and efforts.
/// </summary>
/// <remarks>
/// The panel's Build agent section calls these. The password a model may be
/// behind is the installation's, an environment variable, so it is said
/// whether there is one and never what it is - nor is anything the runner
/// signs in with, of which the runner itself says only the kind.
/// </remarks>
[Tag(ApiDocumentation.BuildAgent)]
[Summary("Whether the build agent builds and changes apps on this installation, and on which models and efforts - for whoever runs it.")]
[Remarks("""
         The build agent behind `/build` and the editor's Change section runs on one of the models of Anthropic's the platform
         knows, at an effort that decides how much it thinks and how many steps it takes. The operator decides which models
         and efforts people may choose, which model needs the installation's password (`LAMBDA_AGENT_PASSWORD`) - a job on it
         runs without a clock and without a limit on its steps - and what a job runs on where nobody chose. `GET /system`
         (`build.offer`) says what the pages offer from it.
         """)]
public sealed class AgentSettingsResource(IBuildService builds, LambdaOptions options, ILogger<AgentSettingsResource> logger)
{

    [ResourceMethod]
    [Summary("Reads the settings of the build agent")]
    [Remarks("""
             What the operator decided, whether the installation has a password for the models behind one, and what the runner
             says of itself - asked as this is read, with a few seconds to answer: what its builds sign in with, which model of
             Anthropic's each model runs as, the version of Claude Code they run and its queue.
             """)]
    [return: Summary("The settings of the build agent, and how its runner is")]
    public async ValueTask<AgentSettingsResponse> Get()
        => Describe(builds.GetSettings(), await builds.RunnerAsync());

    [ResourceMethod(Method.Put)]
    [Summary("Changes the settings of the build agent")]
    [Remarks("""
             What is left out stays as it is. They apply to the next job asked for and the next page drawn; a job under way runs on
             as it was asked for. A model behind the password is offered only where the installation has a password.
             """)]
    [Response(ResponseStatus.BadRequest, """
                                         A model, an access or an effort there is none of; a default model that is not offered to everybody without
                                         the password; no effort offered, or a default effort that is not. Nothing was changed.
                                         """, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The settings, as they were saved")]
    public async ValueTask<AgentSettingsResponse> Change(AgentSettingsRequest body)
    {
        var saved = await builds.SaveSettingsAsync(body.Over(builds.GetSettings()));

        logger.LogInformation("Updated agent settings builds {Builds} changes {Changes} models {Models} model {Model} efforts {Efforts} effort {Effort} by operator",
                              saved.Builds, saved.Changes,
                              string.Join(',', saved.Models.Select(m => $"{AgentCatalog.NameOf(m.Model)}:{AgentCatalog.NameOf(m.Access)}")),
                              AgentCatalog.NameOf(saved.DefaultModel),
                              string.Join(',', saved.Efforts.Select(AgentCatalog.NameOf)),
                              AgentCatalog.NameOf(saved.DefaultEffort));

        // the runner is not asked again: what it says of itself did not change
        return Describe(saved, null);
    }

    private AgentSettingsResponse Describe(AgentSettings settings, AgentRunner? runner)
        => AgentSettingsResponse.Of(settings, builds.Offer(), builds.PasswordSet, builds.PerDay, options.AgentUrl, runner);

}
