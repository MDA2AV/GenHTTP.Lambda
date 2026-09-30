using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Packs a lambda into a .NET project somebody can open and run.
/// </summary>
/// <remarks>
/// The point of this is to be a way out rather than a way in: the code belongs
/// to whoever wrote it, and a lambda that cannot be taken somewhere else is a
/// lambda held hostage by the convenience of not having to.
///
/// What comes out is as small as a GenHTTP project can be. Program.cs hosts
/// what Project.Create() returns, Project.cs is the snippet, the other files
/// are the lambda's own, and everything that stands in for the platform -
/// Workspace, Assets, Secret and the imports a lambda never had to write - sits
/// in a Platform folder of its own, so it is plain which code is theirs.
///
/// What was written about it comes along where a .NET project keeps such
/// things: the documentation in docs/ and the tests in tests/. Neither is
/// compiled into the program or copied into its container, as neither was
/// on the platform.
/// </remarks>
public static class ProjectPacker
{
    /// <summary>
    /// The GenHTTP package that carries every module a lambda may use, with
    /// the internal engine rather than the one this platform runs on.
    /// </summary>
    /// <remarks>
    /// Ioxide needs Linux and io_uring. The internal engine runs wherever .NET
    /// does, which is what a project somebody opens on their own machine needs.
    /// </remarks>
    private const string Package = "GenHTTP.Full";

    /// <summary>
    /// What the project targets.
    /// </summary>
    /// <remarks>
    /// Not what this platform runs on. It runs on a preview, and handing
    /// somebody a project that needs a preview SDK to open is handing them a
    /// second problem. GenHTTP carries a build for this one.
    /// </remarks>
    private const string Framework = "net10.0";

    /// <summary>
    /// The version of GenHTTP this platform compiles lambdas against, so the
    /// project behaves as the lambda did.
    /// </summary>
    private static readonly string FrameworkVersion = GenHttpVersion();

    #region Functionality

    /// <summary>
    /// Writes the lambda out as a zipped project.
    /// </summary>
    /// <param name="lambda">What is written into the head of Program.cs</param>
    /// <param name="files">Its files, the snippet first</param>
    public static byte[] Pack(ExportedLambda lambda, IReadOnlyList<LambdaFile> files)
    {
        var name = Identifier(lambda.PublicKey);

        var snippet = SourceBuilder.ParseSnippet(files.FirstOrDefault(f => f.Name == LambdaSource.EntryName)?.Code ?? string.Empty);

        var awaits = Awaits(snippet);

        var assets = files.Where(f => f.IsAsset).ToList();

        var context = files.Where(f => f.IsContext).ToList();

        var folders = context.Select(f => Outside(f.Name).Split('/')[0]).Distinct().Order(StringComparer.Ordinal).ToList();

        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            Write(archive, $"{name}/{name}.csproj", Csproj(assets.Count > 0, folders));
            Write(archive, $"{name}/Program.cs", Program(lambda, name, awaits, folders));
            Write(archive, $"{name}/Project.cs", Project(snippet, awaits));

            foreach (var file in files.Where(f => f.IsCode && f.Name != LambdaSource.EntryName))
            {
                Write(archive, $"{name}/{Capitalize(file.Name)}", file.Code);
            }

            // assets keep their folders, because the code that serves them names those folders
            foreach (var file in assets)
            {
                Write(archive, $"{name}/assets/{file.Name}", file.Bytes);
            }

            foreach (var file in context)
            {
                Write(archive, $"{name}/{Outside(file.Name)}", file.Bytes);
            }

            Write(archive, $"{name}/Platform/Usings.cs", Usings());
            Write(archive, $"{name}/Platform/LambdaEnvironment.cs", Resource("LambdaEnvironment.cs"));
            Write(archive, $"{name}/Platform/Folder.cs", Resource("Folder.cs"));
            Write(archive, $"{name}/Platform/Secrets.cs", Resource("Secrets.cs"));
            Write(archive, $"{name}/Platform/Handlers.cs", Resource("Handlers.cs"));

            // the aspnet image rather than runtime, although nothing here uses
            // ASP.NET Core: GenHTTP.Full depends on GenHTTP.Testing, which
            // brings the Kestrel engine and with it Microsoft.AspNetCore.App
            Write(archive, $"{name}/Dockerfile", Resource("Dockerfile").Replace("{assembly}", name));
            Write(archive, $"{name}/.dockerignore", $"bin/\nobj/\nworkspace/\n{string.Concat(folders.Select(f => $"{f}/\n"))}");
            Write(archive, $"{name}/.gitignore", "bin/\nobj/\nworkspace/\n");
        }

