using GenHTTP.Lambda.Server;
using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// The health check of the images, which have no shell: the server's own
/// binary asks the server beside it and exits with what it found.
/// </summary>
[TestClass]
public sealed class HealthProbeTests
{

    [TestMethod]
    public async Task AServerThatAnswersIsHealthy()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        Assert.AreEqual(0, await HealthProbe.CheckAsync(new ServerSettings { Port = (ushort)fixture.Host.Port }));
    }

    [TestMethod]
    public async Task NoServerIsNotHealthy()
    {
        // a port that was free a moment ago, and nothing listens on it
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);

        listener.Start();

        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

        listener.Stop();

        Assert.AreEqual(1, await HealthProbe.CheckAsync(new ServerSettings { Port = (ushort)port }));
    }

    [TestMethod]
    public void OnlyTheArgumentAloneAsksForTheCheck()
    {
        Assert.IsTrue(HealthProbe.IsRequested(["--health"]));

        Assert.IsFalse(HealthProbe.IsRequested([]));
        Assert.IsFalse(HealthProbe.IsRequested(["--health", "now"]));
    }

}
