using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Server;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Hosting;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Tests.UsageReports;

/// <summary>
/// Whether an installation sends its usage report, as its environment says:
/// on in the community edition unless the operator switches it off, and off
/// everywhere else.
/// </summary>
/// <remarks>
/// The environment belongs to the whole process, so these change it alone,
/// after the others, and put back what was there.
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class UsageReportSettingsTests
{
    private static readonly string[] Variables = ["LAMBDA_USAGE_REPORTS", "DO_NOT_TRACK"];

    private readonly Dictionary<string, string?> _before = [];

    [TestInitialize]
    public void Remember()
    {
        foreach (var variable in Variables)
        {
            _before[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [TestCleanup]
    public void Restore()
    {
        foreach (var (variable, value) in _before)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }

    [TestMethod]
    public void TheCommunityEditionSendsReportsUnlessTold()
    {
        Assert.AreEqual(LambdaOptions.DefaultUsageReportUrl, Read(usageReports: true));
    }

    [TestMethod]
    public void AnInstallationThatDoesNotOfferThemSendsNone()
    {
        Assert.IsNull(Read(usageReports: false));
    }

    [TestMethod]
    [DataRow("false")]
    [DataRow("False")]
    [DataRow("0")]
    [DataRow("no")]
    [DataRow("off")]
    public void TheOperatorSwitchesThemOff(string value)
    {
        Environment.SetEnvironmentVariable("LAMBDA_USAGE_REPORTS", value);

        Assert.IsNull(Read(usageReports: true));
    }

    [TestMethod]
    public void DoNotTrackIsHonoured()
    {
        Environment.SetEnvironmentVariable("DO_NOT_TRACK", "1");

        Assert.IsNull(Read(usageReports: true));
    }

    [TestMethod]
    public void TheOperatorIsToldOnStartupAndHowToSwitchThemOff()
    {
        var book = new LogBook();

        using (var loggers = LoggerFactory.Create(builder => builder.AddProvider(new LogBookProvider(book))))
        {
            LambdaSettings.FromEnvironment(LambdaAddressing.Paths, usageReports: true).Report(loggers.CreateLogger("Test"));
        }

        var said = book.Read(0, null, LogLevel.Information, 100).Lines.Select(l => l.Text).ToList();

        var line = said.SingleOrDefault(l => l.StartsWith("Usage reports are on", StringComparison.Ordinal));

        Assert.IsNotNull(line, string.Join("\n", said));
        Assert.Contains(LambdaOptions.DefaultUsageReportUrl, line);
        Assert.Contains("LAMBDA_USAGE_REPORTS=false", line);
    }

    private static string? Read(bool usageReports) => LambdaSettings.FromEnvironment(LambdaAddressing.Paths, usageReports).Platform.UsageReportUrl;

}
