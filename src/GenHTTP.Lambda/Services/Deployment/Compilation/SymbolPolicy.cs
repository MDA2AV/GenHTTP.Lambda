using Microsoft.CodeAnalysis;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Decides whether a lambda may use a type or a member of the libraries it
/// is compiled against, by what it is rather than by how it is spelled.
/// </summary>
/// <remarks>
/// The code guard used to read names. A name it had not listed went
/// through: it refused <c>FromFile</c>, and
/// <c>MemoryMappedFile.CreateFromFile("/data/lambda.db")</c> read the
/// platform's database; an alias or a <c>using static</c> was another
/// spelling of a name it did list. Here the compiler says what a name
/// binds to - the type, the overload, the extension method behind it - and
/// that is what is judged:
///
/// <list type="bullet">
/// <item>the namespaces the guard refuses, now also where nothing names them
/// (a member reached through a property), and the types it refuses by name
/// in whatever namespace .NET keeps them;</item>
/// <item>System.IO by an allow-list - streams, readers and writers; every
/// other type of it is a way onto the disk - and the namespaces below it but
/// for the compression of streams and pipelines;</item>
/// <item>every overload that takes the path of a file or directory, of the
/// types that have one beside an overload taking a stream
/// (<see cref="PathTypes" />);</item>
/// <item>a few members on types a lambda needs otherwise: replacing the
/// console, the parts of the server that are the platform's
/// (<see cref="Members" />).</item>
/// </list>
///
/// It sees only the assemblies <see cref="ReferenceProvider" /> hands the
/// compiler. A test lists every member of those that takes a path and
/// fails on one that is neither refused here nor known to be harmless, so a
/// new overload in a new version of a library is a decision, not a hole.
/// </remarks>
public static class SymbolPolicy
{

    #region Rules

    /// <summary>
    /// Types and namespaces inside a refused namespace a lambda reaches
    /// without naming them, and needs.
    /// </summary>
    /// <remarks>
    /// <c>typeof(T).Name</c> is a member of reflection's MemberInfo.
    /// <c>context.Database</c> is Entity Framework's DatabaseFacade - what
    /// a transaction is begun with and raw SQL run through, on the context's
    /// own connection - and the transaction it begins is in Storage;
    /// <c>UseSqlite(connection, sqlite => ...)</c> configures through a
    /// builder of Infrastructure. None of them reaches the services or the
    /// options of a context, which is what Infrastructure is refused for:
    /// <c>GetService</c>, options extensions and another connection stay out.
    /// </remarks>
    private static readonly HashSet<string> Harmless = new(StringComparer.Ordinal)
    {
        "System.Reflection.MemberInfo.Name",
        "Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade",
        "Microsoft.EntityFrameworkCore.Infrastructure.RelationalDbContextOptionsBuilder",
        "Microsoft.EntityFrameworkCore.Infrastructure.SqliteDbContextOptionsBuilder",
        "Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction"
    };

    /// <summary>
    /// The namespaces of Entity Framework a lambda does not reach even where
    /// it does not name them: the services and options of a context, its
    /// connection, its migrations and its internals.
    /// </summary>
    /// <remarks>
    /// The rest - the builders of a bulk update in Query, a model's metadata,
    /// the change tracker - is only refused where it is written out, by the
    /// check by name, as it always was; reached through a call a lambda may
    /// make, it is the call's to answer for. <c>ExecuteUpdate(s =>
    /// s.SetProperty(...))</c> goes through Query, and nothing there opens
    /// anything.
    /// </remarks>
    private static readonly HashSet<string> EntityFrameworkInternals = new(StringComparer.Ordinal)
    {
        "Infrastructure", "Storage", "Internal", "Migrations", "Design", "Scaffolding", "Update"
    };

    /// <summary>
    /// Namespaces refused when a symbol is bound, beside those the guard
    /// refuses where they are written.
    /// </summary>
    private static readonly string[] Namespaces =
    [
        // a resource set or manager reads any file it is pointed at
        "System.Resources",
        // style sheets load files and run script, the resolvers open file:// addresses
        "System.Xml.Xsl",
        "System.Xml.Resolvers",
        // what Resource.FromFile and ResourceTree.FromDirectory build, over any path
        "GenHTTP.Modules.IO.FileSystem",
        // a listener that writes what it hears into any file, and switches for the whole process
        "Microsoft.IdentityModel.Logging",
        // containers of services, and the tools of a designer
        "System.ComponentModel.Design"
    ];

