using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Rejects snippets that reach for capabilities a hosted lambda has no business
/// using: the file system of the host, other processes, or the reflection APIs
/// that would route around those rules. Outbound network access is not among
/// them - a lambda may use HttpClient and sockets directly.
/// </summary>
/// <remarks>
/// A database is a file, so it is held to the same rule: a lambda uses the
/// connection <c>Database.GetConnection()</c> hands it and makes none of its
/// own - nor points one at another file, nor loads native code into one. The
/// names that would do that are refused here, and making a connection is
/// refused by <see cref="InspectConstruction"/>, which needs the compiler to
/// tell what a target-typed <c>new()</c> makes.
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

    #region Functionality

    /// <summary>
    /// Whether the given name would be rejected in a snippet. Used to keep the
    /// suggestions of the editor in sync with what the guard allows.
    /// </summary>
    public static bool IsBanned(string name) => BannedNames.ContainsKey(name);

    /// <summary>
    /// Inspects the parsed snippet and returns a message for every rule it breaks.
    /// </summary>
    public static IReadOnlyList<CompilationDiagnostic> Inspect(SyntaxNode root, IReadOnlySet<string>? declared = null)
    {
        var findings = new List<CompilationDiagnostic>();

        declared ??= Declared(root);

        foreach (var node in root.DescendantNodesAndSelf())
        {
            switch (node)
            {
                case UsingDirectiveSyntax @using when @using.Name != null:
                    CheckNamespace(@using.Name.ToString(), @using.GetLocation(), findings);
                    break;

                case QualifiedNameSyntax qualified when qualified.Parent is not QualifiedNameSyntax:
                    CheckNamespace(qualified.ToString(), qualified.GetLocation(), findings);
                    break;

                case MemberAccessExpressionSyntax access when access.Parent is not MemberAccessExpressionSyntax && Chain(access) is { } chain:
                    CheckChain(chain, access.GetLocation(), findings);
                    break;

                case IdentifierNameSyntax identifier
                    when !IsHarmlessMember(identifier) && !declared.Contains(identifier.Identifier.ValueText):
                    CheckName(identifier.Identifier.ValueText, identifier.GetLocation(), findings);
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
    /// </remarks>
    private static readonly HashSet<string> AllowedNamespaces = new(StringComparer.Ordinal)
    {
        "System.Security.Cryptography",
        "Microsoft.Data.Sqlite"
    };

    private static void CheckNamespace(string name, Location location, List<CompilationDiagnostic> findings)
    {
        if (AllowedNamespaces.Contains(name) || AllowedNamespaces.Any(a => IsTypeOf(name, a)))
        {
            return;
        }

        foreach (var banned in BannedNamespaces)
        {
            if (name.Equals(banned, StringComparison.Ordinal) || name.StartsWith(banned + ".", StringComparison.Ordinal))
            {
                findings.Add(Reject($"'{banned}' is not available inside a lambda.", location));
                return;
            }
        }
    }

    /// <summary>
    /// Checks a name written out in full where an expression goes, such as
    /// <c>System.Linq.Expressions.Expression.Constant(1)</c>.
    /// </summary>
    /// <remarks>
    /// A type named in a declaration is a qualified name, which the check
    /// above reads; the same name in an expression is a chain of member
    /// accesses, and a banned namespace written out there used to go through.
    /// Where the namespace ends and the type begins is not written anywhere,
    /// so a chain is refused when it starts with a banned namespace - unless
    /// it goes on into an allowed one inside it, and from there into one of
    /// its types rather than into the certificates below it.
    /// </remarks>
    private static void CheckChain(string chain, Location location, List<CompilationDiagnostic> findings)
    {
        var banned = BannedNamespaces.Where(b => chain.StartsWith(b + ".", StringComparison.Ordinal)).MaxBy(b => b.Length);

        if (banned == null)
        {
            return;
        }

        var allowed = AllowedNamespaces.Where(a => a.Length > banned.Length && chain.StartsWith(a + ".", StringComparison.Ordinal)).MaxBy(a => a.Length);

        if (allowed != null && !chain[(allowed.Length + 1)..].StartsWith("X509Certificates", StringComparison.Ordinal))
        {
            return;
        }

        findings.Add(Reject($"'{banned}' is not available inside a lambda.", location));
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
    /// - one segment more, and not the certificates namespace below it.
    /// </summary>
    private static bool IsTypeOf(string name, string space)
    {
        if (!name.StartsWith(space + ".", StringComparison.Ordinal))
        {
            return false;
        }

        var rest = name[(space.Length + 1)..];

        return !rest.Contains('.') && rest != "X509Certificates";
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

    private static CompilationDiagnostic Reject(string message, Location location)
    {
        var position = location.GetLineSpan().StartLinePosition;

        return new CompilationDiagnostic("Error", "LAMBDA0001", message, position.Line + 1, position.Character + 1);
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
            "EnableExtensions", "ClearPool", "ClearAllPools");

        Add("native interop is disabled", "Marshal", "NativeLibrary", "NativeMemory", "GCHandle", "SafeHandle",
            "DllImport", "DllImportAttribute", "LibraryImport", "LibraryImportAttribute", "UnmanagedCallersOnly");

        Add("runtime internals are off limits", "GC", "RuntimeHelpers", "Thread", "ThreadPool", "Unsafe");

        // these all accept a plain path and would hand out a resource tree pointing
        // anywhere on the host - Listing, StaticWebsite and SinglePageApplication only
        // accept a tree and therefore stay available
        // Assets is no longer among these: a lambda ships its own now, and
        // Assets.Tree() is the thing it reaches them with
        Add("use Workspace.Tree() or Assets.Tree() to serve files", "FromFile", "FromDirectory", "FromWeb", "FromAssembly");

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
