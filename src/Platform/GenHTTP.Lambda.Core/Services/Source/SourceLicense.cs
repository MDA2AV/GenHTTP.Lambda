using System.Globalization;
using System.Reflection;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A license a source can be published under.
/// </summary>
/// <param name="Id">Its SPDX identifier, which is also how it is stored and asked for</param>
/// <param name="Name">Its full name</param>
/// <param name="Kind">What it asks of whoever reuses the code, roughly</param>
public sealed record SourceLicense(string Id, string Name, LicenseKind Kind)
{

    /// <summary>
    /// Where the license is described in full.
    /// </summary>
    public string Url => $"https://spdx.org/licenses/{Id}.html";

}
