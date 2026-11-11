using System.Reflection.PortableExecutable;

using Microsoft.CodeAnalysis;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Decides which assemblies a lambda is compiled against: the ones named
/// here, and nothing else the server happens to ship.
/// </summary>
/// <remarks>
/// An allow-list, not a list of what to leave out. Every assembly the
/// runtime resolves for this application used to be referenced, minus a
/// few - four hundred of them, among them ASP.NET Core (a configuration
/// that reads the environment, a file provider for any directory), Visual
/// Basic's file system, tar and zip extraction to a directory, memory
/// mapped files and the libraries the server uses for itself. A name the
/// code guard had not thought of was a way onto the disk of the host. A
/// lambda now sees the base class library it needs, the GenHTTP API and its
/// modules, and its database; an assembly the server takes on later is not
/// reachable until somebody decides it should be.
///
/// The assemblies named here still hold what a lambda may not use - the
/// core library has File and Environment - so the code guard reads every
/// name for what it means (<see cref="SymbolPolicy" />). The two together
/// are governance, not a sandbox: the code runs in the server's process.
///
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

    /// <summary>
    /// The assemblies of .NET a lambda is compiled against.
    /// </summary>
    /// <remarks>
    /// The core library and the facades that forward into it, collections,
    /// text, JSON and XML, tasks and channels, HTTP, sockets, mail and
    /// cryptography, the data annotations, compression of streams. Left out
    /// on purpose, among others: processes, memory mapped files, pipes,
    /// isolated storage, file system watchers, zip and tar extraction to
    /// disk, <c>WebClient</c> and <c>WebRequest</c> (both read <c>file://</c>),
    /// reflection emit, assembly loading, Visual Basic and the serializers
    /// that write files.
    /// </remarks>
    private static readonly string[] Framework =
    [
        "mscorlib", "netstandard", "System", "System.Core", "System.Private.CoreLib", "System.Runtime", "System.Runtime.Extensions",
        "System.Buffers", "System.Memory", "System.ValueTuple",
        "System.Collections", "System.Collections.Concurrent", "System.Collections.Immutable", "System.Collections.NonGeneric",
        "System.Collections.Specialized", "System.ObjectModel",
        "System.Linq", "System.Linq.AsyncEnumerable", "System.Linq.Expressions", "System.Linq.Queryable",
        "System.Numerics", "System.Numerics.Vectors", "System.Runtime.Intrinsics", "System.Runtime.Numerics",
        "System.Globalization", "System.Globalization.Calendars", "System.Globalization.Extensions",
        "System.Text.Encoding", "System.Text.Encoding.CodePages", "System.Text.Encoding.Extensions", "System.Text.Encodings.Web",
        "System.Text.Json", "System.Text.RegularExpressions",
        "System.Threading", "System.Threading.Channels", "System.Threading.RateLimiting", "System.Threading.Tasks",
        "System.Threading.Tasks.Extensions", "System.Threading.Timer",
        "System.Net", "System.Net.Http", "System.Net.Http.Json", "System.Net.Mail", "System.Net.NameResolution", "System.Net.Primitives",
        "System.Net.Security", "System.Net.ServerSentEvents", "System.Net.Sockets", "System.Net.WebHeaderCollection",
        "System.Net.WebSockets", "System.Net.WebSockets.Client", "System.Private.Uri", "System.Web.HttpUtility",
        "System.ComponentModel", "System.ComponentModel.Annotations", "System.ComponentModel.DataAnnotations",
        "System.ComponentModel.Primitives", "System.ComponentModel.TypeConverter",
        "System.Security.Claims", "System.Security.Cryptography", "System.Security.Cryptography.Algorithms",
        "System.Security.Cryptography.Encoding", "System.Security.Cryptography.Primitives", "System.Security.Cryptography.X509Certificates",
        "System.Security.Principal", "System.Formats.Asn1", "Microsoft.Bcl.Cryptography",
        "System.Private.Xml", "System.Private.Xml.Linq", "System.Xml", "System.Xml.Linq", "System.Xml.ReaderWriter",
        "System.Xml.XDocument", "System.Xml.XPath", "System.Xml.XPath.XDocument",
        "System.Data", "System.Data.Common", "System.Transactions.Local", "System.Diagnostics.DiagnosticSource",
        "System.IO", "System.IO.Compression", "System.IO.Compression.Brotli", "System.IO.Pipelines",
        "System.Console"
    ];

    /// <summary>
    /// What a lambda builds on and talks to its database with, beside .NET.
    /// </summary>
    /// <remarks>
    /// The GenHTTP modules are those of <see cref="ModuleCatalog.Modules" />.
    /// The rest is here because a module's public API names it: the bearer
    /// authentication hands out JSON web tokens, protobuf takes its contracts,
    /// the MCP module and server-sent events log through the abstractions of
    /// Microsoft.Extensions.Logging, Entity Framework caches in memory. What a
    /// module only uses inside - Cottle, NSwag, SharpCompress, YamlDotNet - is
    /// not referenced.
    /// </remarks>
    private static readonly string[] Libraries =
    [
        "GenHTTP.Api",
        "Microsoft.Data.Sqlite",
        "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Abstractions", "Microsoft.EntityFrameworkCore.Relational",
        "Microsoft.EntityFrameworkCore.Sqlite",
        "Evolve",
        "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.Caching.Memory",
        "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Primitives",
        "protobuf-net", "protobuf-net.Core",
        "Microsoft.IdentityModel.Abstractions", "Microsoft.IdentityModel.JsonWebTokens", "Microsoft.IdentityModel.Logging",
        "Microsoft.IdentityModel.Tokens", "System.IdentityModel.Tokens.Jwt"
    ];

    private static readonly HashSet<string> Allowed = new([..Framework, ..Libraries, ..ModuleCatalog.Modules.Select(m => $"GenHTTP.Modules.{m}")],
                                                          StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<MetadataReference[]> References = new(Collect, LazyThreadSafetyMode.ExecutionAndPublication);

    #region Functionality

    /// <summary>
    /// The metadata references a lambda may be compiled against.
    /// </summary>
    public static MetadataReference[] Resolve() => References.Value;

    /// <summary>
    /// Whether a lambda is compiled against the assembly of this name.
    /// </summary>
    public static bool IsReferenced(string assembly) => Allowed.Contains(assembly);

    private static MetadataReference[] Collect()
    {
        var references = new List<MetadataReference>();

        foreach (var assembly in GetPlatformAssemblies())
        {
            if (!IsReferenced(Path.GetFileNameWithoutExtension(assembly)))
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
    /// lambda could reference then that came to ninety megabytes, of which the
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

    #endregion

}
