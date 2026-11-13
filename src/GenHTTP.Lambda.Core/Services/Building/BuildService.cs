using System.Net;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Settings;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Hands a sentence somebody typed to the build agent, and reports back.
/// </summary>
/// <remarks>
/// The agent is a container of its own with no route off the machine and no
/// tools beyond this server's own MCP, which is the whole reason it is safe
/// to point a public text box at it.
///
/// Two kinds of job go through it. A build makes something new for whoever
/// typed into the box on /build. A change works on a lambda that exists, for
/// whoever holds its editor key, and is asked for from the control center.
/// They share the queue and the daily allowance, since both spend the same
/// subscription.
///
/// This class decides what may be asked for and in which order things are
/// checked; the rest is done by the units it orchestrates. <see cref="AgentClient"/>
/// is the only thing that talks to the agent and holds the shared secret,
/// <see cref="BuildAllowance"/> counts the jobs of each address,
/// <see cref="ModelGate"/> says who may have which model, and
/// <see cref="AgentInput"/> checks what the pages send.
///
/// Nothing about a change is kept here. The agent files it under the lambda
/// and is asked for it by lambda, so a change goes on being reported when
/// this server is redeployed halfway through it.
///
/// Off unless LambdaOptions.AgentUrl is set, so an installation without an
/// agent simply does not have the feature rather than having a broken one.
/// With an agent, the operator can still take either text box away in the
/// panel - builds and changes each have a switch of their own - and the page
/// is then left with how to connect an agent of one's own.
/// </remarks>
public sealed class BuildService(AgentClient agent, BuildAllowance allowance, ModelGate models, ISettingsService settings,
                                 ILogger<BuildService> logger) : IBuildService
{

    /// <summary>Whether this installation has an agent to build with.</summary>
    public bool Available => agent.Available;

    /// <summary>Whether the box on /build is offered: there is an agent, and the operator has not switched it off.</summary>
    public bool BuildsOffered()
        => Available && settings.Get().BuildBox;

    /// <summary>Whether the box in the Change section is offered: there is an agent, and the operator has not switched it off.</summary>
    public bool ChangesOffered()
        => Available && settings.Get().ChangeBox;

    /// <summary>How many builds one address is allowed in a day.</summary>
    public int PerDay => allowance.PerDay;

    /// <summary>Whether the second model is on offer at all.</summary>
    public bool HasSecondModel => models.HasSecondModel;

    #region Building

    /// <summary>
    /// Starts a build, if this caller has any left today.
    /// </summary>
    /// <param name="model">
    /// Which of the offered models to use. The second one needs the password.
    /// </param>
    /// <param name="password">The password for the second model, where one was asked for.</param>
    public async ValueTask<BuildStarted> StartAsync(string? prompt, string? model, string? password, IPAddress? caller)
    {
        agent.Require();

        if (!BuildsOffered())
        {
            throw new ProviderException(ResponseStatus.NotFound, "Nothing is built from here on this installation.");
        }

        var wanted = AgentInput.Prompt(prompt, "Say what you would like built.");

        var wantedModel = models.Choose(model, password);

        if (!allowance.Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is {PerDay} builds today, which is all this offers for now. "
              + "The editor is still there, and so is the MCP if you have an agent of your own.");
        }

        BuildStarted? started;

        try
        {
            started = await agent.StartBuildAsync(wanted, wantedModel);
        }
        catch (ProviderException)
        {
            // the agent did not take it, and what it did not take costs nothing;
            // one it took and answered for unreadably is running all the same
            allowance.Refund(caller);
            throw;
        }

        if (started == null)
        {
            throw new ProviderException(ResponseStatus.BadGateway, "The build agent answered with nothing.");
        }

        logger.LogInformation("Started build {Build}", started.Id);

        return started;
    }

    /// <summary>
    /// How a build is getting on.
    /// </summary>
    /// <remarks>
    /// Anybody holding the id of a build may ask, which is what the build page
    /// does. A change is not answered here even though the agent keeps it by
    /// the same kind of id: it belongs to whoever holds the editor key, and is
    /// asked for with that key, under the lambda.
    /// </remarks>
    public async ValueTask<BuildProgress> ProgressAsync(string id)
    {
        agent.Require();

        if (!Guid.TryParse(id, out _))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "That is not a build.");
        }

        return await agent.GetBuildAsync(id)
            ?? throw new ProviderException(ResponseStatus.NotFound, "There is no build by that name any more.");
    }

    #endregion

    #region Changing

    /// <summary>
    /// What the Change section of a lambda shows: whether it can ask for a
    /// change, how many it has left, and the change under way or the last one.
    /// </summary>
    /// <param name="lambda">The lambda, by the id it is filed under</param>
    public async ValueTask<AgentState> StateAsync(long lambda, IPAddress? caller)
    {
        if (!ChangesOffered())
        {
            return new AgentState(false, PerDay, 0, HasSecondModel, null);
        }

        return new AgentState(true, PerDay, allowance.Left(caller), HasSecondModel, await agent.GetChangeAsync(lambda));
    }

    /// <summary>
    /// Asks the agent to change a lambda, if this caller has any left today
    /// and no other change of it is under way.
    /// </summary>
    /// <param name="lambda">The lambda, by the id the agent files the change under</param>
    /// <param name="privateKey">The editor key, which the agent needs to change anything</param>
    /// <param name="before">The version online now, to offer putting it back afterwards</param>
    /// <param name="deploy">Whether to merge the change and put it online once it works</param>
    /// <param name="language">The language of the control center, for the agent to fall back on</param>
    /// <param name="feature">The feature to go on with, by its key; a new one when left out</param>
    public async ValueTask<AgentState> ChangeAsync(long lambda, string privateKey, int? before, string? prompt, bool deploy,
                                                   string? model, string? password, string? language, string? feature, IPAddress? caller)
    {
        agent.Require();

        if (!ChangesOffered())
        {
            throw new ProviderException(ResponseStatus.NotFound, "Changes are not asked for from here on this installation.");
        }

        var wanted = AgentInput.Prompt(prompt, "Say what should be different.");

        var wantedModel = models.Choose(model, password);

        // asked before anything is spent: a second change of the same lambda
        // is refused, and refusing it should not cost the owner one
        if (await agent.GetChangeAsync(lambda) is { State: "queued" or "running" })
        {
            throw new ProviderException(ResponseStatus.Conflict,
                "A change of this lambda is already under way. Wait for it, or stop it first.");
        }

        if (!allowance.Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is all {PerDay} builds and changes for today. It starts again at midnight UTC.");
        }

        ChangeProgress? job;

        try
        {
            job = await agent.StartChangeAsync(lambda, privateKey, before, wanted, deploy, wantedModel,
                                               AgentInput.Language(language), feature);
        }
        catch (ProviderException)
        {
            allowance.Refund(caller);
            throw;
        }

        if (job == null)
        {
            throw new ProviderException(ResponseStatus.BadGateway, "The agent answered with nothing.");
        }

        // the key is never logged: it is the only thing standing between
        // somebody reading this log and the ability to change the lambda
        logger.LogInformation("Started change {Change} of lambda #{LambdaId}", job.Id, lambda);

        return new AgentState(true, PerDay, allowance.Left(caller), HasSecondModel, job);
    }

    /// <summary>
    /// Stops the change of a lambda that is under way. Whatever it saved so
    /// far stays saved: in its feature, or as a version - which are only ever
    /// added.
    /// </summary>
    public async ValueTask<AgentState> StopAsync(long lambda, IPAddress? caller)
    {
        agent.Require();

        var current = await agent.GetChangeAsync(lambda);

        if (current is { State: "queued" or "running" })
        {
            current = await agent.StopChangeAsync(current.Id, lambda) ?? current;
        }

        return new AgentState(true, PerDay, allowance.Left(caller), HasSecondModel, current);
    }

    #endregion

}