    /// <summary>
    /// The types of System.IO a lambda may use: streams, and what reads and
    /// writes them. Every other one opens, lists, watches or describes the
    /// file system of the host.
    /// </summary>
    private static readonly HashSet<string> Streams = new(StringComparer.Ordinal)
    {
        "Stream", "MemoryStream", "BufferedStream", "TextReader", "TextWriter", "StreamReader", "StreamWriter",
        "StringReader", "StringWriter", "BinaryReader", "BinaryWriter", "SeekOrigin",
        "IOException", "EndOfStreamException", "InvalidDataException", "FileNotFoundException", "DirectoryNotFoundException",
        "PathTooLongException", "FileMode", "FileAccess", "FileShare", "FileOptions", "FileAttributes", "SearchOption",
        "HandleInheritability", "UnixFileMode", "MatchCasing", "MatchType"
    };

    /// <summary>
    /// The namespaces below System.IO a lambda may use.
    /// </summary>
    private static readonly HashSet<string> BelowIO = new(StringComparer.Ordinal)
    {
        "System.IO.Compression", "System.IO.Pipelines"
    };

    /// <summary>
    /// Types refused outright, though their namespace is not.
    /// </summary>
    private static readonly Dictionary<string, string> Types = new(StringComparer.Ordinal)
    {
        ["System.Net.Sockets.UnixDomainSocketEndPoint"] = "the sockets of this machine are off limits, a lambda reaches the network",
        ["System.Xml.XmlUrlResolver"] = "it opens the files a document refers to",
        ["System.Xml.XmlSecureResolver"] = "it opens the files a document refers to",
        ["System.IServiceProvider"] = "the services of the platform are off limits",
        ["System.Runtime.ProfileOptimization"] = "runtime internals are off limits",
        ["System.Runtime.GCSettings"] = "runtime internals are off limits",
        ["System.Runtime.MemoryFailPoint"] = "runtime internals are off limits",
        ["System.Runtime.ControlledExecution"] = "runtime internals are off limits",
        // loads a key through OpenSSL, which reads file: addresses
        ["System.Security.Cryptography.SafeEvpPKeyHandle"] = "certificates and key stores of the host are off limits"
    };

    /// <summary>
    /// Members refused on types a lambda may otherwise use, by the type that
    /// declares them.
    /// </summary>
    /// <remarks>
    /// The console is the platform's: what a lambda writes to it is its log,
    /// and replacing its writer would read every line the server writes.
    /// The server is the platform's. A lambda reads what it is - an
    /// imperative websocket loops while <c>request.Server.Running</c> - and a
    /// handler it writes itself is handed it to pass on to the handlers
    /// inside. Its logging would tap every line the platform logs, request
    /// lines with editor keys among them; its handler, end points and
    /// properties are the platform's. Disposing of it is not refused: that
    /// is <c>IAsyncDisposable</c>'s, and a lambda can bring the process down
    /// in plainer ways - the guard keeps the platform's data from a lambda,
    /// not its time.
    /// </remarks>
    private static readonly Dictionary<string, string> Members = new(StringComparer.Ordinal)
    {
        ["System.Console.SetOut"] = Console,
        ["System.Console.SetError"] = Console,
        ["System.Console.SetIn"] = Console,
        ["System.Console.OpenStandardInput"] = Console,
        ["System.Console.OpenStandardOutput"] = Console,
        ["System.Console.OpenStandardError"] = Console,
        ["GenHTTP.Api.Infrastructure.IServer.Logging"] = Server,
        ["GenHTTP.Api.Infrastructure.IServer.Handler"] = Server,
        ["GenHTTP.Api.Infrastructure.IServer.EndPoints"] = Server,
        ["GenHTTP.Api.Infrastructure.IServer.Properties"] = Server,
        ["GenHTTP.Api.Infrastructure.IServer.StartAsync"] = Server,
        ["Microsoft.Extensions.Logging.ILoggerFactory.AddProvider"] = Server,
        ["System.Xml.XmlResolver.FileSystemResolver"] = "it opens the files a document refers to",
        ["System.Net.Mail.SmtpClient.PickupDirectoryLocation"] = Storage
    };

    private const string Console = "the console is the platform's, Console.WriteLine writes to the lambda's log";

