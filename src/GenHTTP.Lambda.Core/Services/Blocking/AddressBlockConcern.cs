using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Hosting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Services.Blocking;

/// <summary>
/// Refuses a request from a blocked address before anything else is done for
/// it - unless it is for the administration panel.
/// </summary>
/// <remarks>
/// The outermost concern of the host, outside even the redirects, the scope
/// and the request log: a blocked address is answered with 403 and costs
/// nothing more, the platform's pages and every lambda alike. Its requests
/// are counted against its block (see <see cref="AddressBlocks"/>) rather than
/// logged, so somebody hammering the server from one does not fill the log.
///
/// The panel stays reachable from a blocked address: its page, the assets it
/// is built from, what it asks the platform about itself, and the API behind
/// the admin token - so an operator who blocked their own address by mistake
/// can lift the block again. Nothing else is let through for it, and the API
/// asks for the token as it always does.
/// </remarks>
public sealed class AddressBlockConcern(IHandler content, IAddressBlocks blocks, IDomainRegistry registry) : IConcern
{

    /// <summary>
    /// What the panel is made of, on the platform's own host: its bundle, below
    /// its own path (see PlatformRoutes), and what of the API it calls.
    /// </summary>
    private static readonly string[] Panel = ["/" + PlatformRoutes.Admin, "/api/v1/admin", "/api/v1/telemetry", "/api/v1/logs", "/api/v1/system"];

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var address = request.Client.Address;

        if (!blocks.Covers(address) || ForThePanel(request))
        {
            return content.HandleAsync(request);
        }

        blocks.Refused(address!);

        var response = request.Respond()
                              .Status(ResponseStatus.Forbidden)
                              .Header("Cache-Control", "no-store")
                              .Content("Requests from this address are not accepted at the moment.", ContentType.TextPlain)
                              .Build();

        return new(response);
    }

    /// <summary>
    /// Whether the request is for the administration panel, on the platform's own host.
    /// </summary>
    /// <remarks>
    /// Only asked for an address that is blocked. The panel's paths below a
    /// lambda's host are that lambda's own, and refused like the rest.
    /// </remarks>
    private bool ForThePanel(IRequest request)
    {
        if (request.ResolveHost(registry) != null)
        {
            return false;
        }

        var path = request.Header.Path.ToString();

        foreach (var prefix in Panel)
        {
            if (path.StartsWith(prefix, StringComparison.Ordinal)
             && (prefix.EndsWith('/') || path.Length == prefix.Length || path[prefix.Length] is '/' or '?'))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

}

public sealed class AddressBlockConcernBuilder(IAddressBlocks blocks, IDomainRegistry registry) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new AddressBlockConcern(content, blocks, registry);
}
