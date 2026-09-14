using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Lambda.Services.Workspace;

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

    internal const string AssetType = "__LambdaAssets";

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
    internal static SyntaxTree WrapFile(SyntaxTree file, string scope, string name)
    {
        var root = (CompilationUnitSyntax)file.GetRoot();

        var text = file.GetText();

        var builder = new StringBuilder();

        foreach (var import in ModuleCatalog.Imports)
        {
            builder.AppendLine($"using {import};");
        }

        foreach (var import in root.Usings)
        {
            builder.AppendLine(import.NormalizeWhitespace().ToFullString());
        }

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
    /// <param name="workspace">The directory this lambda may read and write</param>
    /// <param name="scope">The namespace everything generated for this lambda lives in</param>
    internal static SyntaxTree Wrap(SyntaxTree snippet, string workspace, string assets, string scope)
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

        foreach (var import in ModuleCatalog.Imports)
        {
            builder.AppendLine($"using {import};");
        }

        foreach (var import in root.Usings)
        {
            builder.AppendLine(import.NormalizeWhitespace().ToFullString());
        }

        builder.AppendLine();
        builder.AppendLine($"namespace {scope};");
        builder.AppendLine();
        builder.AppendLine("internal static class LambdaEnvironment");
        builder.AppendLine("{");
        builder.AppendLine($"    internal static readonly {WorkspaceType} Workspace = new {WorkspaceType}({Literal(workspace)});");
        builder.AppendLine($"    internal static readonly {AssetType} Assets = new {AssetType}({Literal(assets)});");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine($"internal static class {EntryType}");
        builder.AppendLine("{");
        builder.AppendLine($"    private static {WorkspaceType} Workspace => LambdaEnvironment.Workspace;");
        builder.AppendLine($"    private static {AssetType} Assets => LambdaEnvironment.Assets;");
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
        builder.AppendLine(WorkspaceSource);
        builder.AppendLine(AssetSource);

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
    /// The only door a lambda has to the file system: a private directory,
    /// handed to the snippet as <c>Workspace</c>. Generated into the lambda
    /// rather than referenced, so the assembly of this application stays
    /// invisible to the code being compiled.
    /// </summary>
    /// <summary>
    /// What a lambda shipped, as it can read it back.
    /// </summary>
    /// <remarks>
    /// Read only, and rewritten from the version being deployed every time one
    /// goes online. It is the other half of the workspace: this is what came
    /// with the code, that is what the code has written since.
    /// </remarks>
    private static readonly string AssetSource = $$"""
        internal sealed class {{AssetType}}
        {
            private readonly string _root;

            internal {{AssetType}}(string root)
            {
                _root = global::System.IO.Path.TrimEndingDirectorySeparator(global::System.IO.Path.GetFullPath(root))
                      + global::System.IO.Path.DirectorySeparatorChar;

                global::System.IO.Directory.CreateDirectory(_root);
            }

            /// <summary>The absolute path the assets were written to.</summary>
            public string Root => _root;

            /// <summary>Whether an asset was shipped under this name.</summary>
            public bool Exists(string name) => global::System.IO.File.Exists(Resolve(name));

            /// <summary>Reads an asset as UTF-8 text.</summary>
            public string ReadText(string name) => global::System.IO.File.ReadAllText(Resolve(name));

            /// <summary>Reads an asset as bytes.</summary>
            public byte[] ReadBytes(string name) => global::System.IO.File.ReadAllBytes(Resolve(name));

            /// <summary>Every asset that was shipped, relative to the root.</summary>
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

            /// <summary>The assets as a resource tree, ready to be served.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree()
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(_root).Build();

            /// <summary>One folder of the assets as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree(string folder)
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(Folder(folder)).Build();

            /// <summary>A handler that serves the assets as files.</summary>
            public global::GenHTTP.Modules.Files.Multi.TreeAssetsBuilder Files()
                => global::GenHTTP.Modules.Files.Assets.From(Tree());

            /// <summary>A handler that serves one folder of the assets as files.</summary>
            public global::GenHTTP.Modules.Files.Multi.TreeAssetsBuilder Files(string folder)
                => global::GenHTTP.Modules.Files.Assets.From(Tree(folder));

            /// <summary>
            /// A single page application over the assets: index.html is the
            /// shell, and a path that matches no file is answered with it.
            /// </summary>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App()
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree()).ServerSideRouting();

            /// <summary>
            /// A single page application over one folder of the assets.
            /// </summary>
            /// <remarks>
            /// So that a front end can be a folder of files added the same way
            /// every other file is, and served by naming it, rather than
            /// having to be the whole of what the lambda ships.
            /// </remarks>
            public global::GenHTTP.Modules.SinglePageApplications.Provider.SinglePageBuilder App(string folder)
                => global::GenHTTP.Modules.SinglePageApplications.SinglePageApplication.From(Tree(folder)).ServerSideRouting();

            /// <summary>The folders assets were shipped in, relative to the root.</summary>
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
                              : "This lambda ships no folders - a file has to be named like 'site/index.html' to be in one.";

                    throw new global::System.InvalidOperationException(
                        "There is no asset folder called '" + name + "'. " + known);
                }

                return resolved;
            }

            private string Resolve(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new global::System.ArgumentException("The name of an asset must not be empty.", "name");
                }

                var resolved = global::System.IO.Path.GetFullPath(global::System.IO.Path.Combine(_root, name));

                if (!resolved.StartsWith(_root, global::System.StringComparison.Ordinal))
                {
                    throw new global::System.ArgumentException("An asset must be inside the asset directory.", "name");
                }

                return resolved;
            }
        }
        """;

    private static readonly string WorkspaceSource = $$"""
        internal sealed class {{WorkspaceType}}
        {
            private const int MaxFileSize = {{WorkspaceLimits.MaxFileSize}};

            private const int MaxFiles = {{WorkspaceLimits.MaxFiles}};

            private readonly string _root;

            internal {{WorkspaceType}}(string root)
            {
                _root = global::System.IO.Path.TrimEndingDirectorySeparator(global::System.IO.Path.GetFullPath(root))
                      + global::System.IO.Path.DirectorySeparatorChar;

                global::System.IO.Directory.CreateDirectory(_root);
            }

            /// <summary>The absolute path of this workspace.</summary>
            public string Root => _root;

            /// <summary>Checks whether the given file exists.</summary>
            public bool Exists(string name) => global::System.IO.File.Exists(Resolve(name));

            /// <summary>Reads a file as UTF-8 text.</summary>
            public string ReadText(string name) => global::System.IO.File.ReadAllText(Resolve(name));

            /// <summary>Reads a file as bytes.</summary>
            public byte[] ReadBytes(string name) => global::System.IO.File.ReadAllBytes(Resolve(name));

            /// <summary>Writes UTF-8 text into a file, replacing it if it exists.</summary>
            public void WriteText(string name, string content)
            {
                var path = Resolve(name);
                Reserve(path, content == null ? 0 : content.Length);
                global::System.IO.File.WriteAllText(path, content);
            }

            /// <summary>Writes bytes into a file, replacing it if it exists.</summary>
            public void WriteBytes(string name, byte[] content)
            {
                var path = Resolve(name);
                Reserve(path, content == null ? 0 : content.Length);
                global::System.IO.File.WriteAllBytes(path, content);
            }

            /// <summary>Removes a file, if it exists.</summary>
            public void Delete(string name)
            {
                var path = Resolve(name);

                if (global::System.IO.File.Exists(path))
                {
                    global::System.IO.File.Delete(path);
                }
            }

            /// <summary>Lists the files in this workspace, relative to its root.</summary>
            public string[] List()
            {
                var files = global::System.IO.Directory.GetFiles(_root, "*", global::System.IO.SearchOption.AllDirectories);

                var result = new string[files.Length];

                for (var i = 0; i < files.Length; i++)
                {
                    result[i] = files[i].Substring(_root.Length).Replace('\\', '/');
                }

                return result;
            }

            /// <summary>Provides this workspace as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree()
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(_root).Build();

            /// <summary>One folder of this workspace as a resource tree.</summary>
            public global::GenHTTP.Api.Content.IO.IResourceTree Tree(string folder)
                => global::GenHTTP.Modules.IO.ResourceTree.FromDirectory(Folder(folder)).Build();

            /// <summary>
            /// A single page application over this workspace.
            /// </summary>
            /// <remarks>
            /// The same thing Assets.App does, over the other directory. Which
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
                => global::System.IO.Directory.CreateDirectory(Resolve(name));

            /// <summary>The folders of this workspace, relative to its root.</summary>
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
                              ? "The folders this workspace holds are: " + string.Join(", ", had) + "."
                              : "This workspace holds no folders yet.";

                    throw new global::System.InvalidOperationException(
                        "There is no folder called '" + name + "' in the workspace. " + known);
                }

                return resolved;
            }

            /// <summary>Creates a handler that serves the files of this workspace.</summary>
            public global::GenHTTP.Modules.Files.Multi.TreeAssetsBuilder Files()
                => global::GenHTTP.Modules.Files.Assets.From(Tree());

            private string Resolve(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new global::System.ArgumentException("The name of a workspace file must not be empty.", "name");
                }

                var resolved = global::System.IO.Path.GetFullPath(global::System.IO.Path.Combine(_root, name));

                if (!resolved.StartsWith(_root, global::System.StringComparison.Ordinal))
                {
                    throw new global::System.UnauthorizedAccessException("'" + name + "' is outside of the workspace of this lambda.");
                }

                var directory = global::System.IO.Path.GetDirectoryName(resolved);

                if (directory != null)
                {
                    global::System.IO.Directory.CreateDirectory(directory);
                }

                return resolved;
            }

            private void Reserve(string path, int size)
            {
                if (size > MaxFileSize)
                {
                    throw new global::System.InvalidOperationException("A workspace file must not exceed " + MaxFileSize + " bytes.");
                }

                if (!global::System.IO.File.Exists(path) && List().Length >= MaxFiles)
                {
                    throw new global::System.InvalidOperationException("A workspace must not hold more than " + MaxFiles + " files.");
                }
            }
        }
        """;

    #endregion

}
