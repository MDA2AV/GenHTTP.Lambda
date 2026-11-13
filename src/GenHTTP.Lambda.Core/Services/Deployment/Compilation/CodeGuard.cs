using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Rejects snippets that reach for capabilities a hosted lambda has no business
/// using: the file system of the host, other processes, or the reflection APIs
/// that would route around those rules - and, unless the lambda may, other
/// servers: HttpClient, sockets, WebSocket clients, mail.
/// </summary>
/// <remarks>
/// Whether a lambda may connect to other servers is its egress, which its
/// tier includes or the operator allowed. Without it, the assemblies that
/// connect are not handed to the compiler at all (see
/// <see cref="ReferenceProvider"/>), which is what holds; the names and
/// namespaces refused here are what makes the refusal say why and what to do,
/// rather than that a type could not be found. What other assemblies do on
/// the code's behalf - XML loaded from an address, the keys of a token's
/// issuer - is asked of the compilation, see <see cref="InspectEgress"/>.
/// </remarks>
/// <remarks>
/// A database is a file, so it is held to the same rule: a lambda uses the
/// connection <c>Database.GetConnection()</c> hands it and makes none of its
/// own - nor points one at another file, nor loads native code into one. The
/// names that would do that are refused here, and making a connection is
/// refused by <see cref="InspectConstruction"/>, which needs the compiler to
/// tell what a target-typed <c>new()</c> makes. Entity Framework Core works on
/// that same connection: its public surface is there, its internals - where
/// a context's services and options could be pointed at another file - are
/// not, and <see cref="InspectEntityFramework"/> refuses a context configured
/// with anything but a connection.
/// </remarks>
/// <remarks>
/// This is governance, not a sandbox - the compiled code still runs in process.
/// The second half of the story is <see cref="ReferenceProvider" />, which simply
/// does not hand the compiler the assemblies that would make most of this reachable.
/// </remarks>
public static class CodeGuard
{

    /// <summary>
    /// Names that must not appear anywhere in a snippet, neither as a type nor
    /// as a member. Grouped by the reason they are on the list.
    /// </summary>
    private static readonly Dictionary<string, string> BannedNames = Build();

    private static readonly HashSet<string> BannedNamespaces = new(StringComparer.Ordinal)
    {
        "System.Diagnostics",
        "System.Reflection",
        "System.Runtime.CompilerServices",
        "System.Runtime.InteropServices",
        "System.Runtime.Loader",
        "System.Security",
        "System.Linq.Expressions",
        "Microsoft.CodeAnalysis",
        "Microsoft.Data",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Win32",
        "SQLitePCL",
        "GenHTTP.Lambda",
        "GenHTTP.Engine",
        "GenHTTP.Modules.DependencyInjection",
        "GenHTTP.Modules.ReverseProxy"
    };

    /// <summary>
    /// The namespaces that are nothing but connecting to other servers, refused
    /// in a lambda that may not.
    /// </summary>
    /// <remarks>
    /// Not System.Net.Sockets or System.Net.Mail as a whole: an address family
    /// and a mail address are in them, which have nothing to connect with.
    /// </remarks>
    private static readonly HashSet<string> EgressNamespaces = new(StringComparer.Ordinal)
    {
        "System.Net.Http",
        "System.Net.Quic",
        "System.Net.NetworkInformation",
        "System.IO.Pipes"
    };

    /// <summary>
    /// The types that connect to other servers, refused as names in a lambda
    /// that may not.
    /// </summary>
    /// <remarks>
    /// As types only: the same name read off an object - a connection's
    /// <c>Ping()</c>, a request's <c>Socket</c> - is whatever that object
    /// calls it, and where it is one of these the compiler refuses it anyway,
    /// since the assembly behind it is not referenced.
    /// </remarks>
    private static readonly HashSet<string> EgressNames = new(StringComparer.Ordinal)
    {
        "HttpClient", "HttpClientHandler", "SocketsHttpHandler", "HttpMessageHandler", "HttpMessageInvoker", "DelegatingHandler",
        "HttpRequestMessage", "HttpResponseMessage", "IHttpClientFactory",
        "WebRequest", "HttpWebRequest", "FtpWebRequest", "WebClient",
        "Socket", "TcpClient", "TcpListener", "UdpClient", "NetworkStream", "ClientWebSocket", "QuicConnection", "QuicListener",
        "Dns", "Ping", "NetworkInterface", "SmtpClient",
        "NamedPipeClientStream", "NamedPipeServerStream", "AnonymousPipeClientStream", "AnonymousPipeServerStream",
        "XmlUrlResolver", "XmlSecureResolver"
    };

    /// <summary>
    /// What a lambda that may not connect to other servers is told, after
    /// what it reached for.
    /// </summary>
    internal const string EgressMessage =
        "which this lambda may not do. Its owner can ask for that - request_egress, PUT /api/v1/lambdas/{privateKey}/egress " +
        "or Egress in the editor - saying what it connects to and why, and the operator of the installation decides; a premium lambda may. " +
        "A public API that browsers may call can be called from the lambda's page instead (platform_guide, egress).";

