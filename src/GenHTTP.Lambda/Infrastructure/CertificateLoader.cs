using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Provides the certificate the TLS endpoint is secured with, reloading it
/// from disk after it has been renewed.
/// </summary>
/// <remarks>
/// Certificates issued by an ACME client are replaced every couple of months
/// while the server keeps running. GenHTTP asks for the certificate once per
/// connection, so comparing the write timestamps of the files is enough to
/// pick a renewal up without a restart - two stat calls next to a handshake
/// are not worth caching around.
///
/// The server answers to more than one hostname, and a certificate is only
/// good for the hosts it names, so there may be several: the one the client
/// asked for by name is presented, and the configured one answers for anything
/// left over.
///
/// Which of the two ways that happens depends on the engine. Kestrel terminates
/// TLS in .NET and asks for a certificate per connection, so the name comes in
/// through <see cref="Provide" />. The io_uring engine terminates it itself and
/// reads the PEM files directly, so it wants the names up front and a path per
/// name instead - hence <see cref="Hosts" /> and <see cref="ProvideFiles" />.
/// </remarks>
public sealed class CertificateLoader : IHostCertificateProvider, IFileCertificateProvider, IDisposable
{
    private readonly ILogger<CertificateLoader> _logger;

    private readonly Lock _lock = new();

    private readonly Entry _default;

    private readonly List<Entry> _entries;

    #region Initialization

    /// <summary>
    /// Loads the configured certificates, so a missing or malformed one is
    /// reported on startup rather than on the first request.
    /// </summary>
    public CertificateLoader(LambdaOptions options, ILogger<CertificateLoader> logger)
    {
        _logger = logger;

        var path = options.CertificatePath ?? throw new InvalidOperationException("No certificate has been configured.");

        _default = new Entry(path, options.CertificateKeyPath, options.CertificatePassword);

        _entries = [_default, ..Discover(options.CertificateDirectory, path)];

        foreach (var entry in _entries)
        {
            Load(entry);

            _logger.LogInformation("Loaded certificate for {Names}, valid until {Expiry:u}", string.Join(", ", entry.Names), entry.Certificate!.NotAfter);
        }
    }

