using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What the overview of a lambda says about one of its versions: how large it
/// is, what its code reaches for and what its documentation says.
/// </summary>
/// <param name="CodeFiles">Every file of its code: the C# it is compiled from and whatever else it is kept with</param>
/// <param name="CodeBytes">What those weigh</param>
/// <param name="ResourceFiles">The files it reads and serves while it runs</param>
/// <param name="ResourceBytes">What those weigh, decoded</param>
/// <param name="UsesNetwork">Whether its code connects to other servers, which it may only with connectivity</param>
public sealed record VersionFacts(int CodeFiles, long CodeBytes, int ResourceFiles, long ResourceBytes, bool ServesResources, bool ServesWorkspace,
                                  bool UsesWorkspace, bool UsesDatabase, bool UsesNetwork, DocumentationSummary Documentation);
