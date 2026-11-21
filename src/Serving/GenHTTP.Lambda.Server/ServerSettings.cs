using System.Numerics;

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
    /// <remarks>
    /// GenHTTP's internal engine unless a host or <c>LAMBDA_ENGINE</c> says
    /// otherwise: it runs wherever .NET does, where io_uring is refused by
    /// most container runtimes and Kestrel needs ASP.NET Core.
    /// </remarks>
    public LambdaEngine Engine { get; init; } = LambdaEngine.Internal;

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
    /// How many reads of 32 KB each reactor of the io_uring engine shares
    /// between its connections.
    /// </summary>
    /// <remarks>
    /// The engine's own default is 4096 - 128 MB a reactor, and there is a
    /// reactor per core. The kernel takes them in turn, so over a day of
    /// traffic every one of them is written to and stays in memory: up to
    /// half a gigabyte on four cores, for reads of which a few are ever
    /// waiting at once. A request is read within a moment; what holds reads is a body
    /// read more slowly than it arrives. When they run out the engine waits
    /// for them to come back rather than dropping anybody, so fewer only
    /// means that one large upload makes the other connections of its reactor
    /// wait sooner - after 32 MB here instead of 128. A power of two.
    /// </remarks>
    public int ReceiveBuffers { get; init; } = 1024;

    /// <summary>
    /// How many reads a connection may have waiting on the io_uring engine
    /// before the engine drops it.
    /// </summary>
    /// <remarks>
    /// The engine does not slow a sender down while a request is read more
    /// slowly than it arrives: it keeps what came in and, once this many reads
    /// are waiting, closes the connection. Its own default of 64 is two
    /// megabytes, which a JSON body of a few megabytes outruns while it is
    /// being parsed - uploads of 24 MB were dropped even at 5 MB/s. What
    /// slows the sender down instead is the reactor running out of
    /// <see cref="ReceiveBuffers"/>, so a connection may always have more
    /// reads waiting than its reactor has - see <see cref="Receiving"/>.
    /// With 1024 reads against 4096 shared, 100 MB sent at 50 MB/s was
    /// dropped; with 4096 against 4096, 200 MB went through at the full speed
    /// of the loopback.
    /// </remarks>
    public int ReceiveQueueEntries { get; init; } = 2048;

    /// <summary>
    /// The reads each reactor shares, and the reads a connection may have
    /// waiting, as the io_uring engine is given them.
    /// </summary>
    /// <remarks>
    /// Both are rounded up to powers of two, which the engine requires. A
    /// connection may have at least twice the reads its reactor shares
    /// waiting, whatever was asked for: with all of them on one connection
    /// its queue must still not fill up, or the engine drops it instead of
    /// slowing it down - and the end of a connection takes a place in the
    /// queue as well. Each place is 24 bytes, allocated with the connection.
    /// </remarks>
    public (int Buffers, int QueueEntries) Receiving()
    {
        var buffers = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Clamp(ReceiveBuffers, 64, 32768));

        var queue = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Clamp(ReceiveQueueEntries, 64, 65536));

        return (buffers, Math.Max(queue, buffers * 2));
    }

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
