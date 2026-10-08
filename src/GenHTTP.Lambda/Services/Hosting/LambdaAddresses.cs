using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// Where the lambdas answer, both ways round: the address of a lambda, and
/// the lambda a host names.
/// </summary>
/// <remarks>
/// Every lambda answers at a subdomain of one domain, named after its public
/// key - <c>quiz.genhttp.run</c> - so whatever it serves is a site of its own
/// rather than a folder of the platform's: a page of it cannot read what the
/// platform or another lambda keeps in a browser, and its paths start at the
/// root of its host.
///
/// The addresses are built from <see cref="LambdaOptions.HostingUrl"/>, its
/// scheme and port included, so a server on a laptop hands out
/// <c>http://quiz.localhost:8080/</c> and the installation
/// <c>https://quiz.genhttp.run/</c>. Which host a request was addressed to
/// is read here as well, so the two cannot disagree.
/// </remarks>
public sealed class LambdaAddresses : ILambdaAddresses
{
    private const string Key = "{key}";

    private readonly string _scheme;

    private readonly string _port;

    /// <summary>
    /// The hosting domain where it is a page of its own, and nothing where the
    /// platform answers there itself - on localhost, most of all.
    /// </summary>
    private readonly string? _home;

    private readonly string _www;

    /// <summary>
    /// The names the platform answers at, which are never a lambda's.
    /// </summary>
    private readonly HashSet<string> _platform;

    #region Get-/Setters

    public string Domain { get; }

    public string Authority => Domain + _port;

    public string Template { get; }

    #endregion

    #region Initialization

    public LambdaAddresses(LambdaOptions options)
    {
        var configured = options.HostingUrl ?? $"http://localhost:{options.Port}";

        // a name with subdomains: an address has none, and a single label
        // other than localhost resolves nowhere
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https") || url.AbsolutePath != "/"
            || url.HostNameType != UriHostNameType.Dns || DomainNames.Normalize(url.IdnHost) is not { } domain
            || (!domain.Contains('.') && domain != "localhost"))
        {
            throw new InvalidOperationException($"LAMBDA_HOSTING_URL is '{configured}', which is no address lambdas can answer below. "
                                                + "Set it to a scheme and a domain, such as https://genhttp.run.");
        }

        _scheme = url.Scheme;
        _port = url.IsDefaultPort ? string.Empty : $":{url.Port}";

        Domain = domain;
        Template = $"{_scheme}://{Key}.{Domain}{_port}/";

        _platform = [.. DomainNames.PlatformHosts(options)];

        _home = _platform.Contains(Domain) ? null : Domain;

        _www = "www." + Domain;
    }

    #endregion

    #region Functionality

    public string Of(string publicKey) => $"{_scheme}://{publicKey}.{Domain}{_port}/";

    public string Of(string publicKey, LambdaTier tier, string? domain)
        => Serves(tier, domain) ? $"https://{domain}/" : Of(publicKey);

    /// <summary>
    /// Whether a lambda of this tier with this domain answers at it.
    /// </summary>
    public static bool Serves(LambdaTier tier, string? domain) => domain != null && tier == LambdaTier.Premium;

    /// <summary>
    /// What a host below the hosting domain names, or nothing for a host that
    /// is not below it - one of the platform's, or a lambda's own domain.
    /// </summary>
    /// <param name="host">The host in its normalized form, see <see cref="DomainNames.Normalize"/></param>
    public LambdaHost? Resolve(string host)
    {
        // and where somebody typed it the way sites used to be found; no key
        // can be www, so it is nobody's lambda either
        if (_home != null && (host == _home || host == _www))
        {
            return new HostingHome(host);
        }

        // the platform's own names are its, even where one of them is below
        // the hosting domain - www, say
        if (host.Length <= Domain.Length + 1 || _platform.Contains(host))
        {
            return null;
        }

        if (host[^(Domain.Length + 1)] != '.' || !host.EndsWith(Domain, StringComparison.Ordinal))
        {
            return null;
        }

        // whatever is in front of the domain, which is a key only where it is
        // a single label a key could be - the locator asks
        return new HostedLambda(host, host[..^(Domain.Length + 1)]);
    }

    #endregion

}
