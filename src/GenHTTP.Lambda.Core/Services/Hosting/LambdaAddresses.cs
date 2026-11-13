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
///
/// An installation can have its lambdas answer below <c>/lambda/</c> on its
/// own host instead (<see cref="LambdaAddressing.Paths"/>), built from its
/// public address: <c>http://localhost:8080/lambda/quiz/</c>. No host is a
/// lambda's then but a domain of its own.
/// </remarks>
public sealed class LambdaAddresses : ILambdaAddresses
{
    private const string Key = "{key}";

    /// <summary>
    /// Where a lambda's address starts, up to its key: <c>https://</c>, or
    /// <c>http://localhost:8080/lambda/</c> where the lambdas answer by path.
    /// </summary>
    private readonly string _before;

    /// <summary>
    /// Where it goes on after the key, up to the root of the lambda.
    /// </summary>
    private readonly string _after;

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

    /// <summary>
    /// Whether the installation was given its public address, and so knows
    /// which of the names it is reached at are its own.
    /// </summary>
    private readonly bool _named;

    #region Get-/Setters

    public bool ByPath { get; }

    public string Domain { get; }

    public string Authority => Domain + _port;

    public string Template { get; }

    #endregion

    #region Initialization

    public LambdaAddresses(LambdaOptions options)
    {
        ByPath = options.Addressing == LambdaAddressing.Paths;

        var configured = ByPath ? options.PublicUrl ?? $"http://localhost:{options.Port}"
                                : options.HostingUrl ?? $"http://localhost:{options.Port}";

        var variable = ByPath ? "LAMBDA_PUBLIC_URL" : "LAMBDA_HOSTING_URL";

        // a name with subdomains: an address has none, and a single label
        // other than localhost resolves nowhere
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https") || url.AbsolutePath != "/"
            || url.HostNameType != UriHostNameType.Dns || DomainNames.Normalize(url.IdnHost) is not { } domain
            || (!ByPath && !domain.Contains('.') && domain != "localhost"))
        {
            throw new InvalidOperationException($"{variable} is '{configured}', which is no address lambdas can answer below. "
                                                + (ByPath ? "Set it to a scheme and a host, such as https://lambda.example.com."
                                                          : "Set it to a scheme and a domain, such as https://genhttp.run."));
        }

        _scheme = url.Scheme;
        _port = url.IsDefaultPort ? string.Empty : $":{url.Port}";

        Domain = domain;

        if (ByPath)
        {
            _before = $"{_scheme}://{Domain}{_port}/{PlatformRoutes.Lambdas}/";
            _after = "/";
        }
        else
        {
            _before = $"{_scheme}://";
            _after = $".{Domain}{_port}/";
        }

        Template = _before + Key + _after;

        _platform = [.. DomainNames.PlatformHosts(options)];

        _named = options.PublicUrl != null;

        // below a path there is no hosting domain to have a page: the host is
        // the platform's
        _home = ByPath || _platform.Contains(Domain) ? null : Domain;

        _www = "www." + Domain;
    }

    #endregion

    #region Functionality

    public string Of(string publicKey) => _before + publicKey + _after;

    public string Of(string publicKey, LambdaTier tier, string? domain)
        => Serves(tier, domain) ? $"https://{domain}/" : Of(publicKey);

    /// <summary>
    /// Whether a lambda of this tier with this domain answers at it.
    /// </summary>
    public static bool Serves(LambdaTier tier, string? domain) => domain != null && tier == LambdaTier.Premium;

    /// <summary>
    /// The same, for a tier by its name as the models carry it.
    /// </summary>
    public static bool Serves(string tier, string? domain) => Enum.TryParse<LambdaTier>(tier, out var parsed) && Serves(parsed, domain);

    /// <summary>
    /// Whether the platform answers at a host that is not below the hosting
    /// domain: at one of its names - localhost, its public address and the
    /// hosts its agents' endpoint is driven from - and, on an installation
    /// that was given no public address, at any host at all.
    /// </summary>
    /// <remarks>
    /// Without a public address there is no telling the platform's own names
    /// from anybody else's - a laptop on the network is reached by whatever
    /// its owner types - so nothing is turned away.
    /// </remarks>
    /// <param name="host">The host in its normalized form, see <see cref="DomainNames.Normalize"/></param>
    public bool IsPlatform(string host) => !_named || _platform.Contains(host);

    /// <summary>
    /// Whether a host is the platform's or the hosting domain's - one of its
    /// names, the domain itself or below it - and so never a lambda's own.
    /// </summary>
    /// <param name="host">The host in its normalized form, see <see cref="DomainNames.Normalize"/></param>
    public bool Owns(string host) => _platform.Contains(host) || Resolve(host) != null;

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
        // the hosting domain - www, say; and below a path, there is no
        // hosting domain to be below
        if (ByPath || host.Length <= Domain.Length + 1 || _platform.Contains(host))
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
