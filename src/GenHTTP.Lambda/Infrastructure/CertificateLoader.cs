using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

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
/// </remarks>
public sealed class CertificateLoader : ICertificateProvider, IDisposable
{
    private readonly LambdaOptions _options;

    private readonly ILogger<CertificateLoader> _logger;

    private readonly Lock _lock = new();

    private X509Certificate2 _certificate;

    private DateTime _loaded;

    #region Initialization

    /// <summary>
    /// Loads the configured certificate, so a missing or malformed one is
    /// reported on startup rather than on the first request.
    /// </summary>
    public CertificateLoader(LambdaOptions options, ILogger<CertificateLoader> logger)
    {
        _options = options;
        _logger = logger;

        _certificate = Load();
        _loaded = Stamp();

        _logger.LogInformation("Loaded certificate for '{Subject}', valid until {Expiry:u}", _certificate.Subject, _certificate.NotAfter);
    }

    #endregion

    #region Functionality

    public X509Certificate2? Provide(string? host)
    {
        lock (_lock)
        {
            var stamp = Stamp();

            if (stamp == _loaded)
            {
                return _certificate;
            }

            try
            {
                var renewed = Load();

                _logger.LogInformation("Reloaded renewed certificate for '{Subject}', valid until {Expiry:u}", renewed.Subject, renewed.NotAfter);

                var previous = _certificate;

                _certificate = renewed;

                previous.Dispose();
            }
            catch (Exception e)
            {
                // the files may be halfway through being replaced - keep serving
                // the certificate we have, the next write is another attempt
                _logger.LogWarning(e, "Certificate changed on disk but could not be loaded, keeping the previous one");
            }
            finally
            {
                // either way this version has been seen, so a file that stays
                // broken is not retried (and not logged) on every connection
                _loaded = stamp;
            }

            return _certificate;
        }
    }

    /// <summary>
    /// Reads the certificate, either from a PEM pair or from a PKCS#12 archive.
    /// </summary>
    private X509Certificate2 Load()
    {
        var path = _options.CertificatePath ?? throw new InvalidOperationException("No certificate has been configured.");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The certificate '{path}' does not exist.", path);
        }

        if (!string.IsNullOrWhiteSpace(_options.CertificateKeyPath))
        {
            if (!File.Exists(_options.CertificateKeyPath))
            {
                throw new FileNotFoundException($"The certificate key '{_options.CertificateKeyPath}' does not exist.", _options.CertificateKeyPath);
            }

            return X509Certificate2.CreateFromPemFile(path, _options.CertificateKeyPath);
        }

        return X509CertificateLoader.LoadPkcs12FromFile(path, _options.CertificatePassword);
    }

    /// <summary>
    /// The newest write timestamp across the files the certificate is read from.
    /// </summary>
    private DateTime Stamp()
    {
        var stamp = Written(_options.CertificatePath);

        var key = Written(_options.CertificateKeyPath);

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

    public void Dispose() => _certificate.Dispose();

    #endregion

}