    private const string Server = "the server and its logging are the platform's";

    private const string Storage = "use the Workspace object for storage";

    /// <summary>
    /// Types whose overloads that take a path are refused, while those that
    /// take a stream, a reader or a string of content are not.
    /// </summary>
    private static readonly HashSet<string> PathTypes = new(StringComparer.Ordinal)
    {
        "System.IO.StreamReader", "System.IO.StreamWriter",
        "System.Xml.Linq.XDocument", "System.Xml.Linq.XElement", "System.Xml.Linq.XStreamingElement",
        "System.Xml.XmlReader", "System.Xml.XmlWriter", "System.Xml.XmlDocument", "System.Xml.XmlDataDocument",
        "System.Xml.XmlTextReader", "System.Xml.XmlTextWriter", "System.Xml.XmlValidatingReader",
        "System.Xml.XPath.XPathDocument", "System.Xml.Schema.XmlSchemaSet", "System.Xml.Schema.XmlSchemaCollection",
        "System.Data.DataSet", "System.Data.DataTable",
        "System.Net.Mail.Attachment", "System.Net.Mail.AlternateView", "System.Net.Mail.LinkedResource",
        "System.Net.Sockets.Socket", "System.Net.Sockets.SendPacketsElement",
        "GenHTTP.Modules.Files.Asset", "GenHTTP.Modules.Files.Assets",
        "GenHTTP.Modules.IO.Resource", "GenHTTP.Modules.IO.ResourceTree"
    };

