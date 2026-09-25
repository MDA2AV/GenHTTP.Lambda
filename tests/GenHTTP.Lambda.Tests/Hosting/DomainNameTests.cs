using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// What counts as a domain an owner can claim, and the form it is kept in.
/// </summary>
[TestClass]
public sealed class DomainNameTests
{

    private static readonly LambdaOptions Options = new()
    {
        PublicUrl = "https://genhttp.dev",
        McpOrigins = ["genhttp.dev", "mirror.example.net"]
    };

    [TestMethod]
    [DataRow("shop.example.com", "shop.example.com")]
    [DataRow("  Shop.Example.COM  ", "shop.example.com")]
    [DataRow("shop.example.com.", "shop.example.com")]
    [DataRow("shop.example.com:8080", "shop.example.com")]
    [DataRow("https://shop.example.com/some/page?x=1", "shop.example.com")]
    [DataRow("bücher.example", "xn--bcher-kva.example")]
    [DataRow("a-b.c-d.example", "a-b.c-d.example")]
    public void WhatIsTypedIsStoredTheWayItIsMatched(string typed, string stored)
    {
        Assert.IsTrue(DomainNames.TryNormalize(typed, Options, out var normalized, out var reason), reason);
        Assert.AreEqual(stored, normalized);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("example")]
    [DataRow("152.53.120.139")]
    [DataRow("[2a0a:4cc0:c0:4fc0::1]")]
    [DataRow("-shop.example.com")]
    [DataRow("shop-.example.com")]
    [DataRow("sh_op.example.com")]
    [DataRow("shop..example.com")]
    [DataRow("example.123")]
    public void WhatCannotBeADomainIsRefused(string typed)
    {
        Assert.IsFalse(DomainNames.TryNormalize(typed, Options, out _, out var reason));
        Assert.IsNotNull(reason);
    }

    [TestMethod]
    [DataRow("genhttp.dev")]
    [DataRow("GENHTTP.dev")]
    [DataRow("www.genhttp.dev")]
    [DataRow("deep.below.genhttp.dev")]
    [DataRow("mirror.example.net")]
    [DataRow("app.localhost")]
    public void ThePlatformsOwnNamesCannotBeClaimed(string typed)
    {
        Assert.IsFalse(DomainNames.TryNormalize(typed, Options, out _, out var reason));
        Assert.Contains("platform", reason!, StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void ANameMerelyEndingLikeThePlatformsIsNotIt()
    {
        Assert.IsTrue(DomainNames.TryNormalize("notgenhttp.dev", Options, out _, out _));
    }

    [TestMethod]
    [DataRow("shop.example.com", "shop.example.com")]
    [DataRow("SHOP.example.com:443", "shop.example.com")]
    [DataRow("shop.example.com.", "shop.example.com")]
    [DataRow("[::1]:8080", null)]
    [DataRow("", null)]
    [DataRow(null, null)]
    public void AHostHeaderIsReducedToTheDomainItNames(string? header, string? domain)
    {
        Assert.AreEqual(domain, DomainNames.Normalize(header));
    }

}
