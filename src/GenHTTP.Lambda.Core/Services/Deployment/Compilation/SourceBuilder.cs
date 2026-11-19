using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Data;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Turns the snippet a user wrote into a compilable file.
/// </summary>
/// <remarks>
/// The snippet is parsed as a script, which lets people mix statements with
/// record and class declarations. Statements move into the body of the entry
/// method, declared types move next to it, and <c>#line</c> directives keep
/// every compiler message pointing at the line the user actually wrote.
/// Everything lands in a namespace of its own: GenHTTP generates invocation
/// code that refers to the types of a handler by their full name, and two
/// lambdas both declaring a <c>Book</c> must not collide there.
/// </remarks>
internal static class SourceBuilder
{

    internal const string UserFile = "lambda.cs";

    internal const string GeneratedFile = "generated.cs";

    internal const string EntryType = "__GenHttpLambda";

    internal const string EntryMethod = "BuildAsync";

    internal const string WorkspaceType = "__LambdaWorkspace";

    internal const string ResourceType = "__LambdaResources";

    internal const string SecretType = "__LambdaSecrets";

    internal const string DatabaseType = "__LambdaDatabase";

    /// <summary>
    /// The field of the generated secrets and database classes the platform
    /// puts the function they read with into, once the lambda is loaded.
    /// </summary>
    internal const string SecretSource = "_source";

    /// <summary>
    /// The field of the generated scope the platform puts what configures a
    /// context of Entity Framework on the lambda's connection into, once the
    /// lambda is loaded - what <c>UseDatabase</c> calls.
    /// </summary>
    internal const string ContextSource = "_context";

    /// <summary>
    /// The fields of the generated workspace class the platform puts the
    /// functions it works through into, once the lambda is loaded - one per
    /// thing its code can do with its files, as <see cref="Services.Workspace.WorkspaceBinding"/> has them.
    /// </summary>
    internal static class WorkspaceFields
    {
        internal const string Exists = "_exists";
        internal const string Read = "_read";
        internal const string Write = "_write";
        internal const string Delete = "_delete";
        internal const string List = "_list";
        internal const string Folders = "_folders";
        internal const string CreateFolder = "_createFolder";
        internal const string Tree = "_tree";
    }

    /// <summary>
    /// Holds what every file of a lambda may name without qualifying it.
    /// </summary>
    /// <remarks>
    /// Imported into every file with a using static, so Workspace means the
    /// same thing in a type in Store.cs as it does in the top-level code of
    /// lambda.cs - it used to exist only there, and the first thing an agent
    /// writes is a store class in a file of its own that calls Workspace and
    /// fails to compile. Resources is here too; Assets, its name before, stays
    /// out: GenHTTP.Modules.Files, which every lambda imports, has a type of
    /// that name, and a using static member and an imported type sharing a
    /// name is an ambiguity the compiler refuses.
    ///
    /// A type of the author's own of one of these names is found before them,
    /// since what a namespace declares comes before what is imported into it:
    /// code written while the platform did not have the name goes on meaning
    /// what it meant, and its author reaches the platform's through
    /// <c>LambdaEnvironment</c>.
    /// </remarks>
    internal const string ScopeType = "__LambdaScope";

    internal static CSharpParseOptions ScriptOptions { get; } = new(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Script);

    internal static CSharpParseOptions RegularOptions { get; } = new(LanguageVersion.Latest, DocumentationMode.None);

    #region Functionality

    /// <summary>
    /// Parses the snippet the way the user wrote it, so diagnostics and the
    /// code guard can work on the original line numbers.
    /// </summary>
    internal static SyntaxTree ParseSnippet(string code) => CSharpSyntaxTree.ParseText(code, ScriptOptions, UserFile);

    /// <summary>
    /// Parses a file that is not the snippet: ordinary C#, holding types.
    /// </summary>
    internal static SyntaxTree ParseFile(string code, string name) => CSharpSyntaxTree.ParseText(code, RegularOptions, name);

