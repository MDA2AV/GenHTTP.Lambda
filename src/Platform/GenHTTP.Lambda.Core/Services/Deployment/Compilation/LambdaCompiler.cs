using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;

using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Compiles a snippet into an assembly and pulls the handler out of it.
/// </summary>
/// <remarks>
/// The assembly is written to disk and loaded into the default context on
/// purpose: GenHTTP compiles invocation code for the handlers it is given at
/// runtime, and that generated code can only reference assemblies that are
/// loadable by name and have a file behind them. A collectible context would
/// be unloadable but invisible to it, so lambdas stay loaded for the lifetime
/// of the process - identical code is recognized and reused rather than
/// compiled twice.
/// </remarks>
internal static class LambdaCompiler
{
    private static readonly ConcurrentDictionary<string, LoadedLambda> Loaded = [];

    private static readonly CSharpCompilationOptions CompilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            allowUnsafe: false,
            concurrentBuild: false,
            deterministic: false,
            nullableContextOptions: NullableContextOptions.Disable
        )
        .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
        {
            // "async method lacks await" - most snippets are plain synchronous code
            ["CS1998"] = ReportDiagnostic.Suppress,
            // "assuming assembly reference ... matches identity": a library built
            // against an older framework than the one it runs on - Microsoft.Data.Sqlite
            // on .NET 11 - which the SDK silences in every project for the same reason
            ["CS1701"] = ReportDiagnostic.Suppress,
            ["CS1702"] = ReportDiagnostic.Suppress
        });

    #region Functionality

    /// <summary>
    /// Compiles the snippet and, unless <see cref="CompilationRequest.Run" /> is
    /// disabled, loads and invokes it to obtain the handler.
    /// </summary>
    /// <param name="request">What to compile and where to put it</param>
    internal static async ValueTask<(CompilationOutcome Outcome, IHandler? Handler)> CompileAsync(CompilationRequest request)
    {
        var code = request.Files.Where(f => f.IsCompiled).ToList();

        var snippet = SourceBuilder.ParseSnippet(code[0].Code);

        // the snippet is script, the rest are ordinary C# holding types
        var others = code.Skip(1)
                            .Select(f => (File: f, Tree: SourceBuilder.ParseFile(f.Code, f.Name)))
                            .ToList();

        var syntaxErrors = Translate(snippet.GetDiagnostics());

        foreach (var other in others)
        {
            syntaxErrors.AddRange(Translate(other.Tree.GetDiagnostics()));
        }

        if (syntaxErrors.Count > 0)
        {
            return (CompilationOutcome.Failed(syntaxErrors), null);
        }

        // what the code declares is gathered across all of its files first: a
        // type declared in one and used in another is still the author's own
        var roots = new List<SyntaxNode> { await snippet.GetRootAsync() };

        foreach (var other in others)
        {
            roots.Add(await other.Tree.GetRootAsync());
        }

        var declared = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in roots)
        {
            declared.UnionWith(CodeGuard.Declared(node));
        }

        var rejections = new List<CompilationDiagnostic>();

        // every file is the user's, so every file is inspected - a guard that
        // only looked at the snippet would be avoided by moving the code
        foreach (var node in roots)
        {
            rejections.AddRange(CodeGuard.Inspect(node, declared, request.Settings.Connectivity));
        }

        if (rejections.Count > 0)
        {
            return (CompilationOutcome.Failed(rejections), null);
        }

        if (request.Run && Loaded.TryGetValue(Identify(request), out var known))
        {
            // the very same code is already in the process, just build it again
            return (CompilationOutcome.Succeeded(), await InvokeAsync(known, request));
        }

        var scope = $"Lambda_{request.Name}_{Guid.NewGuid():N}";

        var trees = new List<SyntaxTree> { SourceBuilder.Wrap(snippet, request.Resources, scope, request.Binding, request.Settings.Workspace) };

        foreach (var other in others)
        {
            trees.Add(SourceBuilder.WrapFile(other.Tree, scope, other.File.Name, request.Binding));
        }

        // a lambda that may not connect to other servers is not handed the
        // assemblies that would, so it cannot even name what is in them
        var compilation = CSharpCompilation.Create(
            scope,
            trees,
            ReferenceProvider.Resolve(request.Settings.Connectivity, request.Binding),
            CompilationOptions
        );

        // what needs the compiler to tell: a connection made without naming
        // its type, a task waited for rather than awaited, a context of
        // Entity Framework that is not on the lambda's connection - and,
        // where it may not connect, what would fetch from another server on
        // its behalf
        List<CompilationDiagnostic> constructed = [.. CodeGuard.InspectConstruction(compilation), .. CodeGuard.InspectWaiting(compilation),
                                                   .. CodeGuard.InspectEntityFramework(compilation),
                                                   .. (request.Settings.Connectivity ? [] : CodeGuard.InspectConnectivity(compilation))];

        if (constructed.Count > 0)
        {
            return (CompilationOutcome.Failed(constructed), null);
        }

        if (!request.Run)
        {
            using var check = new MemoryStream();

            var probe = compilation.Emit(check);

            return (probe.Success ? CompilationOutcome.Succeeded(Translate(probe.Diagnostics, DiagnosticSeverity.Warning))
                                  : CompilationOutcome.Failed(Translate(probe.Diagnostics)), null);
        }

        Directory.CreateDirectory(request.AssemblyDirectory);

        var file = Path.Combine(request.AssemblyDirectory, $"{scope}.dll");

        var emitted = compilation.Emit(file);

        if (!emitted.Success)
        {
            TryDelete(file);

            return (CompilationOutcome.Failed(Translate(emitted.Diagnostics)), null);
        }

        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);

        var lambda = new LoadedLambda(assembly, scope);

        Loaded[Identify(request)] = lambda;

        return (CompilationOutcome.Succeeded(Translate(emitted.Diagnostics, DiagnosticSeverity.Warning)), await InvokeAsync(lambda, request));
    }

    /// <summary>
    /// Runs the entry point of a compiled lambda and normalizes whatever it
    /// returned. Called on every deployment, so redeploying resets the state a
    /// snippet holds in memory.
    /// </summary>
    /// <remarks>
    /// The secrets, the database and the workspace are connected first,
    /// because the top of a snippet is where an API key is most often read, a
    /// database migrated and a workspace prepared.
    /// </remarks>
    private static async ValueTask<IHandler> InvokeAsync(LoadedLambda lambda, CompilationRequest request)
    {
        if (request.Secrets is { } secrets)
        {
            Connect(lambda, SourceBuilder.SecretType, secrets.Read);
            Connect(lambda, SourceBuilder.SecretType, secrets.ReadAsync, SourceBuilder.AsyncSource);
        }

        if (request.Database is { } database)
        {
            Connect(lambda, SourceBuilder.DatabaseType, database.Connect);
            Connect(lambda, SourceBuilder.DatabaseType, database.ConnectAsync, SourceBuilder.AsyncSource);
        }

        if (request.Context != null)
        {
            Connect(lambda, SourceBuilder.ScopeType, request.Context, SourceBuilder.ContextSource);
        }

        if (request.Workspace is { } files)
        {
            Connect(lambda, SourceBuilder.WorkspaceType, files.Exists, SourceBuilder.WorkspaceFields.Exists);
            Connect(lambda, SourceBuilder.WorkspaceType, files.Read, SourceBuilder.WorkspaceFields.Read);
            Connect(lambda, SourceBuilder.WorkspaceType, files.Write, SourceBuilder.WorkspaceFields.Write);
            Connect(lambda, SourceBuilder.WorkspaceType, files.Delete, SourceBuilder.WorkspaceFields.Delete);
            Connect(lambda, SourceBuilder.WorkspaceType, files.List, SourceBuilder.WorkspaceFields.List);
            Connect(lambda, SourceBuilder.WorkspaceType, files.Folders, SourceBuilder.WorkspaceFields.Folders);
            Connect(lambda, SourceBuilder.WorkspaceType, files.CreateFolder, SourceBuilder.WorkspaceFields.CreateFolder);
            Connect(lambda, SourceBuilder.WorkspaceType, files.Tree, SourceBuilder.WorkspaceFields.Tree);
            Connect(lambda, SourceBuilder.WorkspaceType, files.ExistsAsync, SourceBuilder.WorkspaceFields.ExistsAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.ReadAsync, SourceBuilder.WorkspaceFields.ReadAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.WriteAsync, SourceBuilder.WorkspaceFields.WriteAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.DeleteAsync, SourceBuilder.WorkspaceFields.DeleteAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.ListAsync, SourceBuilder.WorkspaceFields.ListAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.FoldersAsync, SourceBuilder.WorkspaceFields.FoldersAsync);
            Connect(lambda, SourceBuilder.WorkspaceType, files.CreateFolderAsync, SourceBuilder.WorkspaceFields.CreateFolderAsync);
        }

        var entry = lambda.Assembly.GetType($"{lambda.Scope}.{SourceBuilder.EntryType}")
                 ?? throw new InvalidOperationException("The compiled lambda does not contain an entry point.");

        var method = entry.GetMethod(SourceBuilder.EntryMethod, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                  ?? throw new InvalidOperationException("The compiled lambda does not contain an entry point.");

        var invocation = (Task<object>?)method.Invoke(null, null);

        var result = invocation == null ? null : await invocation;

        return result switch
        {
            IHandler handler => handler,
            IHandlerBuilder builder => builder.Build(),
            null => throw new InvalidOperationException("The lambda returned null instead of a handler."),
            _ => throw new InvalidOperationException($"The lambda returned '{result.GetType().Name}', which is neither an IHandler nor an IHandlerBuilder.")
        };
    }

    /// <summary>
    /// Hands a generated class the function it reads through.
    /// </summary>
    private static void Connect(LoadedLambda lambda, string type, object source, string field = SourceBuilder.SecretSource)
        => lambda.Assembly.GetType($"{lambda.Scope}.{type}")?
                 .GetField(field, BindingFlags.Static | BindingFlags.NonPublic)?
                 .SetValue(null, source);

    /// <summary>
    /// Identifies a snippet by what it compiles to, so redeploying unchanged
    /// code does not add another assembly to the process.
    /// </summary>
    /// <remarks>
    /// The directory of the resources, the settings and what the database is
    /// declared as are part of it because they are compiled in: the same code
    /// with its workspace switched off, with or without connectivity, or as
    /// the preview of a feature, is another assembly - in another tier it is
    /// not, since the room of its data is read as it runs, nor for another
    /// workspace, which it reaches through the platform. Beyond those it is
    /// the C# files and nothing else. The resources are read from their directory
    /// while the lambda runs - only where that directory is gets compiled in -
    /// and the rest of the code is never built, so a version that only
    /// changes those builds its handler from the assembly already loaded.
    /// Compiling it again would cost seconds and add an assembly that, loaded
    /// into the default context, stays for the life of the process.
    ///
    /// Hashed piece by piece rather than joined first, so a large file is not
    /// copied only to be hashed.
    /// </remarks>
    private static string Identify(CompilationRequest request)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        Append(hash, request.Resources);
        Append(hash, $"\n{request.Binding.Connection.FullName}\n{string.Join(',', request.Binding.Namespaces)}");
        Append(hash, $"\n{request.Settings.Workspace}\n{request.Settings.Connectivity}");

        foreach (var file in request.Files.Where(f => f.IsCompiled))
        {
            Append(hash, "\n");
            Append(hash, file.Name);
            Append(hash, "\n");
            Append(hash, file.Code);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static void Append(IncrementalHash hash, string text) => hash.AppendData(MemoryMarshal.AsBytes(text.AsSpan()));

    private static void TryDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception)
        {
            // a leftover artifact is cleaned up on the next start
        }
    }

    /// <summary>
    /// Maps compiler messages back onto the snippet the user wrote.
    /// </summary>
    private static List<CompilationDiagnostic> Translate(IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        var result = new List<CompilationDiagnostic>();

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity != severity)
            {
                continue;
            }

            var span = diagnostic.Location.GetMappedLineSpan();

            // a mapped path is one of the user's files; anything else came out
            // of the generated wrapper, or is about no file at all - a
            // reference, say - and has no line worth pointing at
            var written = span.Path is { } path && path != SourceBuilder.GeneratedFile
                       && !path.EndsWith(".generated.cs", StringComparison.Ordinal);

            result.Add(new CompilationDiagnostic(
                severity == DiagnosticSeverity.Error ? "Error" : "Warning",
                diagnostic.Id,
                Describe(diagnostic),
                written ? span.StartLinePosition.Line + 1 : 0,
                written ? span.StartLinePosition.Character + 1 : 0,
                written ? (span.Path!.Length > 0 ? span.Path : SourceBuilder.UserFile) : null
            ));
        }

        return result;
    }

    private static string Describe(Diagnostic diagnostic) => diagnostic.Id switch
    {
        // raised on the generated entry point, so the raw message would point nowhere
        "CS0161" => "The code must end with a return statement that returns a handler.",
        // the top level of lambda.cs is read as a script, where a using declaration
        // is read as a using directive - and the raw message says nothing about that
        "CS1002" when IsUsingDeclaration(diagnostic) => diagnostic.GetMessage()
            + ". A using declaration (using var ...) cannot stand at the top level of lambda.cs: write await using (var connection = await Database.GetConnectionAsync()) { ... } there, or declare it inside the method or route that needs it.",
        // outside the top-level code of lambda.cs, Assets is the type the Files
        // module declares under that name, and the raw message says nothing about why
        "CS0117" when MeantTheLambdaAssets(diagnostic) => diagnostic.GetMessage()
            + ". Outside the top-level code of lambda.cs, Assets is the Files module's type; what the lambda shipped is Resources.",
        // inside a DbContext, Database is the context's own, and the raw
        // message says nothing about the lambda's of the same name
        "CS1061" or "CS0120" when MeantTheLambdaDatabase(diagnostic) => diagnostic.GetMessage()
            + ". Inside a DbContext, Database is the context's own: open the connection where the context is made and hand it in - new Records(await Database.GetConnectionAsync()) - or write LambdaEnvironment.Database.GetConnectionAsync().",
        _ => diagnostic.GetMessage()
    };

    /// <summary>
    /// Whether a diagnostic stands on a line that declares something with
    /// <c>using var</c>.
    /// </summary>
    private static bool IsUsingDeclaration(Diagnostic diagnostic)
    {
        if (diagnostic.Location.SourceTree is not { } tree)
        {
            return false;
        }

        var line = tree.GetText().Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start).ToString().TrimStart();

        return line.StartsWith("using var ", StringComparison.Ordinal) || line.StartsWith("await using var ", StringComparison.Ordinal);
    }

    /// <summary>
    /// What the lambda's own resources offer, which the Files module's type
    /// called Assets - their name before - does not.
    /// </summary>
    private static readonly string[] AssetMembers = ["Root", "Exists", "ReadText", "ReadBytes", "List", "Tree", "Files", "App", "Folders"];

    private static bool MeantTheLambdaAssets(Diagnostic diagnostic)
    {
        var message = diagnostic.GetMessage();

        return message.StartsWith("'Assets' does not contain a definition for '", StringComparison.Ordinal)
            && AssetMembers.Any(member => message.EndsWith($"'{member}'", StringComparison.Ordinal));
    }

    /// <summary>
    /// Whether a diagnostic is about reaching the lambda's database through
    /// the property of a context that shares its name.
    /// </summary>
    private static bool MeantTheLambdaDatabase(Diagnostic diagnostic)
    {
        var message = diagnostic.GetMessage();

        return message.StartsWith("'DatabaseFacade' does not contain a definition for 'GetConnection'", StringComparison.Ordinal)
            || message.Contains("'DbContext.Database'", StringComparison.Ordinal);
    }

    private sealed record LoadedLambda(Assembly Assembly, string Scope);

    #endregion

}
