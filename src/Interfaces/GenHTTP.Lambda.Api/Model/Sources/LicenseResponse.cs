using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A license a source can be published under.")]
public sealed record LicenseResponse(
    [Summary("Its SPDX identifier, which is how it is asked for: `MIT`, `Apache-2.0`, `GPL-3.0-or-later` …")]
    string Id,
    [Summary("Its full name")]
    string Name,
    [Summary("`Permissive`, `Copyleft` or `PublicDomain` - roughly what it asks of whoever reuses the code")]
    string Kind,
    [Summary("Where it is described in full")]
    string Url
)
{

    public static LicenseResponse Of(SourceLicense license) => new(license.Id, license.Name, license.Kind.ToString(), license.Url);

    public static LicenseResponse Of(string id) => Of(SourceLicenses.Find(id) ?? SourceLicenses.Find(SourceLicenses.Default)!);

}
