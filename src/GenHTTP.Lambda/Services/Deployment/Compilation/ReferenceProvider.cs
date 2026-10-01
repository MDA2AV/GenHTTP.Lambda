using System.Reflection.PortableExecutable;

using Microsoft.CodeAnalysis;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Decides which assemblies a lambda is compiled against. Everything the host
/// needs to run itself - the compiler, the engine and this application - is
/// deliberately left out, so a snippet cannot even name those types, let alone
/// call them.
/// </summary>
/// <remarks>
/// Microsoft.Data.Sqlite, Entity Framework Core and Evolve are in: a lambda
/// talks to its database through the first, keeps its records with the second
/// if it likes, and migrates the schema with the third. What is under them -
/// the native SQLite and its bindings, and the dependency injection Entity
/// Framework builds its services with - stays out, so the handle of a
/// connection and the services behind a context are types the code cannot
/// name. The code guard keeps a context on the connection the lambda is
/// handed, see <see cref="CodeGuard.InspectEntityFramework" />.
/// </remarks>
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
        "Microsoft.Extensions.DependencyInjection",
        "SQLitePCLRaw",
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
                references.Add(Read(assembly));
            }
            catch (Exception)
            {
                // an assembly that cannot be read simply is not available to lambdas
            }
        }

        return [..references];
    }

    /// <summary>
    /// Reads the metadata of an assembly, and nothing else of it.
    /// </summary>
    /// <remarks>
    /// <see cref="MetadataReference.CreateFromFile(string, MetadataReferenceProperties, DocumentationProvider)" />
    /// copies the whole file into native memory - its IL and its precompiled
    /// code as well - and keeps it for as long as the reference lives, which
    /// here is the life of the process. Over the three hundred assemblies a
    /// lambda may reference that came to ninety megabytes, of which the
    /// compiler reads the metadata alone: a reference is compiled against,
    /// never run. Mapping the files instead would cost as little memory but
    /// hold every one of them open.
    /// </remarks>
    private static MetadataReference Read(string assembly)
    {
        using var stream = File.OpenRead(assembly);

        var module = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);

        return AssemblyMetadata.Create(module).GetReference(filePath: assembly);
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