    /// <summary>
    /// Puts one of the other files into the namespace the snippet lives in, so
    /// its types are reachable from the snippet without a using.
    /// </summary>
    /// <remarks>
    /// The imports every lambda gets are repeated here rather than made global:
    /// a global using would have to be emitted once, and each file is its own
    /// compilation unit so there is no single place that is. Line directives
    /// carry the file's own name, which is what a diagnostic then reports.
    /// </remarks>
    internal static SyntaxTree WrapFile(SyntaxTree file, string scope, string name, DatabaseBinding binding)
    {
        var root = (CompilationUnitSyntax)file.GetRoot();

        var text = file.GetText();

        var builder = new StringBuilder();

        foreach (var import in ModuleCatalog.ImportsFor(binding))
        {
            builder.AppendLine($"using {import};");
        }

        foreach (var import in root.Usings)
        {
            builder.AppendLine(import.NormalizeWhitespace().ToFullString());
        }

        builder.AppendLine($"using static {scope}.{ScopeType};");

        builder.AppendLine();
        builder.AppendLine($"namespace {scope};");
        builder.AppendLine();

        foreach (var member in root.Members)
        {
            AppendFrom(builder, text, member, name);
        }

        builder.AppendLine("#line default");

        return CSharpSyntaxTree.ParseText(builder.ToString(), RegularOptions, $"{name}.generated.cs");
    }