    /// <summary>
    /// What the parameters that take a path are called, in the libraries a
    /// lambda is compiled against.
    /// </summary>
    internal static readonly HashSet<string> PathParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "path", "file", "filePath", "fileName", "directory", "inputUri", "outputFileName", "uri", "url", "schemaUri"
    };

    /// <summary>
    /// Types no member of a lambda's own or of a library may take or return:
    /// what stands for a file or a directory of the host, or an assembly.
    /// </summary>
    private static readonly HashSet<string> Handles = new(StringComparer.Ordinal)
    {
        "System.IO.FileInfo", "System.IO.DirectoryInfo", "System.IO.FileSystemInfo", "System.IO.FileStream",
        "System.Reflection.Assembly", "Microsoft.Win32.SafeHandles.SafeFileHandle"
    };

    #endregion

    #region Functionality

    /// <summary>
    /// Why a lambda may not use the given symbol, or nothing where it may.
    /// </summary>
    /// <param name="symbol">What a name of the lambda was bound to</param>
    /// <param name="own">The assembly of the lambda, whose own symbols are its own business</param>
    public static string? Refuse(ISymbol symbol, IAssemblySymbol? own)
    {
        symbol = Plain(symbol);

        if (symbol is INamespaceSymbol or ILocalSymbol or IParameterSymbol or ILabelSymbol or IRangeVariableSymbol or IDiscardSymbol
            or ITypeParameterSymbol or IPreprocessingSymbol)
        {
            return null;
        }

        if (own != null && SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, own))
        {
            return null;
        }

        if (Harmless.Contains(FullName(symbol)))
        {
            return null;
        }

        if (symbol is not ITypeSymbol && Members.TryGetValue(FullName(symbol), out var member))
        {
            return member;
        }

        // a member the guard refuses by name, whatever it is reached through:
        // LoadExtension, ConnectionString, FromFile, Assembly ...
        if (symbol is not ITypeSymbol && !CodeGuard.IsHarmlessMemberName(symbol.Name) && CodeGuard.ReasonFor(symbol.Name) is { } named)
        {
            return named;
        }

        for (var type = symbol as INamedTypeSymbol ?? symbol.ContainingType; type != null; type = type.ContainingType)
        {
            if (RefuseType(type) is { } reason)
            {
                return reason;
            }
        }

        if (symbol is ITypeSymbol)
        {
            return null;
        }

        if (Handled(symbol) is { } handle)
        {
            return $"'{handle}' is the file system of the host - use the Workspace object for storage";
        }

        if (symbol is IMethodSymbol method && method.ContainingType is { } owner && PathTypes.Contains(FullName(owner))
            && method.Parameters.Any(p => IsText(p.Type) && PathParameters.Contains(p.Name)))
        {
            return $"this overload of {owner.Name}.{(method.MethodKind == MethodKind.Constructor ? "new" : method.Name)} takes a path on the host - use the one that takes a stream or content, and the Workspace object for storage";
        }

        return null;
    }

    /// <summary>
    /// Why a lambda may not use the given type, or nothing where it may.
    /// </summary>
    private static string? RefuseType(INamedTypeSymbol type)
    {
        var name = FullName(type);

        if (Harmless.Contains(name))
        {
            return null;
        }

        if (Types.TryGetValue(name, out var reason))
        {
            return reason;
        }

        if (Handles.Contains(name))
        {
            return Storage;
        }

        var space = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        if (Harmless.Contains(space))
        {
            return null;
        }

        if (space == "System.IO")
        {
            return Streams.Contains(type.Name) ? null : Storage;
        }

        if (space.StartsWith("System.IO.", StringComparison.Ordinal) && !BelowIO.Any(b => space == b || space.StartsWith(b + ".", StringComparison.Ordinal)))
        {
            return Storage;
        }

        if (Namespaces.Any(n => space == n || space.StartsWith(n + ".", StringComparison.Ordinal)))
        {
            return $"'{space}' is not available inside a lambda";
        }

        if (space.StartsWith("Microsoft.EntityFrameworkCore.", StringComparison.Ordinal))
        {
            // the namespaces the guard allows by name, Storage.ValueConversion among them, stay allowed
            return CodeGuard.RefusesNamespace(space) && space.Split('.').Any(EntityFrameworkInternals.Contains)
                ? $"'{space}' is not available inside a lambda"
                : null;
        }

        if (CodeGuard.RefusesNamespace(space))
        {
            return $"'{space}' is not available inside a lambda";
        }

        // what the guard refuses by name is refused in whichever namespace
        // .NET keeps it - Environment, Process, GC, Marshal, Thread ...
        if ((space == "System" || space.StartsWith("System.", StringComparison.Ordinal) || space.StartsWith("Microsoft.", StringComparison.Ordinal))
            && CodeGuard.ReasonFor(type.Name) is { } banned)
        {
            return banned;
        }

        return null;
    }

    /// <summary>
    /// The handle of the file system a member takes or returns, if any.
    /// </summary>
    private static string? Handled(ISymbol symbol)
    {
        IEnumerable<ITypeSymbol> types = symbol switch
        {
            IMethodSymbol method => [method.ReturnType, ..method.Parameters.Select(p => p.Type)],
            IPropertySymbol property => [property.Type, ..property.Parameters.Select(p => p.Type)],
            IFieldSymbol field => [field.Type],
            IEventSymbol @event => [@event.Type],
            _ => []
        };

        foreach (var type in types)
        {
            var element = type is IArrayTypeSymbol array ? array.ElementType : type;

            if (element is INamedTypeSymbol named && Handles.Contains(FullName(named)))
            {
                return named.Name;
            }
        }

        return null;
    }

    /// <summary>
    /// The definition behind what a name was bound to: the extension method
    /// rather than its reduced form, the generic definition rather than the
    /// construction, the target of an alias, the element of an array.
    /// </summary>
    private static ISymbol Plain(ISymbol symbol)
    {
        while (true)
        {
            switch (symbol)
            {
                case IAliasSymbol alias:
                    symbol = alias.Target;
                    continue;
                case IMethodSymbol { ReducedFrom: { } reduced }:
                    symbol = reduced;
                    continue;
                case IArrayTypeSymbol array:
                    symbol = array.ElementType;
                    continue;
                case IPointerTypeSymbol pointer:
                    symbol = pointer.PointedAtType;
                    continue;
            }

            return symbol.OriginalDefinition;
        }
    }

    /// <summary>
    /// The full name of a type or member without its generic arguments,
    /// such as <c>System.Console.SetOut</c>.
    /// </summary>
    internal static string FullName(ISymbol symbol)
    {
        var name = symbol.MetadataName;

        var tick = name.IndexOf('`');

        if (tick >= 0)
        {
            name = name[..tick];
        }

        if (symbol.ContainingType is { } type)
        {
            return $"{FullName(type)}.{name}";
        }

        var space = symbol.ContainingNamespace;

        return space == null || space.IsGlobalNamespace ? name : $"{space.ToDisplayString()}.{name}";
    }

    private static bool IsText(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String
        || type is INamedTypeSymbol { Name: "ReadOnlySpan", TypeArguments: [{ SpecialType: SpecialType.System_Char }] };

    #endregion

}
