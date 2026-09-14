using System.IO.Compression;
using System.Text;

using Microsoft.CodeAnalysis;

using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Packs a lambda into a .NET project somebody can open and run.
/// </summary>
/// <remarks>
/// The point of this is to be a way out rather than a way in. Whatever is
/// written here runs on somebody else's machine for as long as they leave it
/// running; a lambda that cannot be taken somewhere else is a lambda held
/// hostage by the convenience of not having to.
///
/// What comes out is an ordinary project. One package reference, a host that
/// serves the handler the snippet returns, the files exactly as they were
/// written, and small stand-ins for the two things the platform provides that
/// a plain project has no notion of - Workspace and Assets - pointed at two
/// folders beside the code. No part of it refers to this platform, and nothing
/// has to be uncommented or filled in before it runs.
/// </remarks>
public static class ProjectPacker
{
    /// <summary>The GenHTTP package that carries everything a lambda may use.</summary>
    private const string Package = "GenHTTP.Full.Ioxide";

    private const string Version = "11.0.1";

    /// <summary>
    /// What the project targets.
    /// </summary>
    /// <remarks>
    /// Not what this platform runs on. It runs on a preview, and handing
    /// somebody a project that needs a preview SDK to open is handing them a
    /// second problem. GenHTTP 11 carries a build for this one, which is the
    /// newest that somebody is likely to already have.
    /// </remarks>
    private const string Framework = "net10.0";

    #region Functionality

