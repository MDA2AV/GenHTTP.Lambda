using System.Reflection;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Knows which GenHTTP modules exist, makes sure they are loaded before the
/// first snippet is compiled and provides the namespaces that are implicitly
/// imported into every lambda - so users never have to write a using.
/// </summary>
public static class ModuleCatalog
{

    /// <summary>
    /// The GenHTTP module assemblies that lambdas may use.
    /// </summary>
    public static IReadOnlyList<string> Modules { get; } =
    [
        "ApiBrowsing",
        "Archives",
        "Authentication",
        "ClientCaching",
        "Compression",
        "Controllers",
        "Conversion",
        "DirectoryBrowsing",
        "ErrorHandling",
        "Files",
        "Functional",
        "I18n",
        "Inspection",
        "IO",
        "Layouting",
        "LoadBalancing",
        "OpenApi",
        "Pages",
        "Protobuf",
        "Redirects",
        "Reflection",
        "Security",
        "ServerSentEvents",
        "SinglePageApplications",
        "StaticWebsites",
        "VirtualHosting",
        "Webservices",
        "Websockets"
    ];

    /// <summary>
    /// The namespaces every lambda gets for free.
    /// </summary>
    public static IReadOnlyList<string> Imports { get; } = BuildImports();

    /// <summary>
    /// Loads every module assembly so it can be referenced by the compiler,
    /// even if the host itself never touches it.
    /// </summary>
    /// <returns>The assemblies that could be loaded</returns>
    public static IReadOnlyList<Assembly> LoadModules()
    {
        var loaded = new List<Assembly>();

        foreach (var module in Modules)
        {
            try
            {
                loaded.Add(Assembly.Load(new AssemblyName($"GenHTTP.Modules.{module}")));
            }
            catch (Exception)
            {
                // a module that is not part of the deployment simply cannot be used
            }
        }

        return loaded;
    }

    private static string[] BuildImports()
    {
        string[] system =
        [
            "System",
            "System.Collections",
            "System.Collections.Concurrent",
            "System.Collections.Generic",
            "System.Globalization",
            "System.IO",
            "System.Linq",
            "System.Text",
            "System.Text.Json",
            "System.Text.Json.Serialization",
            "System.Threading",
            "System.Threading.Tasks"
        ];

        string[] api =
        [
            "GenHTTP.Api.Content",
            "GenHTTP.Api.Content.IO",
            "GenHTTP.Api.Infrastructure",
            "GenHTTP.Api.Protocol"
        ];

        return [..system, ..api, ..Modules.Select(m => $"GenHTTP.Modules.{m}")];
    }

}