    /// <summary>
    /// The id of a refusal for want of egress, so the editor and an agent can
    /// tell it from the rest.
    /// </summary>
    public const string EgressId = "LAMBDA0003";

    #region Functionality

    /// <summary>
    /// Whether the given name would be rejected in a snippet. Used to keep the
    /// suggestions of the editor in sync with what the guard allows.
    /// </summary>
    public static bool IsBanned(string name) => BannedNames.ContainsKey(name);

    /// <summary>
    /// Inspects the parsed snippet and returns a message for every rule it breaks.
    /// </summary>
    /// <param name="egress">Whether the lambda may connect to other servers</param>
    public static IReadOnlyList<CompilationDiagnostic> Inspect(SyntaxNode root, IReadOnlySet<string>? declared = null, bool egress = true)
    {
        var findings = new List<CompilationDiagnostic>();

        declared ??= Declared(root);

        foreach (var node in root.DescendantNodesAndSelf())
        {
            switch (node)
            {
                case UsingDirectiveSyntax @using when @using.Name != null:
                    CheckNamespace(@using.Name.ToString(), @using.GetLocation(), findings, egress);
                    break;

                case QualifiedNameSyntax qualified when qualified.Parent is not QualifiedNameSyntax:
                    CheckNamespace(qualified.ToString(), qualified.GetLocation(), findings, egress);
                    break;

                case MemberAccessExpressionSyntax access when access.Parent is not MemberAccessExpressionSyntax && Chain(access) is { } chain:
                    CheckChain(chain, access.GetLocation(), findings, egress);
                    break;

                case IdentifierNameSyntax identifier
                    when !IsHarmlessMember(identifier) && !declared.Contains(identifier.Identifier.ValueText):
                    CheckName(identifier.Identifier.ValueText, identifier.GetLocation(), findings);

                    if (!egress && !IsMember(identifier))
                    {
                        CheckEgress(identifier.Identifier.ValueText, identifier.GetLocation(), findings);
                    }
                    break;

                case GenericNameSyntax generic when !declared.Contains(generic.Identifier.ValueText):
                    CheckName(generic.Identifier.ValueText, generic.GetLocation(), findings);
                    break;
            }
        }

        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.LineDirectiveTrivia) || trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
            {
                findings.Add(Reject("Preprocessor directives that change diagnostics are not allowed.", trivia.GetLocation()));
            }
        }

        return findings;
    }

    /// <summary>
    /// Namespaces inside a banned one that a lambda may use all the same.
    /// </summary>
    /// <remarks>
    /// Only the namespace itself and not what is below it: hashing a password
    /// or making a token is something every lambda with accounts has to do,
    /// while System.Security.Cryptography.X509Certificates reads the stores of
    /// the host. The types in here that reach the host are banned by name.
    /// Microsoft.Data.Sqlite is what a lambda talks to its database with; what
    /// in it would open another database is banned by name as well.
    ///
    /// Entity Framework Core is the namespace a context, its sets and its
    /// queries are written in, and the three below it that describing a
    /// model takes: its builders, a converter for a value SQLite has no type
    /// for, and the entries of the change tracker. Its Infrastructure,
    /// Storage, Internal and the rest stay out - that is where the services
    /// and the options of a context are, and with them another connection.
    /// </remarks>
    private static readonly HashSet<string> AllowedNamespaces = new(StringComparer.Ordinal)
    {
        "System.Security.Cryptography",
        "Microsoft.Data.Sqlite",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.ChangeTracking",
        "Microsoft.EntityFrameworkCore.Metadata.Builders",
        "Microsoft.EntityFrameworkCore.Storage.ValueConversion"
    };

    /// <summary>
    /// The namespaces directly inside an allowed one, by their full names.
    /// </summary>
    /// <remarks>
    /// What follows an allowed namespace in a name is one of its types or a
    /// namespace inside it, and the text does not say which:
    /// <c>Microsoft.EntityFrameworkCore.DbContext</c> is written exactly like
    /// <c>Microsoft.EntityFrameworkCore.Infrastructure</c>. The references a
    /// lambda is compiled against do say, so they are asked once.
    /// </remarks>
    private static readonly Lazy<HashSet<string>> Nested = new(FindNested, LazyThreadSafetyMode.ExecutionAndPublication);

    private static HashSet<string> FindNested()
    {
        var global = CSharpCompilation.Create("namespaces", references: ReferenceProvider.Resolve()).GlobalNamespace;

        var nested = new HashSet<string>(StringComparer.Ordinal);

        foreach (var allowed in AllowedNamespaces)
        {
            INamespaceSymbol? space = global;

            foreach (var part in allowed.Split('.'))
            {
                space = space?.GetNamespaceMembers().FirstOrDefault(n => n.Name == part);
            }

            foreach (var inner in space?.GetNamespaceMembers() ?? [])
            {
                nested.Add($"{allowed}.{inner.Name}");
            }
        }

        return nested;
    }

    private static void CheckNamespace(string name, Location location, List<CompilationDiagnostic> findings, bool egress = true)
    {
        // global::System.Reflection is System.Reflection, and the arguments
        // of a generic type are names of their own, checked where they stand
        name = Plain(name);

        if (AllowedNamespaces.Contains(name) || AllowedNamespaces.Any(a => IsTypeOf(name, a)))
        {
            return;
        }

        foreach (var banned in BannedNamespaces)
        {
            if (name.Equals(banned, StringComparison.Ordinal) || name.StartsWith(banned + ".", StringComparison.Ordinal))
            {
                findings.Add(Reject($"'{Refused(name, banned)}' is not available inside a lambda.", location));
                return;
            }
        }

        if (!egress && EgressNamespace(name) is { } connecting && !NamesConnecting(name))
        {
            findings.Add(Reject($"'{connecting}' connects to other servers, {EgressMessage}", location, EgressId));
        }
    }

    /// <summary>
    /// Whether a name written out in full goes on to a type that is refused by
    /// its own name - <c>System.Net.Http.HttpClient</c> - which says it once.
    /// </summary>
    private static bool NamesConnecting(string name) => name.Split('.').Any(EgressNames.Contains);

    /// <summary>
    /// The namespace that connects to other servers a name is in or starts
    /// with, if it is one.
    /// </summary>
    private static string? EgressNamespace(string name)
        => EgressNamespaces.FirstOrDefault(e => name.Equals(e, StringComparison.Ordinal) || name.StartsWith(e + ".", StringComparison.Ordinal));

    /// <summary>
    /// The namespace to name when refusing one: the banned one, or - below a
    /// namespace that is allowed - the one inside it the name went on into,
    /// so a refused Microsoft.EntityFrameworkCore.Infrastructure does not
    /// read as if Entity Framework were refused altogether.
    /// </summary>
    private static string Refused(string name, string banned)
    {
        var allowed = AllowedNamespaces.Where(a => name.StartsWith(a + ".", StringComparison.Ordinal)).MaxBy(a => a.Length);

        return allowed == null ? banned : $"{allowed}.{name[(allowed.Length + 1)..].Split('.')[0]}";
    }

    /// <summary>
    /// Checks a name written out in full where an expression goes, such as
    /// <c>System.Linq.Expressions.Expression.Constant(1)</c>.
    /// </summary>
    /// <remarks>
    /// A type named in a declaration is a qualified name, which the check
    /// above reads; the same name in an expression is a chain of member
    /// accesses, and a banned namespace written out there used to go through.
    /// A chain is refused when it starts with a banned namespace - unless it
    /// goes on into an allowed one, the banned one itself or one inside it,
    /// and from there into one of its types rather than a namespace below it.
    /// </remarks>
    private static void CheckChain(string chain, Location location, List<CompilationDiagnostic> findings, bool egress = true)
    {
        var banned = BannedNamespaces.Where(b => chain.StartsWith(b + ".", StringComparison.Ordinal)).MaxBy(b => b.Length);

        if (banned == null)
        {
            if (!egress && EgressNamespace(chain) is { } connecting && !NamesConnecting(chain))
            {
                findings.Add(Reject($"'{connecting}' connects to other servers, {EgressMessage}", location, EgressId));
            }

            return;
        }

        var allowed = AllowedNamespaces.Where(a => a.Length >= banned.Length && chain.StartsWith(a + ".", StringComparison.Ordinal)).MaxBy(a => a.Length);

        if (allowed != null && !Nested.Value.Contains($"{allowed}.{chain[(allowed.Length + 1)..].Split('.')[0]}"))
        {
            return;
        }

        findings.Add(Reject($"'{Refused(chain, banned)}' is not available inside a lambda.", location));
    }

    /// <summary>
    /// The names of a chain of member accesses from its start, joined by dots,
    /// or nothing where it does not start with a plain name.
    /// </summary>
    private static string? Chain(MemberAccessExpressionSyntax access)
    {
        var names = new List<string>();

        ExpressionSyntax current = access;

        while (current is MemberAccessExpressionSyntax member)
        {
            names.Add(member.Name.Identifier.ValueText);

            current = member.Expression;
        }

        switch (current)
        {
            case IdentifierNameSyntax start:
                names.Add(start.Identifier.ValueText);
                break;
            case AliasQualifiedNameSyntax global:
                names.Add(global.Name.Identifier.ValueText);
                break;
            default:
                return null;
        }

        names.Reverse();

        return string.Join('.', names);
    }

    /// <summary>
    /// Whether a qualified name is a type directly inside the given namespace
    /// - one segment more, and not a namespace below it.
    /// </summary>
    private static bool IsTypeOf(string name, string space)
    {
        if (!name.StartsWith(space + ".", StringComparison.Ordinal))
        {
            return false;
        }

        var rest = name[(space.Length + 1)..];

        return !rest.Contains('.') && !Nested.Value.Contains(name);
    }

    /// <summary>
    /// A qualified name as the namespaces read it: without <c>global::</c> in
    /// front and without the arguments of a generic type at its end.
    /// </summary>
    private static string Plain(string name)
    {
        if (name.StartsWith("global::", StringComparison.Ordinal))
        {
            name = name["global::".Length..];
        }

        var generic = name.IndexOf('<');

        return generic < 0 ? name : name[..generic];
    }

    /// <summary>
    /// Every name the code declares for itself.
    /// </summary>
    /// <remarks>
    /// A name somebody declares is theirs, whatever it is called. A helper
    /// named File shadows nothing dangerous - a call to it reaches their
    /// method, and the type of the same name is still out of reach through
    /// its namespace, which is checked separately. Refusing it was refusing
    /// somebody their own code because of what it was called.
    /// </remarks>
    public static HashSet<string> Declared(SyntaxNode root)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in root.DescendantNodesAndSelf())
        {
            switch (node)
            {
                case BaseTypeDeclarationSyntax type:
                    names.Add(type.Identifier.ValueText);
                    break;

                case DelegateDeclarationSyntax @delegate:
                    names.Add(@delegate.Identifier.ValueText);
                    break;

                case MethodDeclarationSyntax method:
                    names.Add(method.Identifier.ValueText);
                    break;

                case LocalFunctionStatementSyntax function:
                    names.Add(function.Identifier.ValueText);
                    break;

                case PropertyDeclarationSyntax property:
                    names.Add(property.Identifier.ValueText);
                    break;

                case VariableDeclaratorSyntax variable:
                    names.Add(variable.Identifier.ValueText);
                    break;

                case ParameterSyntax parameter:
                    names.Add(parameter.Identifier.ValueText);
                    break;
            }
        }

        return names;
    }

    /// <summary>
    /// Whether this occurrence of a banned name is harmless: a member read off
    /// some object rather than the type of the same name.
    /// </summary>
    /// <remarks>
    /// This reads what was written rather than what it means, so
    /// <c>request.Header.Path</c> looked exactly like <c>System.IO.Path</c>
    /// and was refused - a name a handler has every reason to ask for.
    ///
    /// Only the names in <see cref="HarmlessMembers" /> are let through this
    /// way, and only where the left of the dot is an expression rather than a
    /// namespace. It cannot be every banned name: <c>Assembly</c> is a type
    /// and also the property that <c>typeof(x).Assembly</c> reaches reflection
    /// through, so letting members past wholesale would open the door this is
    /// here to hold shut.
    /// </remarks>
    private static bool IsHarmlessMember(IdentifierNameSyntax identifier)
    {
        if (!HarmlessMembers.Contains(identifier.Identifier.ValueText))
        {
            return false;
        }

        if (identifier.Parent is not MemberAccessExpressionSyntax access || access.Name != identifier)
        {
            return false;
        }

        var root = access.Expression;

        while (root is MemberAccessExpressionSyntax inner)
        {
            root = inner.Expression;
        }

        // a chain rooted in a namespace is a type being named, not a member
        return root is not IdentifierNameSyntax start || !BannedRoots.Contains(start.Identifier.ValueText);
    }

    /// <summary>
    /// Whether a name is read off an expression - <c>connection.Ping</c> -
    /// rather than naming a type, written alone or after its namespace.
    /// </summary>
    private static bool IsMember(IdentifierNameSyntax identifier)
    {
        if (identifier.Parent is MemberBindingExpressionSyntax)
        {
            return true;
        }

        if (identifier.Parent is not MemberAccessExpressionSyntax access || access.Name != identifier)
        {
            return false;
        }

        var root = access.Expression;

        while (root is MemberAccessExpressionSyntax inner)
        {
            root = inner.Expression;
        }

        return root is not IdentifierNameSyntax start || !BannedRoots.Contains(start.Identifier.ValueText);
    }

    private static void CheckEgress(string name, Location location, List<CompilationDiagnostic> findings)
    {
        if (EgressNames.Contains(name))
        {
            findings.Add(Reject($"'{name}' connects to other servers, {EgressMessage}", location, EgressId));
        }
    }

    private static void CheckName(string name, Location location, List<CompilationDiagnostic> findings)
    {
        if (BannedNames.TryGetValue(name, out var reason))
        {
            findings.Add(Reject($"'{name}' is not available inside a lambda ({reason}).", location));
            return;
        }

        // the generated scaffolding around the snippet uses this prefix
        if (name.StartsWith("__", StringComparison.Ordinal))
        {
            findings.Add(Reject($"'{name}' is reserved, names must not start with a double underscore.", location));
        }
    }

    /// <summary>
    /// Refuses code that makes a database connection of its own: with
    /// <c>new SqliteConnection(...)</c>, with a target-typed <c>new(...)</c>,
    /// or through a class of its own derived from one.
    /// </summary>
    /// <remarks>
    /// Asked of the compilation rather than of the text, because
    /// <c>SqliteConnection connection = new("Data Source=/data/lambda.db");</c>
    /// names no type where the connection is made, and an alias or a derived
    /// class names another one. Only the object creations are bound, and only
    /// in files that have any - the rest is what the compiler binds anyway.
    /// </remarks>
    public static IReadOnlyList<CompilationDiagnostic> InspectConstruction(Microsoft.CodeAnalysis.Compilation compilation)
    {
        var connection = compilation.GetTypeByMetadataName("Microsoft.Data.Sqlite.SqliteConnection");

        if (connection == null)
        {
            return [];
        }

        var findings = new List<CompilationDiagnostic>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            SemanticModel? model = null;

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                INamedTypeSymbol? type = node switch
                {
                    BaseObjectCreationExpressionSyntax creation => (model ??= compilation.GetSemanticModel(tree)).GetTypeInfo(creation).Type as INamedTypeSymbol,
                    ClassDeclarationSyntax { BaseList: not null } declaration => (model ??= compilation.GetSemanticModel(tree)).GetDeclaredSymbol(declaration)?.BaseType,
                    _ => null
                };

                for (var current = type; current != null; current = current.BaseType)
                {
                    if (SymbolEqualityComparer.Default.Equals(current, connection))
                    {
                        var span = node.GetLocation().GetMappedLineSpan();

                        findings.Add(new CompilationDiagnostic("Error", "LAMBDA0001",
                            "A lambda does not make database connections of its own: Database.GetConnection() opens one to the lambda's own database.",
                            span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1, span.HasMappedPath ? span.Path : null));

                        break;
                    }
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// What a lambda is told that configures a context with anything but a connection.
    /// </summary>
    internal const string ContextMessage =
        "A lambda's DbContext works on the connection Database.GetConnection() opens: options.UseSqlite(connection, contextOwnsConnection: true), " +
        "with the connection handed to the context from outside it - new Records(Database.GetConnection()). A connection string would open a database of its own.";

    /// <summary>
    /// What a lambda is told that asks Entity Framework for its schema.
    /// </summary>
    internal const string SchemaMessage =
        "A lambda's schema is SQL migrations shipped in resources/migrations/ and applied by Evolve as it starts, not Entity Framework's EnsureCreated, EnsureDeleted or Migrate: " +
        "add the next file (V2__Add_due_date.sql) and map the context onto the tables it makes.";

    /// <summary>
    /// The names of what is refused about Entity Framework, and so are bound to see whether they are its.
    /// </summary>
    private static readonly HashSet<string> EntityFrameworkNames = new(StringComparer.Ordinal)
    {
        "UseSqlite", "EnsureCreated", "EnsureCreatedAsync", "EnsureDeleted", "EnsureDeletedAsync", "Migrate", "MigrateAsync"
    };

    /// <summary>
    /// Refuses a context that is not on the lambda's connection, and the ways
    /// Entity Framework would make or drop the schema itself.
    /// </summary>
    /// <remarks>
    /// <c>UseSqlite</c> with a connection string, or with nothing and the
    /// string set later, has Entity Framework open a connection of its own -
    /// to whatever file the string names, and without what the platform does
    /// to the connections it hands out. Only the overloads that take a
    /// connection are left, and the only connection a lambda has is its own.
    ///
    /// The schema belongs to Evolve: <c>EnsureDeleted</c> deletes the file
    /// behind the connection, which only the owner does, by switching the
    /// database off; <c>EnsureCreated</c> and <c>Migrate</c> make tables
    /// Evolve knows nothing about, and the next migration then fails on them.
    ///
    /// Asked of the compilation, since Evolve's own <c>Migrate()</c> is the
    /// very thing a lambda is meant to call - only Entity Framework's is
    /// refused. Only the names in question are bound.
    /// </remarks>
    public static IReadOnlyList<CompilationDiagnostic> InspectEntityFramework(Microsoft.CodeAnalysis.Compilation compilation)
    {
        var connection = compilation.GetTypeByMetadataName("System.Data.Common.DbConnection");

        var findings = new List<CompilationDiagnostic>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            SemanticModel? model = null;

            foreach (var name in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
            {
                if (!EntityFrameworkNames.Contains(name.Identifier.ValueText))
                {
                    continue;
                }

                // what does not bind does not compile either, and says so itself
                if ((model ??= compilation.GetSemanticModel(tree)).GetSymbolInfo(name).Symbol is not IMethodSymbol method
                    || method.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) != true)
                {
                    continue;
                }

                var message = method.Name == "UseSqlite"
                    ? method.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, connection)) ? null : ContextMessage
                    : SchemaMessage;

                if (message != null)
                {
                    var span = name.GetLocation().GetMappedLineSpan();

                    findings.Add(new CompilationDiagnostic("Error", "LAMBDA0001", message,
                                                           span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1,
                                                           span.HasMappedPath ? span.Path : null));
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// The names of what fetches from another server on the code's behalf,
    /// and so are bound to see whether they do.
    /// </summary>
    private static readonly HashSet<string> FetchingNames = new(StringComparer.Ordinal)
    {
        "Create", "Load", "Transform", "Add", "ReadXml", "ReadXmlSchema", "Issuer", "FromWeb"
    };

    /// <summary>
    /// The types that connect to other servers and live in an assembly a
    /// lambda is handed all the same, for what else is in it.
    /// </summary>
    private static readonly HashSet<string> ConnectingTypes = new(StringComparer.Ordinal)
    {
        "System.Net.Mail.SmtpClient", "System.Xml.XmlUrlResolver", "System.Xml.XmlSecureResolver"
    };

    /// <summary>
    /// Refuses, in a lambda that may not connect to other servers, what would
    /// connect on its behalf: XML read from an address, the keys of a token's
    /// issuer, a resource on the web, a mail client.
    /// </summary>
    /// <remarks>
    /// The assemblies that connect are not referenced by such a lambda (see
    /// <see cref="ReferenceProvider"/>), but others it is handed connect
    /// through them as they run: <c>XDocument.Load("https://...")</c> fetches
    /// what it is given, and so does an <c>XmlTextReader</c> made with an
    /// address, a <c>DataSet</c> reading XML from one, and the bearer
    /// authentication of GenHTTP asking the issuer of a token for its keys.
    /// The same members with a stream or a string of XML are left alone.
    ///
    /// Asked of the compilation, because only the overload a call binds to
    /// says whether it is given an address - and a type made with a
    /// target-typed <c>new()</c> names nothing. Only the names in question
    /// and the creations are bound.
    /// </remarks>
    public static IReadOnlyList<CompilationDiagnostic> InspectEgress(Microsoft.CodeAnalysis.Compilation compilation)
    {
        var findings = new List<CompilationDiagnostic>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            SemanticModel? model = null;

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                var (method, at) = node switch
                {
                    BaseObjectCreationExpressionSyntax creation => ((model ??= compilation.GetSemanticModel(tree)).GetSymbolInfo(creation).Symbol as IMethodSymbol, creation),
                    SimpleNameSyntax name when FetchingNames.Contains(name.Identifier.ValueText)
                        => ((model ??= compilation.GetSemanticModel(tree)).GetSymbolInfo(name).Symbol as IMethodSymbol, (SyntaxNode)name),
                    _ => (null, node)
                };

                if (method == null || Fetches(method) is not { } what)
                {
                    continue;
                }

                var span = at.GetLocation().GetMappedLineSpan();

                findings.Add(new CompilationDiagnostic("Error", EgressId, $"{what}, {EgressMessage}",
                                                       span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1,
                                                       span.HasMappedPath ? span.Path : null));
            }
        }

        return findings;
    }

    /// <summary>
    /// What a member does that connects to another server, or nothing where it does not.
    /// </summary>
    private static string? Fetches(IMethodSymbol method)
    {
        var type = method.ContainingType;

        var space = type?.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        if (type != null && ConnectingTypes.Contains(type.ToDisplayString()))
        {
            return $"'{type.Name}' connects to other servers";
        }

        // what reads XML takes a path, which is an address as well
        if ((space.StartsWith("System.Xml", StringComparison.Ordinal) || space == "System.Data")
            && method.Parameters.Any(p => p.Type.SpecialType == SpecialType.System_String && IsAddress(p.Name)))
        {
            var name = method.MethodKind == MethodKind.Constructor ? $"new {type!.Name}" : $"{type!.Name}.{method.Name}";

            return $"'{name}' with an address fetches what it names from where it is, another server included";
        }

        if (method.Name == "Issuer" && space.StartsWith("GenHTTP.Modules.Authentication", StringComparison.Ordinal))
        {
            return "'Issuer' has the keys of the tokens fetched from the issuer, another server";
        }

        if (method.Name == "FromWeb" && space.StartsWith("GenHTTP.Modules.IO", StringComparison.Ordinal))
        {
            return "'FromWeb' fetches the resource from another server";
        }

        return null;

        // the address of what is read, not the base a relative one would be
        // resolved against, nor the name of a namespace
        static bool IsAddress(string parameter)
            => (parameter.Contains("uri", StringComparison.OrdinalIgnoreCase)
             || parameter.Contains("url", StringComparison.OrdinalIgnoreCase)
             || parameter.Equals("filename", StringComparison.OrdinalIgnoreCase))
            && !parameter.StartsWith("base", StringComparison.OrdinalIgnoreCase)
            && !parameter.Contains("namespace", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether the code reaches for what connects to other servers, as far as
    /// what is written tells - for the overview of a version, not a check.
    /// </summary>
    /// <remarks>
    /// The names and namespaces <see cref="Inspect"/> refuses where a lambda
    /// may not connect, read the same way: in what is written, the names a
    /// lambda declares for itself left alone, a comment or a string ignored.
    /// </remarks>
    public static bool Connects(IEnumerable<SyntaxNode> roots)
    {
        var all = roots.ToList();

        var declared = new HashSet<string>(StringComparer.Ordinal);

        foreach (var root in all)
        {
            declared.UnionWith(Declared(root));
        }

        return all.Any(root => Inspect(root, declared, egress: false).Any(f => f.Id == EgressId));
    }

    /// <summary>
    /// Whether the code of a version reaches for what connects to other
    /// servers - lambda.cs read as the script it is, the other C# files as the
    /// types they hold, the rest of the files not at all.
    /// </summary>
    public static bool Connects(IEnumerable<LambdaFile> files)
    {
        var compiled = files.Where(f => f.IsCompiled).ToList();

        return compiled.Count > 0
            && Connects(compiled.Select((f, i) => (i == 0 ? SourceBuilder.ParseSnippet(f.Code) : SourceBuilder.ParseFile(f.Code, f.Name)).GetRoot()));
    }

    /// <summary>
    /// What a lambda is told that waits for a task instead of awaiting it.
    /// </summary>
    internal const string WaitingMessage =
        "A lambda does not wait for a task (.Result, .Wait(), .GetAwaiter().GetResult(), Task.WaitAll, Task.WaitAny): " +
        "requests are served on one thread per core, and the task has to finish on the very thread that would be waiting for it - " +
        "so it never does, and every request on that thread waits with it. Await the task instead and make the method async " +
        "(GenHTTP handlers and resource methods may return Task<T> or ValueTask<T>), or call the synchronous method where there is one.";

    /// <summary>
    /// What a lambda is told that takes a semaphore synchronously.
    /// </summary>
    internal const string SemaphoreMessage =
        "A lambda takes a SemaphoreSlim with 'await semaphore.WaitAsync()', not with Wait(): Wait() holds the thread that serves " +
        "this core's requests, and the request that would release the semaphore may be waiting to run on that same thread. " +
        "For a short section without any await in it, a plain lock statement does the job.";

    /// <summary>
    /// The names that may be a wait for a task, and so are bound to see whether they are.
    /// </summary>
    private static readonly HashSet<string> Waits = new(StringComparer.Ordinal) { "Result", "Wait", "WaitAll", "WaitAny", "GetResult" };

    /// <summary>
    /// Refuses code that blocks the thread it runs on until a task has
    /// finished: <c>.Result</c>, <c>.Wait()</c>, <c>.GetAwaiter().GetResult()</c>,
    /// <c>Task.WaitAll</c> and <c>Task.WaitAny</c> - and a semaphore taken with
    /// <c>Wait()</c>.
    /// </summary>
    /// <remarks>
    /// Not a matter of style here. On the ioxide engine a request runs on a
    /// reactor, one thread per core, and the continuations of what it awaits
    /// are posted back to that thread. A request that blocks it waiting for a
    /// task waits for work queued behind itself: that core never serves
    /// anything again, the lambda's requests and everybody else's alike. On
    /// Kestrel it merely costs a thread, which is why such code works on a
    /// developer's machine and must not get further than this.
    ///
    /// Asked of the compilation, like <see cref="InspectConstruction"/>, so a
    /// property of somebody's own type that happens to be called Result is
    /// theirs to read. Only names that could be a wait are bound.
    /// </remarks>
    public static IReadOnlyList<CompilationDiagnostic> InspectWaiting(Microsoft.CodeAnalysis.Compilation compilation)
    {
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var valueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var semaphore = compilation.GetTypeByMetadataName("System.Threading.SemaphoreSlim");

        var findings = new List<CompilationDiagnostic>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            SemanticModel? model = null;

            foreach (var name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (!Waits.Contains(name.Identifier.ValueText) || name.Parent is not (MemberAccessExpressionSyntax or MemberBindingExpressionSyntax))
                {
                    continue;
                }

                var symbol = (model ??= compilation.GetSemanticModel(tree)).GetSymbolInfo(name).Symbol;

                var message = symbol switch
                {
                    IPropertySymbol { Name: "Result" } property when IsTask(property.ContainingType) => WaitingMessage,
                    IMethodSymbol { Name: "Wait" or "WaitAll" or "WaitAny" } method when IsTask(method.ContainingType) => WaitingMessage,
                    IMethodSymbol { Name: "GetResult" } method when IsAwaiter(method.ContainingType) => WaitingMessage,
                    IMethodSymbol { Name: "Wait" } method when SymbolEqualityComparer.Default.Equals(method.ContainingType, semaphore) => SemaphoreMessage,
                    _ => null
                };

                if (message != null)
                {
                    var span = name.GetLocation().GetMappedLineSpan();

                    findings.Add(new CompilationDiagnostic("Error", "LAMBDA0001", message,
                                                           span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1,
                                                           span.HasMappedPath ? span.Path : null));
                }
            }
        }

        return findings;

        bool IsTask(INamedTypeSymbol? type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                var definition = current.OriginalDefinition;

                if (SymbolEqualityComparer.Default.Equals(definition, task) || SymbolEqualityComparer.Default.Equals(definition, valueTask)
                    || definition.ToDisplayString() is "System.Threading.Tasks.Task<TResult>" or "System.Threading.Tasks.ValueTask<TResult>")
                {
                    return true;
                }
            }

            return false;
        }

        // the awaiters of tasks, configured or not, which is where GetResult blocks
        static bool IsAwaiter(INamedTypeSymbol? type)
            => type != null
            && type.Name.EndsWith("Awaiter", StringComparison.Ordinal)
            && type.ContainingNamespace?.ToDisplayString() == "System.Runtime.CompilerServices";
    }

    private static CompilationDiagnostic Reject(string message, Location location, string id = "LAMBDA0001")
    {
        var position = location.GetLineSpan().StartLinePosition;

        return new CompilationDiagnostic("Error", id, message, position.Line + 1, position.Character + 1);
    }

    /// <summary>
    /// The namespaces a banned type can be reached through, so a name written
    /// after one of them is still read as that type.
    /// </summary>
    private static readonly HashSet<string> BannedRoots = new(StringComparer.Ordinal)
    {
        "System", "Microsoft", "global"
    };

    /// <summary>
    /// Banned names that are types only, and so mean nothing as a member.
    /// </summary>
    /// <remarks>
    /// Kept short on purpose, and grown only when something a lambda has a
    /// real reason to write turns out to be refused. Every entry here is a
    /// name that cannot reach anything on its own: reading <c>.Path</c> off an
    /// object gives whatever that object calls its path.
    /// </remarks>
    private static readonly HashSet<string> HarmlessMembers = new(StringComparer.Ordinal)
    {
        "Path"
    };

    private static Dictionary<string, string> Build()
    {
        var banned = new Dictionary<string, string>(StringComparer.Ordinal);

        Add("use the Workspace object for storage", "File", "FileInfo", "Directory", "DirectoryInfo", "DriveInfo",
            "Path", "FileStream", "FileSystemInfo", "FileSystemWatcher", "IsolatedStorageFile");

        Add("processes and the host environment are off limits", "Process", "ProcessStartInfo", "Environment",
            "AppDomain", "AppContext", "Registry", "EventLog", "Debugger");

        Add("reflection is disabled", "Assembly", "AssemblyName", "AssemblyLoadContext", "Activator", "BindingFlags",
            "MethodInfo", "FieldInfo", "PropertyInfo", "ConstructorInfo", "MemberInfo", "TypeInfo", "Module",
            "GetType", "GetTypeInfo", "GetMethod", "GetMethods", "GetField", "GetFields", "GetProperty",
            "GetProperties", "GetConstructor", "GetConstructors", "GetMember", "GetMembers", "InvokeMember",
            "CreateInstance", "CreateDelegate", "DynamicInvoke", "TypeDescriptor");

        // the connection Database.GetConnection() hands out is the only one: these
        // would make another, point one at another file, or load native code into it
        Add("use Database.GetConnection() for the lambda's own database", "ConnectionString", "SqliteConnectionStringBuilder",
            "SqliteFactory", "DbProviderFactories", "DbProviderFactory", "DbDataSource", "CreateDataSource", "LoadExtension",
            "EnableExtensions", "ClearPool", "ClearAllPools", "GetConnectionString", "SetConnectionString");

        // dynamic binds members at runtime, by the type an object turns out to
        // have - which is everything this guard reads the code for, skipped
        Add("binding at runtime goes around what a lambda may name", "dynamic");

        Add("native interop is disabled", "Marshal", "NativeLibrary", "NativeMemory", "GCHandle", "SafeHandle",
            "DllImport", "DllImportAttribute", "LibraryImport", "LibraryImportAttribute", "UnmanagedCallersOnly");

        Add("runtime internals are off limits", "GC", "RuntimeHelpers", "Thread", "ThreadPool", "Unsafe");

        // these all accept a plain path and would hand out a resource tree pointing
        // anywhere on the host - Listing, StaticWebsite and SinglePageApplication only
        // accept a tree and therefore stay available
        // Assets is no longer among these: a lambda ships its own resources
        // now, and Resources.Tree() is the thing it reaches them with
        Add("use Workspace.Tree() or Resources.Tree() to serve files", "FromFile", "FromDirectory", "FromWeb", "FromAssembly");

        Add("outbound proxying is disabled", "Proxy", "ReverseProxy");

        // cryptography is available, its ways into the certificates and key
        // containers of the host are not - several take a path or a store
        Add("certificates and key stores of the host are off limits", "X509Store", "X509Certificate", "X509Certificate2",
            "X509Certificate2Collection", "X509CertificateLoader", "X509Chain", "CspParameters", "CngKey", "CngProvider");

        return banned;

        void Add(string reason, params string[] names)
        {
            foreach (var name in names)
            {
                banned[name] = reason;
            }
        }
    }

    #endregion

}