    /// <summary>
    /// Writes the lambda out as a zipped project.
    /// </summary>
    /// <param name="publicKey">What the lambda is called, which names the project</param>
    /// <param name="files">Its files, the snippet first</param>
    public static byte[] Pack(string publicKey, IReadOnlyList<LambdaFile> files)
    {
        var name = Identifier(publicKey);

        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true))
        {
            Write(archive, $"{name}/{name}.csproj", Project());
            Write(archive, $"{name}/Program.cs", Program(files));
            Write(archive, $"{name}/Lambda.cs", Support());
            Write(archive, $"{name}/README.md", Readme(name, publicKey, files));
            Write(archive, $"{name}/.gitignore", "bin/\nobj/\nworkspace/\n");

            foreach (var file in files)
            {
                if (file.Name == LambdaSource.EntryName)
                {
                    // the snippet is the body of Program.cs rather than a file
                    continue;
                }

                if (file.IsCode)
                {
                    Write(archive, $"{name}/{file.Name}", file.Code);
                }
                else
                {
                    // assets keep their folders, because the code that serves
                    // them names those folders
                    Write(archive, $"{name}/assets/{file.Name}", file.Bytes);
                }
            }

            // a workspace that is empty is still a workspace, and a project
            // that creates its own on first run is a project that behaves
            // differently the first time
            Write(archive, $"{name}/workspace/.keep", string.Empty);
        }

        return buffer.ToArray();
    }

    #endregion

    #region Parts

    private static string Project() => $"""
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>{Framework}</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>disable</Nullable>
            <RootNamespace>Lambda</RootNamespace>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="{Package}" Version="{Version}" />
          </ItemGroup>

          <ItemGroup>
            <None Update="assets\**" CopyToOutputDirectory="PreserveNewest" />
          </ItemGroup>

        </Project>

        """;

    /// <summary>
    /// The snippet, wrapped in a program that hosts what it returns.
    /// </summary>
    /// <remarks>
    /// The snippet is put in a local function rather than inlined, for the
    /// same reason the platform puts it in a method: it ends in a return, and
    /// a return at the top of a file ends the program instead of producing a
    /// handler. Everything a lambda gets without asking is imported at the
    /// top, in the order the platform imports it, so code that compiled there
    /// compiles here.
    /// </remarks>
    private static string Program(IReadOnlyList<LambdaFile> files)
    {
        var snippet = files.FirstOrDefault(f => f.Name == LambdaSource.EntryName)?.Code ?? string.Empty;

        var parsed = SourceBuilder.ParseSnippet(snippet);

        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)parsed.GetRoot();

        /*
         * The same split the platform makes. A snippet may end with a record
         * or two, and a type cannot be declared inside a method body - so
         * statements become the body and declared types sit beside it.
         */
        var statements = new List<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>();
        var types = new List<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>();

        foreach (var member in root.Members)
        {
            var declared = member is Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax
                                  or Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax;

            (declared ? types : statements).Add(member);
        }

        var builder = new StringBuilder();

        foreach (var import in ModuleCatalog.Imports)
        {
            builder.AppendLine($"using {import};");
        }

        // hosting, which a lambda never had to do for itself
        builder.AppendLine("using System.Runtime.InteropServices;");
        builder.AppendLine("using GenHTTP.Api.Infrastructure;");
        builder.AppendLine("using GenHTTP.Modules.Practices;");

        foreach (var import in root.Usings)
        {
            builder.AppendLine(import.NormalizeWhitespace().ToFullString());
        }

        builder.AppendLine();
        builder.AppendLine("""
            // This was a lambda. It is an ordinary program now: everything inside
            // Build below is exactly what was written in the editor, and the rest
            // is the hosting the platform used to do for you.
            //
            //     dotnet run
            //
            // and it is at http://localhost:8080/.

            var built = await Entry.BuildAsync();

            var handler = built switch
            {
                IHandler ready => ready,
                IHandlerBuilder builder => builder.Build(),
                null => throw new InvalidOperationException("The snippet returned nothing to serve."),
                _ => throw new InvalidOperationException($"The snippet returned {built.GetType().Name}, which is not a handler.")
            };

            var host = GenHTTP.Engine.Ioxide.Host.Create()
                                              .Handler(handler)
                                              .Defaults()
                                              .Port(8080);

            await host.StartAsync();

            Console.WriteLine("Listening on http://localhost:8080/ . Ctrl+C to stop.");

            /*
             * Waits for a signal rather than for a keypress. Reading the
             * console works right up until nobody is typing at it - a service
             * unit, a container, anything with its input closed - where it
             * returns straight away and the program stops the moment it has
             * started.
             */
            var shutdown = new TaskCompletionSource();

            void Stop(PosixSignalContext context)
            {
                context.Cancel = true;
                shutdown.TrySetResult();
            }

            using (PosixSignalRegistration.Create(PosixSignal.SIGTERM, Stop))
            using (PosixSignalRegistration.Create(PosixSignal.SIGINT, Stop))
            {
                Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.TrySetResult(); };

                await shutdown.Task;
            }

            Console.WriteLine("Stopping.");

            await host.StopAsync();

            /// <summary>
            /// Where the snippet lives.
            /// </summary>
            /// <remarks>
            /// A class rather than the top of the file, for two reasons. The
            /// snippet ends in a return, which at the top of a file would end
            /// the program instead of producing a handler. And Workspace and
            /// Assets are properties here, which is what lets them keep those
            /// names without colliding with the module of the same name.
            /// </remarks>
            static class Entry
            {
                private static Folder Workspace => Lambda.Workspace;
                private static Folder Assets => Lambda.Assets;

                internal static async Task<object> BuildAsync()
                {
            """);

        var text = parsed.GetText();

        foreach (var member in statements)
        {
            foreach (var line in text.ToString(member.Span).Replace("\r\n", "\n").Split('\n'))
            {
                builder.AppendLine(line.Length == 0 ? string.Empty : "        " + line);
            }
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        foreach (var member in types)
        {
            builder.AppendLine();
            builder.AppendLine(text.ToString(member.Span));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Stand-ins for the two things the platform hands a lambda.
    /// </summary>
    /// <remarks>
    /// Deliberately the same surface and deliberately smaller: these are two
    /// folders beside the program rather than anything enforced. The limits
    /// the hosted version applies are its own business, and a project on
    /// somebody's own machine has no reason to inherit them.
    /// </remarks>
    private static string Support() => """
        using GenHTTP.Api.Content.IO;
        using GenHTTP.Modules.IO;
        using GenHTTP.Modules.Files;
        using GenHTTP.Modules.Files.Multi;
        using GenHTTP.Modules.SinglePageApplications;
        using GenHTTP.Modules.SinglePageApplications.Provider;

        /// <summary>
        /// A folder beside the program, the way the platform gave you one.
        /// </summary>
        public sealed class Folder
        {
            private readonly string _root;

            public Folder(string root)
            {
                _root = Path.GetFullPath(root);

                Directory.CreateDirectory(_root);
            }

            public string Root => _root;

            public bool Exists(string name) => File.Exists(Resolve(name));

            public string ReadText(string name) => File.ReadAllText(Resolve(name));

            public byte[] ReadBytes(string name) => File.ReadAllBytes(Resolve(name));

            public void WriteText(string name, string content)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Resolve(name))!);
                File.WriteAllText(Resolve(name), content);
            }

            public void WriteBytes(string name, byte[] content)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Resolve(name))!);
                File.WriteAllBytes(Resolve(name), content);
            }

            public void Delete(string name)
            {
                if (File.Exists(Resolve(name))) File.Delete(Resolve(name));
            }

            public void CreateFolder(string name) => Directory.CreateDirectory(Resolve(name));

            public string[] List()
                => Directory.Exists(_root)
                 ? [.. Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
                                .Select(p => p[(_root.Length + 1)..].Replace('\\', '/'))]
                 : [];

            public string[] Folders()
                => Directory.Exists(_root)
                 ? [.. Directory.GetDirectories(_root, "*", SearchOption.AllDirectories)
                                .Select(p => p[(_root.Length + 1)..].Replace('\\', '/'))]
                 : [];

            public IResourceTree Tree() => ResourceTree.FromDirectory(_root).Build();

            public IResourceTree Tree(string folder) => ResourceTree.FromDirectory(Resolve(folder)).Build();

            public TreeAssetsBuilder Files() => Assets.From(Tree());

            public TreeAssetsBuilder Files(string folder) => Assets.From(Tree(folder));

            public SinglePageBuilder App() => SinglePageApplication.From(Tree()).ServerSideRouting();

            public SinglePageBuilder App(string folder) => SinglePageApplication.From(Tree(folder)).ServerSideRouting();

            private string Resolve(string name)
            {
                var resolved = Path.GetFullPath(Path.Combine(_root, name));

                if (!resolved.StartsWith(_root, StringComparison.Ordinal))
                {
                    throw new ArgumentException("That is outside the folder.", nameof(name));
                }

                return resolved;
            }
        }

        /// <summary>
        /// The two names a lambda could use without declaring them.
        /// </summary>
        public static class Lambda
        {
            /// <summary>What the lambda shipped, which is the assets folder here.</summary>
            public static readonly Folder Assets = new("assets");

            /// <summary>What it writes, which is the workspace folder here.</summary>
            public static readonly Folder Workspace = new("workspace");
        }

        """;

    private static string Readme(string name, string publicKey, IReadOnlyList<LambdaFile> files) => $"""
        # {name}

        This was the lambda `{publicKey}` on genhttp.dev. It is an ordinary
        .NET project now, and nothing in it refers back to that platform.

        ```
        dotnet run
        ```

        Then open <http://localhost:8080/>.

        ## What is where

        | | |
        |---|---|
        | `Program.cs` | your snippet, in a program that hosts what it returns |
        {string.Join("\n", files.Where(f => f.IsCode && f.Name != LambdaSource.EntryName)
                                .Select(f => $"| `{f.Name}` | exactly as you wrote it |"))}
        | `Lambda.cs` | stand-ins for `Workspace` and `Assets`, as folders |
        | `assets/` | the files your lambda shipped |
        | `workspace/` | what it reads and writes at runtime |

        ## The two differences

        `Workspace` and `Assets` were provided by the platform. Here they are
        `Lambda.Workspace` and `Lambda.Assets`, over the two folders above. The
        top of `Program.cs` brings them into scope under their old names, so
        your code did not have to change.

        The limits the hosted version applied - how large a file may be, how
        many there may be, what the compiler would refuse - were its own. This
        is your machine and none of them came with it.

        """;

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