    /// <summary>
    /// Wraps the parsed snippet into the file that is handed to the compiler.
    /// </summary>
    /// <param name="snippet">The parsed snippet of the user</param>
    /// <param name="resources">The directory this lambda's resources were written to</param>
    /// <param name="scope">The namespace everything generated for this lambda lives in</param>
    /// <param name="binding">How the lambda is bound to its database, which its database integration decides</param>
    /// <param name="enabled">
    /// Whether the lambda has a workspace, which its owner decides. Left out
    /// where nothing is going to run, such as for the editor's questions.
    /// </param>
    internal static SyntaxTree Wrap(SyntaxTree snippet, string resources, string scope, DatabaseBinding binding, bool enabled = true)
    {
        var root = (CompilationUnitSyntax)snippet.GetRoot();

        var text = snippet.GetText();

        var statements = new List<MemberDeclarationSyntax>();
        var types = new List<MemberDeclarationSyntax>();

        foreach (var member in root.Members)
        {
            // types live next to the entry point, everything else (statements,
            // script level fields and methods) becomes part of its body
            (member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax ? types : statements).Add(member);
        }

        var builder = new StringBuilder();

        foreach (var import in ModuleCatalog.ImportsFor(binding))
        {
            builder.AppendLine($"using {import};");
        }

        foreach (var import in root.Usings)
        {
            builder.AppendLine(import.NormalizeWhitespace().ToFullString());
        }

        // types declared in lambda.cs sit outside the entry class, so they
        // need the import as much as the other files do
        builder.AppendLine($"using static {scope}.{ScopeType};");

        builder.AppendLine();
        builder.AppendLine($"namespace {scope};");
        builder.AppendLine();
        builder.AppendLine("internal static class LambdaEnvironment");
        builder.AppendLine("{");
        builder.AppendLine($"    internal static readonly {WorkspaceType} Workspace = new {WorkspaceType}();");
        builder.AppendLine($"    internal static readonly {ResourceType} Resources = new {ResourceType}({Literal(resources)});");
        // what they were called before, which code written then still says
        builder.AppendLine($"    internal static {ResourceType} Assets => Resources;");
        builder.AppendLine($"    internal static readonly {SecretType} Secret = new {SecretType}();");
        builder.AppendLine($"    internal static readonly {DatabaseType} Database = new {DatabaseType}();");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"internal static class {ScopeType}");
        builder.AppendLine("{");
        builder.AppendLine($"    internal static {WorkspaceType} Workspace => LambdaEnvironment.Workspace;");
        builder.AppendLine($"    internal static {SecretType} Secret => LambdaEnvironment.Secret;");
        // not members of the entry class like the two before them, only here:
        // a class of the author's own called Database or Resources is common
        // enough, and should mean the same thing in lambda.cs as in every
        // other file - and the same as it did before the platform had either
        builder.AppendLine($"    internal static {DatabaseType} Database => LambdaEnvironment.Database;");
        builder.AppendLine($"    internal static {ResourceType} Resources => LambdaEnvironment.Resources;");
        builder.AppendLine();
        builder.AppendLine(ContextSourceCode);
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"internal static class {EntryType}");
        builder.AppendLine("{");
        builder.AppendLine($"    private static {WorkspaceType} Workspace => LambdaEnvironment.Workspace;");
        builder.AppendLine($"    private static {ResourceType} Assets => LambdaEnvironment.Resources;");
        builder.AppendLine($"    private static {SecretType} Secret => LambdaEnvironment.Secret;");
        builder.AppendLine();
        builder.AppendLine($"    internal static async global::System.Threading.Tasks.Task<object> {EntryMethod}()");
        builder.AppendLine("    {");

        foreach (var member in statements)
        {
            Append(builder, text, member);
        }

        builder.AppendLine("#line default");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        foreach (var member in types)
        {
            AppendType(builder, text, member);
        }

        builder.AppendLine("#line default");
        builder.AppendLine();
        builder.AppendLine(WorkspaceSource(enabled));
        builder.AppendLine(ResourceSource);
        builder.AppendLine(SecretSourceCode);
        builder.AppendLine(DatabaseSourceCode(binding));

        return CSharpSyntaxTree.ParseText(builder.ToString(), RegularOptions, GeneratedFile);
    }

    /// <summary>
    /// Copies one member over verbatim, prefixed by the line directive that
    /// maps it back onto the snippet.
    /// </summary>
    private static void Append(StringBuilder builder, SourceText text, SyntaxNode member)
        => Append(builder, text, member.Span, text.ToString(member.Span), UserFile);

    /// <summary>
    /// Copies a member of one of the other files over, mapped onto that file.
    /// </summary>
    private static void AppendFrom(StringBuilder builder, SourceText text, SyntaxNode member, string file)
        => Append(builder, text, member.Span, text.ToString(member.Span), file);

    private static void Append(StringBuilder builder, SourceText text, TextSpan span, string content, string file)
    {
        var start = text.Lines.GetLinePosition(span.Start);

        builder.AppendLine($"#line {start.Line + 1} \"{file}\"");

        // the span starts at the first token, so the indentation of the first
        // line has to be restored to keep the columns of the messages correct
        builder.Append(' ', start.Character);
        builder.AppendLine(content);
    }

    /// <summary>
    /// Copies a declared type over, made public on the way.
    /// </summary>
    /// <remarks>
    /// GenHTTP generates invocation code into an assembly of its own, so every
    /// type that shows up in the signature of a handler has to be visible from
    /// the outside. The keyword goes on a line of its own ahead of the line
    /// directive, and a narrower one that is already there is blanked out
    /// rather than removed - both so that the positions the compiler reports
    /// keep matching the snippet character for character.
    /// </remarks>
    private static void AppendType(StringBuilder builder, SourceText text, MemberDeclarationSyntax member)
    {
        var accessibility = member.Modifiers.Where(IsAccessibility).ToList();

        if (accessibility.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            Append(builder, text, member);
            return;
        }

        builder.AppendLine("public");

        var span = member.Span;

        var content = text.ToString(span).ToCharArray();

        foreach (var modifier in accessibility)
        {
            for (var i = 0; i < modifier.Span.Length; i++)
            {
                content[modifier.SpanStart - span.Start + i] = ' ';
            }
        }

        Append(builder, text, span, new string(content), UserFile);
    }

    private static bool IsAccessibility(SyntaxToken token) => token.Kind() is SyntaxKind.PublicKeyword
        or SyntaxKind.InternalKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.FileKeyword;

    private static string Literal(string value) => SyntaxFactory.Literal(value).ToFullString();

    #endregion

    #region Workspace

    /// <summary>
    /// The resources a lambda shipped, as it can read them back.
    /// </summary>
    /// <remarks>
    /// Read only, and rewritten from the version being deployed every time one
    /// goes online. It is the other half of the workspace: this is what came
    /// with the code, that is what the code has written since.
    /// </remarks>
    private static readonly string ResourceSource = $$"""
        internal sealed class {{ResourceType}}
        {
            private readonly string _root;

            internal {{ResourceType}}(string root)
            {
                _root = global::System.IO.Path.TrimEndingDirectorySeparator(global::System.IO.Path.GetFullPath(root))
                      + global::System.IO.Path.DirectorySeparatorChar;

                global::System.IO.Directory.CreateDirectory(_root);
            }

            /// <summary>The absolute path the resources were written to.</summary>
            public string Root => _root;

            /// <summary>Whether a resource was shipped under this name.</summary>
            public bool Exists(string name) => global::System.IO.File.Exists(Resolve(name));

            /// <summary>Reads a resource as UTF-8 text.</summary>
            public string ReadText(string name) => global::System.IO.File.ReadAllText(Resolve(name));

            /// <summary>Reads a resource as bytes.</summary>
            public byte[] ReadBytes(string name) => global::System.IO.File.ReadAllBytes(Resolve(name));

            /// <summary>Every resource that was shipped, relative to the root.</summary>
            public string[] List()
            {
                if (!global::System.IO.Directory.Exists(_root))
                {
                    return new string[0];
                }

                var files = global::System.IO.Directory.GetFiles(_root, "*", global::System.IO.SearchOption.AllDirectories);

                var result = new string[files.Length];

                for (var i = 0; i < files.Length; i++)
                {
                    result[i] = files[i].Substring(_root.Length).Replace('\\', '/');
                }

                return result;
            }

            /// <summary>The resources as a resource tree, ready to be served.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree()
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(_root).Build();

            /// <summary>One folder of the resources as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree(string folder)
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(Folder(folder)).Build();

            /// <summary>A handler that serves the resources as files.</summary>
            /// <remarks>
            /// Served from the directory rather than through a tree, which on the
            /// ioxide engine is its own file handler: descriptors opened once and
            /// read off the ring. The resources of a build do not change while it
            /// is served - a deployment writes them before it builds the handler -
            /// so the directory is never walked again to see whether they did.
            /// </remarks>
            public global::GenHTTP.Modules.Files.Multi.FileAssetsBuilder Files()
                => global::GenHTTP.Modules.Files.Assets.From(_root).RefreshInterval(global::System.Threading.Timeout.InfiniteTimeSpan);

            /// <summary>A handler that serves one folder of the resources as files.</summary>
            public global::GenHTTP.Modules.Files.Multi.FileAssetsBuilder Files(string folder)
                => global::GenHTTP.Modules.Files.Assets.From(Folder(folder)).RefreshInterval(global::System.Threading.Timeout.InfiniteTimeSpan);

            /// <summary>
            /// A single page application over the resources: index.html is the
            /// shell, and a path that matches no file is answered with it.
            /// </summary>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App()
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree()).ServerSideRouting();

            /// <summary>
            /// A single page application over one folder of the resources.
            /// </summary>
            /// <remarks>
            /// So that a front end can be a folder of files added the same way
            /// every other file is, and served by naming it, rather than
            /// having to be the whole of what the lambda ships.
            /// </remarks>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App(string folder)
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree(folder)).ServerSideRouting();

            /// <summary>The folders resources were shipped in, relative to the root.</summary>
            public string[] Folders()
            {
                if (!global::System.IO.Directory.Exists(_root))
                {
                    return new string[0];
                }

                var found = global::System.IO.Directory.GetDirectories(_root, "*", global::System.IO.SearchOption.AllDirectories);

                var result = new string[found.Length];

                for (var i = 0; i < found.Length; i++)
                {
                    result[i] = found[i].Substring(_root.Length).Replace('\\', '/');
                }

                return result;
            }

            private string Folder(string name)
            {
                var resolved = Resolve(name);

                if (!global::System.IO.Directory.Exists(resolved))
                {
                    var had = Folders();

                    var known = had.Length > 0
                              ? "The folders this lambda ships are: " + string.Join(", ", had) + "."
                              : "This lambda ships no folders - a resource has to be named like 'resources/site/index.html' to be in one.";

                    throw new global::System.InvalidOperationException(
                        "There is no folder called '" + name + "' in the resources. " + known);
                }

                return resolved;
            }

            private string Resolve(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new global::System.ArgumentException("The name of a resource must not be empty.", "name");
                }

                var resolved = global::System.IO.Path.GetFullPath(global::System.IO.Path.Combine(_root, name));

                if (!resolved.StartsWith(_root, global::System.StringComparison.Ordinal))
                {
                    throw new global::System.ArgumentException("A resource must be inside the directory of the resources.", "name");
                }

                return resolved;
            }
        }
        """;

    /// <summary>
    /// The secrets of the lambda, as its code reads them: <c>Secret.Read("STRIPE_KEY")</c>.
    /// </summary>
    /// <remarks>
    /// The values are not compiled in. The class holds a function the
    /// platform hands it once the lambda is loaded, which reads the current
    /// value on every call - so a secret changed in the editor is what the
    /// next request reads, and no value ends up in an assembly on the disk.
    /// The field is private: code of the lambda that names it does not compile,
    /// and reaching it any other way needs reflection, which the guard refuses.
    ///
    /// Exists answers false while secrets are switched off rather than
    /// throwing, so code that works with and without a value works in either
    /// case; Read throws with what to do, which is what the log then says.
    /// </remarks>
    private static readonly string SecretSourceCode = $$"""
        internal sealed class {{SecretType}}
        {
            private static global::System.Func<string, bool, string> {{SecretSource}} = null;

            /// <summary>
            /// The value of a secret. Throws, saying how to set it, when there
            /// is none of that name.
            /// </summary>
            public string Read(string name) => Source()(name, true);

            /// <summary>Whether a secret of that name is set.</summary>
            public bool Exists(string name) => Source()(name, false) != null;

            private static global::System.Func<string, bool, string> Source()
                => {{SecretSource}} ?? throw new global::System.InvalidOperationException("The secrets of this lambda are not available here.");
        }
        """;

    /// <summary>
    /// The database of the lambda, as its code connects to it:
    /// <c>using var connection = Database.GetConnection();</c>, or
    /// <c>using var db = new Records(Database.GetConnection());</c> for a
    /// context of Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Like the secrets, the class holds a function the platform hands it once
    /// the lambda is loaded, and the code never sees where the database is.
    /// Each call opens a connection of its own, taken from a pool, which the
    /// caller disposes of - one per request, not one shared between requests.
    ///
    /// Declared to return what the database integration says (<see cref="DatabaseBinding.Connection"/>):
    /// a <c>DbConnection</c>, which every database has, or the connection of
    /// the integration's own library where code written before names it.
    ///
    /// Throws while the database is switched off, with what to do about it,
    /// which is what the log then says.
    /// </remarks>
    private static string DatabaseSourceCode(DatabaseBinding binding) => $$"""
        internal sealed class {{DatabaseType}}
        {
            private static global::System.Func<global::System.Data.Common.DbConnection> {{SecretSource}} = null;

            /// <summary>
            /// An open connection to the database of this lambda, for plain SQL
            /// or for a DbContext of Entity Framework Core (options.UseDatabase(connection)).
            /// Dispose of it when done - it goes back to a pool, so one per request
            /// costs nothing. Use it synchronously: ToList and SaveChanges, not
            /// their Async forms.
            /// </summary>
            public global::{{binding.Connection.FullName}} GetConnection() => (global::{{binding.Connection.FullName}})Source()();

            private static global::System.Func<global::System.Data.Common.DbConnection> Source()
                => {{SecretSource}} ?? throw new global::System.InvalidOperationException("The database of this lambda is not available here.");
        }
        """;

    /// <summary>
    /// What a context of Entity Framework Core is configured with in a lambda:
    /// <c>options.UseDatabase(connection)</c> in its <c>OnConfiguring</c>.
    /// </summary>
    /// <remarks>
    /// An extension of the generated scope, which every file imports, so it is
    /// found inside a context - where <c>Database</c> is the context's own. It
    /// calls what the database integration configures a context with, handed
    /// over once the lambda is loaded, so the code names no database and runs
    /// on whichever keeps it. The context owns the connection, and disposes of
    /// it with itself.
    /// </remarks>
    private const string ContextSourceCode = $$"""
            private static global::System.Action<global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder, global::System.Data.Common.DbConnection> {{ContextSource}} = null;

            /// <summary>
            /// Configures a DbContext of Entity Framework Core on the connection
            /// Database.GetConnection() opened, which the context then owns.
            /// </summary>
            internal static global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseDatabase(this global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder options,
                                                                                                    global::System.Data.Common.DbConnection connection)
            {
                Context()(options, connection);
                return options;
            }

            /// <summary>
            /// Configures a DbContext of Entity Framework Core on the connection
            /// Database.GetConnection() opened, which the context then owns.
            /// </summary>
            internal static global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TContext> UseDatabase<TContext>(this global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TContext> options,
                                                                                                                       global::System.Data.Common.DbConnection connection)
                where TContext : global::Microsoft.EntityFrameworkCore.DbContext
            {
                Context()(options, connection);
                return options;
            }

            private static global::System.Action<global::Microsoft.EntityFrameworkCore.DbContextOptionsBuilder, global::System.Data.Common.DbConnection> Context()
                => {{ContextSource}} ?? throw new global::System.InvalidOperationException("The database of this lambda is not available here.");
        """;

    /// <summary>
    /// The only door a lambda has to files it keeps: its workspace, handed to
    /// the snippet as <c>Workspace</c>. Generated into the lambda rather than
    /// referenced, so the assembly of this application stays invisible to the
    /// code being compiled.
    /// </summary>
    /// <remarks>
    /// Like the secrets, the class holds functions the platform hands it once
    /// the lambda is loaded - one for each thing it does - and does nothing
    /// with files itself: the platform resolves the names, holds a write to
    /// the room of the lambda's data and asks the files integration, the same
    /// way for what the lambda writes and what its owner puts there (see
    /// <c>LambdaWorkspace</c>). So the code is the same whatever keeps its
    /// files, and never learns where they are. Without the platform's
    /// functions - nothing is running - there is nothing to read or write.
    ///
    /// A workspace its owner switched off is still the same class, so code
    /// that names it compiles, but every member refuses with the reason and
    /// the way to switch it on - which is what the stack trace in the log then
    /// says. Written as two bodies of one method rather than a flag to test,
    /// so the compiler has no unreachable branch to warn the author about.
    /// </remarks>
    private static string WorkspaceSource(bool enabled) => $$"""
        internal sealed class {{WorkspaceType}}
        {
            private static global::System.Func<string, bool> {{WorkspaceFields.Exists}} = null;

            private static global::System.Func<string, byte[]> {{WorkspaceFields.Read}} = null;

            private static global::System.Action<string, byte[]> {{WorkspaceFields.Write}} = null;

            private static global::System.Action<string> {{WorkspaceFields.Delete}} = null;

            private static global::System.Func<string[]> {{WorkspaceFields.List}} = null;

            private static global::System.Func<string[]> {{WorkspaceFields.Folders}} = null;

            private static global::System.Action<string> {{WorkspaceFields.CreateFolder}} = null;

            private static global::System.Func<string, global::GenHTTP.Api.Content.IO.IResourceTree> {{WorkspaceFields.Tree}} = null;

            {{(enabled
                ? "private static void Guard() { }"
                : $"private static void Guard() => throw new global::System.InvalidOperationException({Literal(DataKinds.WorkspaceOff)});")}}

            /// <summary>Checks whether the given file exists.</summary>
            public bool Exists(string name)
            {
                Guard();
                return Port({{WorkspaceFields.Exists}})(name);
            }

            /// <summary>Reads a file as UTF-8 text.</summary>
            public string ReadText(string name)
            {
                var bytes = ReadBytes(name);

                using var reader = new global::System.IO.StreamReader(new global::System.IO.MemoryStream(bytes), global::System.Text.Encoding.UTF8, true);

                return reader.ReadToEnd();
            }

            /// <summary>Reads a file as bytes.</summary>
            public byte[] ReadBytes(string name)
            {
                Guard();
                return Port({{WorkspaceFields.Read}})(name);
            }

            /// <summary>Writes UTF-8 text into a file, replacing it if it exists.</summary>
            public void WriteText(string name, string content)
                => WriteBytes(name, content == null ? new byte[0] : global::System.Text.Encoding.UTF8.GetBytes(content));

            /// <summary>Writes bytes into a file, replacing it if it exists.</summary>
            public void WriteBytes(string name, byte[] content)
            {
                Guard();
                Port({{WorkspaceFields.Write}})(name, content);
            }

            /// <summary>Removes a file, if it exists.</summary>
            public void Delete(string name)
            {
                Guard();
                Port({{WorkspaceFields.Delete}})(name);
            }

            /// <summary>Lists the files in this workspace, relative to its root.</summary>
            public string[] List()
            {
                Guard();
                return Port({{WorkspaceFields.List}})();
            }

            /// <summary>Provides this workspace as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree()
            {
                Guard();
                return Port({{WorkspaceFields.Tree}})(null);
            }

            /// <summary>One folder of this workspace as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree(string folder)
            {
                Guard();
                return Port({{WorkspaceFields.Tree}})(folder ?? throw new global::System.ArgumentException("The name of a workspace file must not be empty.", "name"));
            }

            /// <summary>
            /// A single page application over this workspace.
            /// </summary>
            /// <remarks>
            /// The same thing Resources.App does, over the other files. Which
            /// one a front end belongs in is a real choice rather than a
            /// detail: shipped with the code it is versioned, travels with a
            /// clone and is replaced wholesale on every deploy; here it is
            /// uploaded once, outlives every deployment, and a deploy never
            /// touches it. A site that is part of the program wants the first.
            /// A site somebody uploads and changes without redeploying wants
            /// this one.
            /// </remarks>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App()
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree()).ServerSideRouting();

            /// <summary>A single page application over one folder of this workspace.</summary>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App(string folder)
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree(folder)).ServerSideRouting();

            /// <summary>A handler that serves one folder of this workspace.</summary>
            public global::GenHTTP.Modules.Files.Multi.TreeAssetsBuilder Files(string folder)
                => global::GenHTTP.Modules.Files.Assets.From(Tree(folder));

            /// <summary>Makes a folder, so files can be written into it.</summary>
            public void CreateFolder(string name)
            {
                Guard();
                Port({{WorkspaceFields.CreateFolder}})(name);
            }

            /// <summary>The folders of this workspace, relative to its root.</summary>
            public string[] Folders()
            {
                Guard();
                return Port({{WorkspaceFields.Folders}})();
            }

            /// <summary>Creates a handler that serves the files of this workspace.</summary>
            public global::GenHTTP.Modules.Files.Multi.TreeAssetsBuilder Files()
                => global::GenHTTP.Modules.Files.Assets.From(Tree());

            private static T Port<T>(T function) where T : class
                => function ?? throw new global::System.InvalidOperationException("The workspace of this lambda is not available here.");
        }
        """;

    #endregion

}
