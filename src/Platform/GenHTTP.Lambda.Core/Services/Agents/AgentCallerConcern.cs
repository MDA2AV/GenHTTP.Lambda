using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta.Model;

using GenHTTP.Modules.DependencyInjection;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Finds out, as a call comes in, whether it belongs to a run of the
/// installation's agent - by the credential the run's calls carry - and
/// keeps who it is on the request for whatever the call does.
/// </summary>
/// <remarks>
/// <para>
/// Put in front of every door an agent uses - the REST API, MCP, git - so
/// the agent is a client like any other, and what it does is recorded
/// whichever door it took (see <see cref="IAgentJournal"/>). A call without
/// a credential, or with one this installation did not issue, is nobody
/// known and goes on as it would.
/// </para>
/// <para>
/// Read here rather than where the call is handled: GenHTTP lets go of the
/// headers once a body is bound.
/// </para>
/// </remarks>
public sealed class AgentCallerConcern(IAgentService agents) : IDependentConcern
{
    private const string Key = "lambda.author";

    public async ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request)
    {
        if (request.Header.Headers.GetEntry(IAgentService.CredentialHeader) is { Length: > 0 } credential
            && await agents.AuthorOfAsync(credential) is { } author)
        {
            request.Properties[Key] = author;
        }

        return await content.HandleAsync(request);
    }

    /// <summary>
    /// Who a call is, as this concern found it: a run of the agent, or nobody known.
    /// </summary>
    public static VersionAuthor? Of(IRequest request) => request.Properties.TryGet<VersionAuthor>(Key, out var author) ? author : null;

}
