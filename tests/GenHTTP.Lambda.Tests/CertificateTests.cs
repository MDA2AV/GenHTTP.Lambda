using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;

using Microsoft.Extensions.Logging.Abstractions;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The certificate the TLS endpoint is secured with, including the reload that
/// keeps a renewal from needing a restart.
/// </summary>
[TestClass]
public sealed class CertificateTests
{

    [TestMethod]
    public void NoCertificateMeansNoSecureEndpoint()
    {
        Assert.IsFalse(new LambdaOptions().Secure);

        Assert.IsFalse(new LambdaOptions { SecurePort = 8443 }.Secure, "a port alone is not enough");

        Assert.IsFalse(new LambdaOptions { CertificatePath = "/certs/full.pem" }.Secure, "a certificate alone is not enough");

        Assert.IsTrue(new LambdaOptions { SecurePort = 8443, CertificatePath = "/certs/full.pem" }.Secure);
    }

    [TestMethod]
    public void PemPairsAreLoaded()
    {
        using var workspace = new TemporaryDirectory();

        var options = Write(workspace, "first.example");

        using var loader = new CertificateLoader(options, NullLogger<CertificateLoader>.Instance);

        Assert.AreEqual("CN=first.example", loader.Provide("first.example")?.Subject);
    }

    [TestMethod]
    public void MissingCertificatesAreReportedOnStartup()
    {
        var options = new LambdaOptions { SecurePort = 8443, CertificatePath = "/does/not/exist.pem" };

        Assert.ThrowsExactly<FileNotFoundException>(() => new CertificateLoader(options, NullLogger<CertificateLoader>.Instance));
    }

    [TestMethod]
    public void RenewedCertificatesArePickedUp()
    {
        using var workspace = new TemporaryDirectory();

        var options = Write(workspace, "before.example");

        using var loader = new CertificateLoader(options, NullLogger<CertificateLoader>.Instance);

        Assert.AreEqual("CN=before.example", loader.Provide(null)?.Subject);

        Write(workspace, "after.example");

        Assert.AreEqual("CN=after.example", loader.Provide(null)?.Subject);
    }

    [TestMethod]
    public void BrokenRenewalsKeepThePreviousCertificate()
    {
        using var workspace = new TemporaryDirectory();

        var options = Write(workspace, "intact.example");

        using var loader = new CertificateLoader(options, NullLogger<CertificateLoader>.Instance);

        Touch(options.CertificatePath!, "-----BEGIN CERTIFICATE-----\nnot a certificate\n-----END CERTIFICATE-----");

        Assert.AreEqual("CN=intact.example", loader.Provide(null)?.Subject, "a half written file must not take the endpoint down");
    }

    [TestMethod]
    public void AChainYieldsTheLeafAndNotItsIssuer()
    {
        using var workspace = new TemporaryDirectory();

        var options = WriteChain(workspace, "leaf.example", "issuer.example");

        using var loader = new CertificateLoader(options, NullLogger<CertificateLoader>.Instance);

        var served = loader.Provide(null);

        Assert.AreEqual("CN=leaf.example", served?.Subject, "the issuer must not be served in place of the leaf");
        Assert.IsTrue(served!.HasPrivateKey, "the served certificate has to be able to answer a handshake");
    }

    #region Helpers

    /// <summary>
    /// Writes a self signed certificate into the workspace and returns the
    /// options pointing at it.
    /// </summary>
    private static LambdaOptions Write(TemporaryDirectory workspace, string name)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest($"CN={name}", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        var certificatePath = Path.Combine(workspace.Path, "fullchain.pem");
        var keyPath = Path.Combine(workspace.Path, "privkey.pem");

        Touch(certificatePath, certificate.ExportCertificatePem());
        Touch(keyPath, key.ExportPkcs8PrivateKeyPem());

        return new LambdaOptions
        {
            SecurePort = 8443,
            CertificatePath = certificatePath,
            CertificateKeyPath = keyPath
        };
    }

    /// <summary>
    /// Writes a file and stamps it, so a rewrite within the resolution of the
    /// file system still reads as a change.
    /// </summary>
    /// <summary>
    /// Writes a leaf followed by its issuer, the way an ACME client does.
    /// </summary>
    private static LambdaOptions WriteChain(TemporaryDirectory workspace, string leaf, string issuer)
    {
        using var issuerKey = RSA.Create(2048);

        var issuerRequest = new CertificateRequest($"CN={issuer}", issuerKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        issuerRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

        using var authority = issuerRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(2));

        using var leafKey = RSA.Create(2048);

        var leafRequest = new CertificateRequest($"CN={leaf}", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var signed = leafRequest.Create(authority, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), Guid.NewGuid().ToByteArray());

        var certificatePath = Path.Combine(workspace.Path, "fullchain.pem");
        var keyPath = Path.Combine(workspace.Path, "privkey.pem");

        Touch(certificatePath, signed.ExportCertificatePem() + "\n" + authority.ExportCertificatePem());
        Touch(keyPath, leafKey.ExportPkcs8PrivateKeyPem());

        return new LambdaOptions { SecurePort = 8443, CertificatePath = certificatePath, CertificateKeyPath = keyPath };
    }

    private static void Touch(string path, string content)
    {
        File.WriteAllText(path, content);

        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(Interlocked.Increment(ref _tick)));
    }

    private static long _tick;

    private sealed class TemporaryDirectory : IDisposable
    {
        internal string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"lambda-certs-{Guid.NewGuid():N}");

        internal TemporaryDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            try { Directory.Delete(Path, true); } catch (Exception) { /* best effort */ }
        }
    }

    #endregion

}
