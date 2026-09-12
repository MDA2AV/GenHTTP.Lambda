using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The rules a key has to follow to be hosted as a path segment.
/// </summary>
[TestClass]
public sealed class LambdaKeyTests
{

    [TestMethod]
    public void KeysAreNormalizedToLowerCase()
    {
        Assert.IsTrue(LambdaKeys.TryNormalize("  My-Lambda  ", out var normalized, out _));

        Assert.AreEqual("my-lambda", normalized);
    }

    [TestMethod]
    public void GeneratedKeysAreAccepted()
    {
        Assert.IsTrue(LambdaKeys.TryNormalize(LambdaKeys.CreatePublicKey(), out _, out var reason), reason);

        Assert.AreEqual(32, LambdaKeys.CreatePrivateKey().Length);
    }

    [TestMethod]
    [DataRow("ab", "too short")]
    [DataRow("hello world", "contains a space")]
    [DataRow("hello/world", "contains a path separator")]
    [DataRow("hello_world", "contains an underscore")]
    [DataRow("-lambda", "starts with a dash")]
    [DataRow("lambda-", "ends with a dash")]
    [DataRow("editor", "is reserved")]
    [DataRow("", "is empty")]
    public void InvalidKeysAreRejected(string key, string because)
    {
        Assert.IsFalse(LambdaKeys.TryNormalize(key, out _, out var reason), $"'{key}' should be rejected because it {because}");

        Assert.IsNotNull(reason);
    }

}