    /// <summary>
    /// Finds the certificates published next to the configured one, a
    /// subdirectory per name in the layout an ACME client keeps them in.
    /// </summary>
    private IEnumerable<Entry> Discover(string? directory, string configured)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            yield break;
        }

        var primary = Path.GetFullPath(configured);

        foreach (var candidate in Directory.EnumerateDirectories(directory).OrderBy(d => d, StringComparer.Ordinal))
        {
            var chain = Path.Combine(candidate, "fullchain.pem");

            var key = Path.Combine(candidate, "privkey.pem");

            if (!File.Exists(chain) || !File.Exists(key) || Path.GetFullPath(chain) == primary)
            {
                continue;
            }

            yield return new Entry(chain, key, null);
        }
    }

    #endregion

    #region Functionality

    public X509Certificate2? Provide(string? host)
    {
        lock (_lock)
        {
            foreach (var entry in _entries)
            {
                Refresh(entry);
            }

            if (host != null)
            {
                foreach (var entry in _entries)
                {
                    if (Covers(entry, host))
                    {
                        return entry.Certificate;
                    }
                }
            }

            return _default.Certificate;
        }
    }

    /// <summary>
    /// Every hostname the server holds a certificate for.
    /// </summary>
    /// <remarks>
    /// The io_uring engine registers a certificate per name when it starts, so
    /// a name missing here is one it will answer for with the default
    /// certificate however well covered it is.
    /// </remarks>
    public IEnumerable<string> Hosts
    {
        get
        {
            lock (_lock)
            {
                return _entries.SelectMany(e => e.Names)
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .ToList();
            }
        }
    }

    /// <summary>
    /// The files the certificate for a host is read from, for an engine that
    /// reads them itself rather than taking a loaded certificate.
    /// </summary>
    /// <remarks>
    /// Chooses the same certificate <see cref="Provide" /> would. An archive
    /// has no separate key file to hand over, so it answers with nothing and
    /// the engine falls back to the loaded certificate - falling through to
    /// the default files instead would serve that name someone else's.
    /// </remarks>
    public CertificateFiles? ProvideFiles(string? host)
    {
        lock (_lock)
        {
            if (host != null)
            {
                foreach (var entry in _entries)
                {
                    if (Covers(entry, host))
                    {
                        return Files(entry);
                    }
                }
            }

            return Files(_default);
        }
    }

    private static CertificateFiles? Files(Entry entry)
        => !string.IsNullOrWhiteSpace(entry.KeyPath) ? new CertificateFiles(entry.Path, entry.KeyPath) : null;

    /// <summary>
    /// Whether a certificate names the host the client asked for, including
    /// the one level of subdomain a wildcard stands for.
    /// </summary>
    private static bool Covers(Entry entry, string host)
    {
        foreach (var name in entry.Names)
        {
            if (string.Equals(name, host, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (name.StartsWith("*.", StringComparison.Ordinal))
            {
                var suffix = name[1..];

                // a wildcard covers one label, so what precedes the suffix has
                // to be a label itself rather than a further name of its own
                if (host.Length > suffix.Length
                    && host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    && !host.AsSpan(0, host.Length - suffix.Length).Contains('.'))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Reloads a certificate that has been replaced on disk since it was read.
    /// </summary>
    private void Refresh(Entry entry)
    {
        var stamp = Stamp(entry);

        if (stamp == entry.Loaded)
        {
            return;
        }

        var previous = entry.Certificate;

        try
        {
            Load(entry);

            _logger.LogInformation("Reloaded renewed certificate for {Names}, valid until {Expiry:u}", string.Join(", ", entry.Names), entry.Certificate!.NotAfter);

            previous?.Dispose();
        }
        catch (Exception e)
        {
            // the files may be halfway through being replaced - keep serving
            // the certificate we have, the next write is another attempt
            _logger.LogWarning(e, "Certificate '{Path}' changed on disk but could not be loaded, keeping the previous one", entry.Path);

            entry.Certificate = previous;
        }
        finally
        {
            // either way this version has been seen, so a file that stays
            // broken is not retried (and not logged) on every connection
            entry.Loaded = stamp;
        }
    }

    /// <summary>
    /// Reads the certificate, either from a PEM pair or from a PKCS#12 archive,
    /// and notes the hosts it is good for.
    /// </summary>
    private void Load(Entry entry)
    {
        if (!File.Exists(entry.Path))
        {
            throw new FileNotFoundException($"The certificate '{entry.Path}' does not exist.", entry.Path);
        }

        X509Certificate2 certificate;

        if (!string.IsNullOrWhiteSpace(entry.KeyPath))
        {
            if (!File.Exists(entry.KeyPath))
            {
                throw new FileNotFoundException($"The certificate key '{entry.KeyPath}' does not exist.", entry.KeyPath);
            }

            // a PEM chain holds the issuers after the leaf, and reading the file
            // as a certificate keeps only the first of them
            var chain = new X509Certificate2Collection();

            chain.ImportFromPemFile(entry.Path);

            Publish(chain);

            certificate = X509Certificate2.CreateFromPemFile(entry.Path, entry.KeyPath);
        }
        else
        {
            var archive = X509CertificateLoader.LoadPkcs12CollectionFromFile(entry.Path, entry.Password);

            Publish(archive);

            certificate = archive.FirstOrDefault(c => c.HasPrivateKey)
                          ?? X509CertificateLoader.LoadPkcs12FromFile(entry.Path, entry.Password);
        }

        entry.Certificate = certificate;
        entry.Names = Names(certificate);
        entry.Loaded = Stamp(entry);
    }

    /// <summary>
    /// The hosts a certificate is valid for.
    /// </summary>
    /// <remarks>
    /// Modern certificates carry every name in the subject alternative name
    /// extension and browsers read nothing else, but the common name is taken
    /// as well so a certificate issued without the extension still matches.
    /// </remarks>
    private static string[] Names(X509Certificate2 certificate)
    {
        var names = new List<string>();

        foreach (var extension in certificate.Extensions.OfType<X509SubjectAlternativeNameExtension>())
        {
            names.AddRange(extension.EnumerateDnsNames());
        }

        var common = certificate.GetNameInfo(X509NameType.DnsName, false);

        if (!string.IsNullOrWhiteSpace(common) && !names.Contains(common, StringComparer.OrdinalIgnoreCase))
        {
            names.Add(common);
        }

        return [..names];
    }

    /// <summary>
    /// Puts the issuers of the certificate where the runtime will find them.
    /// </summary>
    /// <remarks>
    /// A server has to send the chain, not just its own certificate, or a client
    /// has nothing to build a path to a root with. .NET assembles that chain from
    /// the certificates it can find locally, so the issuers have to be in a store
    /// before the first handshake rather than in a file nobody reads. Kestrel
    /// hides the omission by fetching the issuer over the network on demand; the
    /// io_uring engine sends what it was given and the connection fails.
    /// </remarks>
    private void Publish(X509Certificate2Collection chain)
    {
        if (chain.Count < 2)
        {
            return;
        }

        try
        {
            using var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadWrite);

            foreach (var issuer in chain.Skip(1).Where(c => !c.HasPrivateKey))
            {
                store.Add(issuer);
            }

            _logger.LogInformation("Published {Count} issuer(s) of the certificate for chain building", chain.Count - 1);
        }
        catch (Exception e)
        {
            // a read only or missing store is survivable: clients that already
            // know the issuer still connect, the rest get a verification error
            _logger.LogWarning(e, "The issuers of the certificate could not be published, clients may fail to verify the chain");
        }
    }

    /// <summary>
    /// The newest write timestamp across the files the certificate is read from.
    /// </summary>
    private static DateTime Stamp(Entry entry)
    {
        var stamp = Written(entry.Path);

        var key = Written(entry.KeyPath);

        return key > stamp ? key : stamp;
    }

    private static DateTime Written(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return DateTime.MinValue;
        }

        try
        {
            // resolves the symlinks an ACME client leaves behind, so a renewal
            // that only repoints them is still seen as a change
            return File.ResolveLinkTarget(path, true) is { } target ? target.LastWriteTimeUtc : File.GetLastWriteTimeUtc(path);
        }
        catch (Exception)
        {
            return DateTime.MinValue;
        }
    }

    public void Dispose()
    {
        foreach (var entry in _entries)
        {
            entry.Certificate?.Dispose();
        }
    }

    #endregion

    #region Types

    /// <summary>
    /// One certificate, the files it is read from and the hosts it answers for.
    /// </summary>
    private sealed class Entry(string path, string? keyPath, string? password)
    {
        public string Path { get; } = path;

        public string? KeyPath { get; } = keyPath;

        public string? Password { get; } = password;

        public X509Certificate2? Certificate { get; set; }

        public string[] Names { get; set; } = [];

        public DateTime Loaded { get; set; }
    }

    #endregion

}
