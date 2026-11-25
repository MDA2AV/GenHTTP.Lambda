using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.PortableExecutable;

using GenHTTP.Lambda.Integrations.Databases;

using Microsoft.CodeAnalysis;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Decides which assemblies a lambda is compiled against. Everything the host
/// needs to run itself - the compiler, the engine and this application - is
/// deliberately left out, so a snippet cannot even name those types, let alone
/// call them.
/// </summary>
/// <remarks>
/// Entity Framework Core and Evolve are in: a lambda keeps its records with
/// the first if it likes, and migrates the schema with the second, on the
/// connection it is handed. The client library of a database is in only where
/// the database integration names it (<see cref="DatabaseBinding.Assemblies"/>):
/// one a lambda could name would let it open connections of its own, to any
/// file or server. What is under them - the native SQLite and its bindings,
/// and the dependency injection Entity Framework builds its services with -
/// stays out, so the handle of a connection and the services behind a context
/// are types the code cannot name. The code guard keeps a context on the
/// connection the lambda is handed, see <see cref="CodeGuard.InspectEntityFramework" />.
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

    /// <summary>
    /// The assembly of the host, whatever it is called: a host of somebody's
    /// own has an application of its own, whose types are no lambda's to name.
    /// </summary>
    private static readonly string? Host = Assembly.GetEntryAssembly()?.GetName().Name;

    /// <summary>
    /// The client libraries of databases the platform knows of, by the names
    /// of their assemblies: handed to a lambda only where the integration of
    /// its database names them.
    /// </summary>
    private static readonly string[] DatabaseClients =
    [
        "Microsoft.Data.Sqlite",
        "Microsoft.EntityFrameworkCore.Sqlite",
        "Npgsql",
        "MySqlConnector",
        "MySql.",
        "Pomelo.",
        "Microsoft.Data.SqlClient",
        "Microsoft.EntityFrameworkCore.SqlServer",
        "Oracle."
    ];

    /// <summary>
    /// What a lambda that may not connect to other servers is compiled
    /// without, by the names of the assemblies they start with.
    /// </summary>
    private static readonly string[] Network =
    [
        "System.Net.Http",
        "System.Net.Sockets",
        "System.Net.Requests",
        "System.Net.WebClient",
        "System.Net.WebSockets.Client",
        "System.Net.Quic",
        "System.Net.NameResolution",
        "System.Net.Ping",
        "System.Net.NetworkInformation",
        "System.IO.Pipes"
    ];

    private static readonly Lazy<MetadataReference[]> References = new(Collect, LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly ConcurrentDictionary<(string Clients, bool Connectivity), MetadataReference[]> Filtered = [];

    /// <summary>
    /// Every metadata reference that may be handed to a lambda, whatever
    /// keeps its database - for what asks which namespaces there are.
    /// </summary>
    public static MetadataReference[] Resolve() => References.Value;

    /// <summary>
    /// The metadata references a lambda may be compiled against, at most:
    /// those of one that may connect to other servers.
    /// </summary>
    public static MetadataReference[] Resolve(DatabaseBinding binding) => Resolve(true, binding);

    /// <summary>
    /// The metadata references a lambda is compiled against, which leave out
    /// what connects to other servers unless it may, and every client library
    /// of a database but the one its integration names.
    /// </summary>
    /// <remarks>
    /// The same references whatever is asked, filtered once per combination -
    /// nothing is read twice.
    /// </remarks>
    /// <param name="connectivity">Whether the lambda may connect to other servers</param>
    /// <param name="binding">How the lambda is bound to its database</param>
    public static MetadataReference[] Resolve(bool connectivity, DatabaseBinding binding)
        => Filtered.GetOrAdd((string.Join(',', binding.Assemblies), connectivity),
                             key => [.. References.Value.Where(r => (key.Connectivity || !IsNetwork(r)) && IsHanded(r, binding))]);

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

    private static bool IsExcluded(string name) => Starts(name, Excluded) || name.Equals(Host, StringComparison.OrdinalIgnoreCase);

    private static bool IsNetwork(MetadataReference reference)
        => reference is PortableExecutableReference { FilePath: { } path } && Starts(Path.GetFileNameWithoutExtension(path), Network);

    /// <summary>
    /// Whether a reference is no client library of a database, or the one the
    /// integration of the lambda's database names.
    /// </summary>
    private static bool IsHanded(MetadataReference reference, DatabaseBinding binding)
    {
        if (reference is not PortableExecutableReference { FilePath: { } path })
        {
            return true;
        }

        var name = Path.GetFileNameWithoutExtension(path);

        return !Starts(name, DatabaseClients) || Starts(name, [.. binding.Assemblies]);
    }

    private static bool Starts(string name, string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

}
