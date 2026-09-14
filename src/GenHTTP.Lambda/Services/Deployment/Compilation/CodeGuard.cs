using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Rejects snippets that reach for capabilities a hosted lambda has no business
/// using: the file system of the host, other processes, the network or the
/// reflection APIs that would route around those rules.
/// </summary>
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
        "System.Net.Sockets",
        "System.Net.NetworkInformation",
        "System.Reflection",
        "System.Runtime.CompilerServices",
        "System.Runtime.InteropServices",
        "System.Runtime.Loader",
        "System.Security",
        "Microsoft.CodeAnalysis",
        "Microsoft.Data",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Win32",
        "Evolve",
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
    public static IReadOnlyList<CompilationDiagnostic> Inspect(SyntaxNode root)
    {
        var findings = new List<CompilationDiagnostic>();

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

                case IdentifierNameSyntax identifier when !IsHarmlessMember(identifier):
                    CheckName(identifier.Identifier.ValueText, identifier.GetLocation(), findings);
                    break;

                case GenericNameSyntax generic:
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

    private static void CheckNamespace(string name, Location location, List<CompilationDiagnostic> findings)
    {
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
            "CreateInstance", "CreateDelegate", "DynamicInvoke");

        Add("native interop is disabled", "Marshal", "NativeLibrary", "NativeMemory", "GCHandle", "SafeHandle",
            "DllImport", "DllImportAttribute", "LibraryImport", "LibraryImportAttribute", "UnmanagedCallersOnly");

        Add("runtime internals are off limits", "GC", "RuntimeHelpers", "Thread", "ThreadPool", "Unsafe");

        Add("outbound network access is disabled", "HttpClient", "HttpClientHandler", "HttpMessageInvoker",
            "SocketsHttpHandler", "WebClient", "WebRequest", "HttpWebRequest", "HttpListener", "Socket",
            "TcpClient", "TcpListener", "UdpClient", "Dns", "SmtpClient", "ServicePointManager");

        // these all accept a plain path and would hand out a resource tree pointing
        // anywhere on the host - Listing, StaticWebsite and SinglePageApplication only
        // accept a tree and therefore stay available
        Add("use Workspace.Tree() to serve files", "FromFile", "FromDirectory", "FromWeb", "FromAssembly", "Assets");

        Add("outbound proxying is disabled", "Proxy", "ReverseProxy");

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
