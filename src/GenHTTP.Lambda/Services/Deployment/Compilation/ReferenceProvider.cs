using Microsoft.CodeAnalysis;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Decides which assemblies a lambda is compiled against. Everything the host
/// needs to run itself - the database, the compiler, the engine and this
/// application - is deliberately left out, so a snippet cannot even name those
/// types, let alone call them.
/// </summary>
public static class ReferenceProvider
{
    private static readonly string[] Excluded =
    [
        "GenHTTP.Lambda",
        "GenHTTP.Engine",
        "GenHTTP.Testing",
        "GenHTTP.Modules.DependencyInjection",
        "GenHTTP.Modules.ReverseProxy",
        "Microsoft.CodeAnalysis",
        "Microsoft.Data.Sqlite",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions.DependencyInjection",
        "SQLitePCLRaw",
        "Evolve",
        "ioxide"
    ];

    private static readonly Lazy<MetadataReference[]> References = new(Collect, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// The metadata references a lambda may be compiled against.
    /// </summary>
    public static MetadataReference[] Resolve() => References.Value;

    private static MetadataReference[] Collect()
    {
        var references = new List<MetadataReference>();

        foreach (var assembly in GetPlatformAssemblies())
        {
            var name = Path.GetFileNameWithoutExtension(assembly);

            if (IsExcluded(name))
            {
                continue;
            }

            try
            {
                references.Add(MetadataReference.CreateFromFile(assembly));
            }
            catch (Exception)
            {
                // an assembly that cannot be read simply is not available to lambdas
            }
        }

        return [..references];
    }

    /// <summary>
    /// Every assembly the runtime resolves for this application, which is a
    /// superset of what is loaded at any given moment.
    /// </summary>
    private static IEnumerable<string> GetPlatformAssemblies()
    {
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted && trusted.Length > 0)
        {
            return trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        return AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                        .Select(a => a.Location);
    }

    private static bool IsExcluded(string name)
    {
        foreach (var prefix in Excluded)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

}
