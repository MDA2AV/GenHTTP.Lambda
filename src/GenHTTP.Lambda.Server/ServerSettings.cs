using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Lambda.Server;

/// <summary>
/// How a host serves the platform: the engine, the ports and the
/// certificates. None of it is the platform's concern - it answers the same
/// whatever it is served on.
/// </summary>
public sealed record ServerSettings
{

    /// <summary>
    /// The port the server listens on for plain HTTP.
    /// </summary>
    public ushort Port { get; init; } = 8080;

    /// <summary>
    /// The engine the root web server is hosted with.
    /// </summary>
    public LambdaEngine Engine { get; init; } = LambdaEngine.Ioxide;

    /// <summary>
    /// Which HTTP versions a port answers on.
    /// </summary>
    /// <remarks>
    /// This was HTTP/1.1 only for a while: the ioxide engine's HTTP/2 driver
    /// stopped writing once it had filled the window the client advertised,
    /// so Firefox, which advertises a hundred and twenty eight kilobytes,
    /// never finished loading the editor. That was fixed in ioxide, and HTTP/2
    /// turned out to be buggy again afterwards, so it is HTTP/1.1 only once
    /// more.
    ///
    /// Set LAMBDA_HTTP_PROTOCOLS=Http1AndHttp2 to try HTTP/2 again without a
    /// release.
    /// </remarks>
    public HttpProtocols Protocols { get; init; } = HttpProtocols.Http1;

    /// <summary>
    /// How many reads a connection may have waiting on the io_uring engine
    /// before the engine drops it.
    /// </summary>
    /// <remarks>
    /// Each read is up to 32 KB. The engine does not slow a sender down while
    /// a request is read more slowly than it arrives: it keeps what came in
    /// and, once this many reads are waiting, closes the connection. Its own
    /// default of 64 is two megabytes, which a JSON body of a few megabytes
    /// outruns while it is being parsed - uploads of 24 MB were dropped even
    /// at 5 MB/s. The reads come out of the ones a reactor shares between its
    /// connections, 4096 of them, and when those run out the engine waits for
    /// them to come back rather than dropping anybody. At 4096 a connection is
    /// therefore only ever slowed down: 200 MB went through at the full speed
    /// of the loopback, where 1024 dropped 100 MB sent at 50 MB/s. The price
    /// is that one large upload can hold the reads of its reactor while it is
    /// consumed, and the other connections on that reactor wait for them.
    /// </remarks>
    public int ReceiveQueueEntries { get; init; } = 4096;

    /// <summary>
    /// The port the server offers TLS on. Zero leaves the secure endpoint off.
    /// </summary>
    public ushort SecurePort { get; init; }

    /// <summary>
    /// The certificate the TLS endpoint is secured with, either a PEM chain or
    /// a PKCS#12 archive.
    /// </summary>
    public string? CertificatePath { get; init; }

    /// <summary>
    /// The private key belonging to the certificate. Set for a PEM pair, left
    /// empty when the archive carries its own key.
    /// </summary>
    public string? CertificateKeyPath { get; init; }

    /// <summary>
    /// The password of the PKCS#12 archive, if it has one.
    /// </summary>
    public string? CertificatePassword { get; init; }

    /// <summary>
    /// A directory holding further certificates, one subdirectory per name,
    /// each with a <c>fullchain.pem</c> and a <c>privkey.pem</c> beside it.
    /// </summary>
    /// <remarks>
    /// The server answers to more than one hostname and a certificate names
    /// the hosts it is good for, so the one to present depends on the host the
    /// client asked for. The certificate above stays the default, used when a
    /// client sends no name or a name nothing here covers.
    /// </remarks>
    public string? CertificateDirectory { get; init; }

    /// <summary>
    /// The web root an ACME client writes its HTTP challenges into, such as
    /// the folder given to <c>certbot --webroot -w</c>.
    /// </summary>
    /// <remarks>
    /// Answered for every host the server receives, a lambda's own domain
    /// included, so a certificate can be issued for a name while the server
    /// keeps running. Left empty, challenges are not answered at all.
    /// </remarks>
    public string? AcmeDirectory { get; init; }

    /// <summary>
    /// Whether the server should offer a TLS endpoint next to the plain one.
    /// </summary>
    public bool Secure => SecurePort > 0 && !string.IsNullOrWhiteSpace(CertificatePath);

}