        return buffer.ToArray();
    }

    #endregion

    #region Parts

    /// <summary>
    /// The project file: one package, and the assets copied beside the program
    /// when there are any.
    /// </summary>
    /// <remarks>
    /// No nullable context and no implicit usings, as on the platform: the
    /// lambda was written without either, and Platform/Usings.cs brings in
    /// exactly what it had. The documentation and the tests are kept out of
    /// the build, because a test script ending in .cs would otherwise be
    /// compiled into the program the way nothing in them ever was.
    /// </remarks>
    /// <param name="context">The folders the documentation and the tests are in, if there are any</param>
    private static string Csproj(bool assets, IReadOnlyList<string> context)
    {
        var copy = assets ? "\n\n    <ItemGroup>\n        <None Update=\"assets/**\" CopyToOutputDirectory=\"PreserveNewest\" />\n    </ItemGroup>" : string.Empty;

        if (context.Count > 0)
        {
            copy += $"\n\n    <ItemGroup>\n        <Compile Remove=\"{string.Join(';', context.Select(f => $"{f}/**"))}\" />\n    </ItemGroup>";
        }

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>{Framework}</TargetFramework>
                </PropertyGroup>

                <ItemGroup>
                    <PackageReference Include="{Package}" Version="{FrameworkVersion}" />
                </ItemGroup>{copy}

            </Project>

            """;
    }

    /// <summary>
    /// The host, and a word about where the app came from.
    /// </summary>
    /// <param name="context">The folders the documentation and the tests are in, if there are any</param>
    private static string Program(ExportedLambda lambda, string name, bool awaits, IReadOnlyList<string> context)
    {
        var facts = new List<(string Key, string Value)>
        {
            ("Lambda", lambda.Address is { } address ? $"{lambda.PublicKey} ({address})" : lambda.PublicKey),
            ("Version", $"{lambda.Version}, saved {Day(lambda.Saved)}")
        };

        if (!string.IsNullOrWhiteSpace(lambda.Change))
        {
            facts.Add(("Change", OneLine(lambda.Change)));
        }

        facts.Add(("Exported", Day(lambda.Exported)));
        facts.Add(("GenHTTP", FrameworkVersion));

        var width = facts.Max(f => f.Key.Length) + 2;

        var header = string.Join("\n", facts.Select(f => $"//   {f.Key.PadRight(width)}{f.Value}"));

        var tag = name.ToLowerInvariant();

        var handler = awaits ? "await Project.CreateAsync()" : "Project.Create()";

        // the names of the secrets, never their values, which stay on the platform
        var secrets = lambda.Secrets?.Order(StringComparer.Ordinal).ToList() ?? [];

        var variables = string.Concat(secrets.Select(s => $" -e {s}"));

        var environment = secrets.Count == 0
            ? string.Empty
            : "\n//\n// Its secrets are environment variables here - set them before it starts; the\n// values stayed on the platform, where nobody can read them back:\n//\n"
            + string.Join("\n", secrets.Select(s => $"//   {s}"));

        var written = (context.Contains("docs"), context.Contains("tests")) switch
        {
            (true, true) => "\n//\n// docs/ says what the app is for and why it is built the way it is, and tests/\n// how it is tested - as they were written beside it on the platform.",
            (true, false) => "\n//\n// docs/ says what the app is for and why it is built the way it is, as it was\n// written beside it on the platform.",
            (false, true) => "\n//\n// tests/ says how it is tested, as it was written beside it on the platform.",
            _ => string.Empty
        };

        return $"""
            // This app was built as a lambda on GenHTTP Lambda (https://genhttp.dev),
            // where you describe an app - or let your coding agent write it - and it
            // is online at an address of its own a moment later, with every version
            // kept and a way back to each of them.
            //
            {header}
            //
            // It is served by GenHTTP, an embeddable web server for .NET. What it can
            // do, and how: https://genhttp.org/documentation/
            //
            //   dotnet run                    then open http://localhost:8080/
            //
            //   docker build -t {tag} .
            //   docker run -p 8080:8080{variables} -v {tag}-data:/app/workspace {tag}
            //
            // Project.cs holds the code of the lambda and the other .cs files are its
            // own. Platform/ stands in for what the platform provided: the Workspace
            // the app writes to, the Assets it shipped with (in assets/), and the
            // Secret it reads, from environment variables of the same name.{environment}{written}

            using GenHTTP.Engine.Internal;
            using GenHTTP.Modules.Practices;

            await Host.Create()
                      .Handler({handler})
                      .Defaults()
                      .RunAsync();

            """;
    }

    /// <summary>
    /// The snippet, as the body of the method that builds the app.
    /// </summary>
    /// <remarks>
    /// The same split the platform makes. A snippet ends in a return, which at
    /// the top of a file would end the program rather than produce a handler,
    /// and it may end with a record or two, which cannot be declared inside a
    /// method - so statements become the body and types sit beside the class.
    /// Leading comments travel with what they stand in front of.
    /// </remarks>
    private static string Project(SyntaxTree snippet, bool awaits)
    {
        var root = (CompilationUnitSyntax)snippet.GetRoot();

        var text = snippet.GetText();

        var builder = new StringBuilder();

        foreach (var import in root.Usings)
        {
            builder.Append(import.NormalizeWhitespace().ToFullString()).Append('\n');
        }

        if (root.Usings.Count > 0)
        {
            builder.Append('\n');
        }

        builder.Append("public static class Project").Append('\n');
        builder.Append("{").Append('\n');
        builder.Append("    // Workspace comes from Platform/LambdaEnvironment.cs, for every file").Append('\n');
        builder.Append("    private static Platform.Folder Assets => Platform.LambdaEnvironment.Assets;").Append('\n');
        builder.Append('\n');

        if (awaits)
        {
            builder.Append("    public static async Task<IHandler> CreateAsync() => Platform.Handlers.From(await BuildAsync());").Append('\n');
            builder.Append('\n');
            builder.Append("    private static async Task<object> BuildAsync()").Append('\n');
        }
        else
        {
            builder.Append("    public static IHandler Create() => Platform.Handlers.From(Build());").Append('\n');
            builder.Append('\n');
            builder.Append("    private static object Build()").Append('\n');
        }

        builder.Append("    {").Append('\n');

        foreach (var line in Body(root, text))
        {
            builder.Append(line).Append('\n');
        }

        builder.Append("    }").Append('\n');
        builder.Append("}").Append('\n');

        foreach (var member in root.Members.Where(IsType))
        {
            builder.Append('\n');
            builder.Append(Public(member, text).Trim('\r', '\n')).Append('\n');
        }

        return builder.ToString();
    }

    /// <summary>
    /// What every lambda may name without a using.
    /// </summary>
    /// <remarks>
    /// The platform puts the same imports at the top of every file it
    /// compiles, which is why the other files of a lambda have none. Global
    /// here, so they compile as they are.
    /// </remarks>
    private static string Usings()
    {
        var builder = new StringBuilder();

        builder.Append("// What the platform imported into every file of the lambda.").Append('\n');
        builder.Append('\n');

        foreach (var import in ModuleCatalog.Imports)
        {
            builder.Append($"global using {import};").Append('\n');
        }

        builder.Append('\n');
        builder.Append("global using Platform;").Append('\n');
        builder.Append("global using static Platform.LambdaScope;").Append('\n');

        return builder.ToString();
    }

    #endregion

    #region Snippet

    private static bool IsType(MemberDeclarationSyntax member) => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax;

    /// <summary>
    /// Whether the statements of the snippet wait for something, which
    /// decides whether the method they become has to be asynchronous.
    /// </summary>
    /// <remarks>
    /// Most snippets do not, and a synchronous Project.Create() is the
    /// simpler thing to read. Waiting inside a lambda or a local function
    /// does not count, because that one is asynchronous on its own.
    /// </remarks>
    private static bool Awaits(SyntaxTree snippet)
    {
        var root = (CompilationUnitSyntax)snippet.GetRoot();

        return root.Members.OfType<GlobalStatementSyntax>()
                   .SelectMany(s => s.DescendantNodesAndSelf(n => n is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)))
                   .Any(n => n switch
                   {
                       AwaitExpressionSyntax => true,
                       CommonForEachStatementSyntax loop => loop.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       UsingStatementSyntax block => block.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       LocalDeclarationStatementSyntax declaration => declaration.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       _ => false
                   });
    }

    /// <summary>
    /// The lines of the statements, moved eight spaces in to sit in the method.
    /// </summary>
    /// <remarks>
    /// A line that starts inside a token - a verbatim or raw string spanning
    /// lines - is left as it is, because its whitespace is part of the string.
    /// </remarks>
    private static List<string> Body(CompilationUnitSyntax root, SourceText text)
    {
        var numbers = new SortedSet<int>();

        foreach (var member in root.Members.Where(m => !IsType(m)))
        {
            var first = text.Lines.GetLineFromPosition(member.FullSpan.Start).LineNumber;
            var last = text.Lines.GetLineFromPosition(Math.Max(member.FullSpan.Start, member.FullSpan.End - 1)).LineNumber;

            for (var number = first; number <= last; number++)
            {
                numbers.Add(number);
            }
        }

        var lines = new List<string>();

        foreach (var number in numbers)
        {
            var line = text.Lines[number];

            var content = line.ToString();

            var token = root.FindToken(line.Start);

            var inside = line.Start > token.SpanStart && line.Start < token.Span.End;

            lines.Add(inside ? content : content.Trim().Length == 0 ? string.Empty : "        " + content);
        }

        // the blank lines that stood between the usings, the statements and the types
        while (lines.Count > 0 && lines[0].Length == 0)
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    /// <summary>
    /// A type declared in the snippet, made public on the way.
    /// </summary>
    /// <remarks>
    /// As the platform does: GenHTTP generates the code that invokes a handler
    /// into an assembly of its own, so every type in the signature of a
    /// handler has to be visible from outside this one.
    /// </remarks>
    private static string Public(MemberDeclarationSyntax member, SourceText text)
    {
        var original = text.ToString(member.FullSpan);

        var modifiers = member.Modifiers.Where(IsAccessibility).ToList();

        if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            return original;
        }

        var start = member.FullSpan.Start;

        var at = member.AttributeLists.Count > 0
               ? member.AttributeLists.Last().GetLastToken().GetNextToken().SpanStart
               : member.GetFirstToken().SpanStart;

        // from the back, so every position stays where it was; at the same
        // position the modifier goes before "public" comes in
        var edits = modifiers.Select(m => (Start: m.SpanStart, Length: m.FullSpan.End - m.SpanStart, Text: string.Empty))
                             .Append((Start: at, Length: 0, Text: "public "))
                             .OrderByDescending(e => e.Start)
                             .ThenByDescending(e => e.Length);

        var builder = new StringBuilder(original);

        foreach (var (position, length, insert) in edits)
        {
            builder.Remove(position - start, length).Insert(position - start, insert);
        }

        return builder.ToString();
    }

    private static bool IsAccessibility(SyntaxToken token) => token.Kind() is SyntaxKind.PublicKeyword
        or SyntaxKind.InternalKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.FileKeyword;

    #endregion

    #region Plumbing

    /// <summary>
    /// A name that is legal as both a folder and a project.
    /// </summary>
    private static string Identifier(string publicKey)
    {
        var builder = new StringBuilder();

        foreach (var character in publicKey)
        {
            builder.Append(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' ? character : '-');
        }

        var name = builder.ToString().Trim('-');

        return name.Length == 0 || !char.IsAsciiLetter(name[0]) ? $"lambda-{name}".TrimEnd('-') : name;
    }

    /// <summary>
    /// A code file named the way .NET names them: "store.cs" becomes
    /// "Store.cs", and the folders it is in stay as they are.
    /// </summary>
    private static string Capitalize(string path)
    {
        var slash = path.LastIndexOf('/') + 1;

        return slash < path.Length ? path[..slash] + char.ToUpperInvariant(path[slash]) + path[(slash + 1)..] : path;
    }

    /// <summary>
    /// Where a file of the context goes in the project: .lambda/docs/x.md to docs/x.md.
    /// </summary>
    private static string Outside(string name) => name[LambdaSource.ContextFolder.Length..];

    private static string Day(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string OneLine(string value) => string.Join(' ', value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string GenHttpVersion()
    {
        var version = typeof(IHandler).Assembly.GetName().Version ?? new Version(11, 0, 0);

        return $"{version.Major}.{version.Minor}.{Math.Max(0, version.Build)}";
    }

    private static string Resource(string file)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"GenHTTP.Lambda.Resources.Export.{file}.txt")
                        ?? throw new InvalidOperationException($"The export file '{file}' is missing from the assembly.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    private static void Write(ZipArchive archive, string path, string content)
        => Write(archive, path, Encoding.UTF8.GetBytes(content));

    private static void Write(ZipArchive archive, string path, byte[] content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);

        using var stream = entry.Open();

        stream.Write(content);
    }

    #endregion

}

/// <summary>
/// What an exported project says about the lambda it came from.
/// </summary>
/// <param name="PublicKey">The name of the lambda, which names the project</param>
/// <param name="Version">The version being exported</param>
/// <param name="Saved">When that version was saved</param>
/// <param name="Change">What that version changed, in a line</param>
/// <param name="Address">Where the lambda is online, if the installation knows its own address</param>
/// <param name="Exported">When the export was made</param>
/// <param name="Secrets">The names of the secrets it keeps or reads, which become environment variables - never their values</param>
public sealed record ExportedLambda(string PublicKey, int Version, DateTime Saved, string? Change, string? Address, DateTime Exported,
                                    IReadOnlyList<string>? Secrets = null);
