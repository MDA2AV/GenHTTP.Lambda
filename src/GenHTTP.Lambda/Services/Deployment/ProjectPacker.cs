using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Source;

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
/// Workspace, Assets, Secret, Database and the imports a lambda never had to
/// write - sits in a Platform folder of its own, so it is plain which code is
/// theirs.
///
/// What was written about it comes along where a .NET project keeps such
/// things: the documentation in docs/ and the tests in tests/. Neither is
/// compiled into the program or copied into its container, as neither was
/// on the platform. Nor is its development space, in dev/ - what its assets
/// or code are built from - which the project does not even look into: once
/// built, it may hold whatever its build tool installed.
///
/// A lambda with a database takes it along: what the app kept is written into
/// database/, and the project references SQLite - Entity Framework Core where
/// the code keeps its records with it, and Evolve where the code migrates
/// with it. One without a database references none of them.
///
/// The same project is what a lambda whose owner published its source is read
/// as on /source, and downloaded as from there - without the database, which
/// is data and never published, and with the license it is published under
/// in LICENSE, where every project keeps it.
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

    /// <summary>
    /// The SQLite library the project talks to its database with.
    /// </summary>
    /// <remarks>
    /// The newest release for the framework the project targets, rather than
    /// the one this platform runs, which is a preview.
    /// </remarks>
    private const string SqlitePackage = "Microsoft.Data.Sqlite";

    private const string SqliteVersion = "10.0.12";

    /// <summary>
    /// What the project keeps its records with, where the code uses it: Entity
    /// Framework Core on SQLite, from the same release as the library above.
    /// </summary>
    private const string EntityFrameworkPackage = "Microsoft.EntityFrameworkCore.Sqlite";

    /// <summary>
    /// What migrates the database, at the version the platform runs.
    /// </summary>
    private const string EvolvePackage = "Evolve";

    private static readonly string EvolveVersion = typeof(EvolveDb.Evolve).Assembly.GetName().Version is { } evolve
        ? $"{evolve.Major}.{evolve.Minor}.{Math.Max(0, evolve.Build)}"
        : "3.2.0";

    /// <summary>
    /// Where the project keeps its database, relative to where it runs.
    /// </summary>
    internal const string DatabaseFile = "database/database.db";

    #region Functionality

    /// <summary>
    /// Writes the lambda out as a zipped project.
    /// </summary>
    /// <param name="lambda">What is written into the head of Program.cs</param>
    /// <param name="files">Its files, the snippet first</param>
    /// <param name="database">A plain copy of its database, to carry along</param>
    public static byte[] Pack(ExportedLambda lambda, IReadOnlyList<LambdaFile> files, string? database = null)
    {
        using var buffer = new MemoryStream();

        Pack(lambda, files, buffer, database);

        return buffer.ToArray();
    }

    /// <summary>
    /// Writes the lambda out as the project its published source is read and
    /// downloaded as: its program and what is written about it, with the
    /// license it is published under.
    /// </summary>
    /// <remarks>
    /// Apart from <see cref="Pack(ExportedLambda, IReadOnlyList{LambdaFile}, Stream, string?)" />
    /// on purpose, and without a database to pass: the export is its owner's
    /// and takes the records along, a published source is everybody's and
    /// never holds data - not the database, not the workspace, not a secret's
    /// value. What cannot be handed in cannot be let out by mistake.
    /// </remarks>
    public static void Publish(ExportedLambda lambda, IReadOnlyList<LambdaFile> files, Stream target)
    {
        if (lambda.License == null)
        {
            throw new ArgumentException("A published source is packed with the license it is published under.", nameof(lambda));
        }

        Pack(lambda, files, target, database: null);
    }

    /// <summary>
    /// Writes the lambda out as a zipped project, into the given stream as it
    /// goes - so a database of a gigabyte is streamed, not held.
    /// </summary>
    /// <param name="database">A plain copy of its database, to carry along - its owner's export only, see <see cref="Publish" /></param>
    public static void Pack(ExportedLambda lambda, IReadOnlyList<LambdaFile> files, Stream target, string? database = null)
    {
        var name = Identifier(lambda.PublicKey);

        var snippet = files.FirstOrDefault(f => f.Name == LambdaSource.EntryName)?.Code ?? string.Empty;

        var awaits = ProjectSnippet.Awaits(snippet);

        var assets = files.Where(f => f.IsAsset).ToList();

        var context = files.Where(f => f.IsContext).ToList();

        var folders = context.Select(f => ProjectPaths.Of(f.Name).Split('/')[0]).Distinct().Order(StringComparer.Ordinal).ToList();

        var development = files.Where(f => f.IsDevelopment).ToList();

        var code = files.Where(f => f.IsCode).ToList();

        // what the code talks to its database with, and whether it migrates it
        var data = database != null || code.Any(f => DatabaseService.Uses(f.Code) || f.Code.Contains("Sqlite", StringComparison.Ordinal));

        var evolve = data && code.Any(f => f.Code.Contains("Evolve", StringComparison.Ordinal));

        // a context of its own is how code uses Entity Framework
        var entities = data && code.Any(f => f.Code.Contains("DbContext", StringComparison.Ordinal));

        using (var archive = new ZipArchive(target, ZipArchiveMode.Create, true))
        {
            Write(archive, $"{name}/{name}.csproj", Csproj(assets.Count > 0, folders, development.Count > 0, data, entities, evolve));
            Write(archive, $"{name}/Program.cs", Program(lambda, name, awaits, folders, development.Count > 0, data ? database != null : null));
            Write(archive, $"{name}/{ProjectPaths.Snippet}", ProjectSnippet.ForExport(snippet));

            foreach (var file in files.Where(f => f.IsCode && f.Name != LambdaSource.EntryName))
            {
                Write(archive, $"{name}/{ProjectPaths.Of(file.Name)}", file.Code);
            }

            // assets keep their folders, because the code that serves them names those folders
            foreach (var file in assets)
            {
                Write(archive, $"{name}/{ProjectPaths.Of(file.Name)}", file.Bytes);
            }

            foreach (var file in context.Concat(development))
            {
                Write(archive, $"{name}/{ProjectPaths.Of(file.Name)}", file.Bytes);
            }

            Write(archive, $"{name}/Platform/Usings.cs", Usings(data, entities, evolve));
            Write(archive, $"{name}/Platform/LambdaEnvironment.cs", Resource("LambdaEnvironment.cs"));
            Write(archive, $"{name}/Platform/Folder.cs", Resource("Folder.cs"));
            Write(archive, $"{name}/Platform/Secrets.cs", Resource("Secrets.cs"));
            Write(archive, $"{name}/Platform/Handlers.cs", Resource("Handlers.cs"));

            if (data)
            {
                Write(archive, $"{name}/Platform/Database.cs", Resource("Database.cs"));
            }

            if (database != null)
            {
                WriteFile(archive, $"{name}/{DatabaseFile}", database);
            }

            if (lambda.License is { } license)
            {
                Write(archive, $"{name}/LICENSE", SourceLicenses.Text(license.License, lambda.Saved.Year, license.Holder));
            }

            // the aspnet image rather than runtime, although nothing here uses
            // ASP.NET Core: GenHTTP.Full depends on GenHTTP.Testing, which
            // brings the Kestrel engine and with it Microsoft.AspNetCore.App
            Write(archive, $"{name}/Dockerfile", Resource("Dockerfile").Replace("{assembly}", name));

            // what the app keeps is kept out of the image and out of the
            // repository: the image mounts it, and data is nobody's source
            var ignored = data ? "bin/\nobj/\nworkspace/\ndatabase/\n" : "bin/\nobj/\nworkspace/\n";

            // the documentation, the tests and the development space are kept out of the image as well
            var beside = development.Count > 0 ? [.. folders, ProjectPaths.Development.TrimEnd('/')] : folders;

            Write(archive, $"{name}/.dockerignore", $"{ignored}{string.Concat(beside.Select(f => $"{f}/\n"))}");
            Write(archive, $"{name}/.gitignore", ignored);
        }
    }

    /// <summary>
    /// The files of the project a lambda's git repository holds: the lambda's
    /// own where <see cref="ProjectPaths"/> puts them, and the platform's around them.
    /// </summary>
    /// <remarks>
    /// The export as a repository needs it. What the platform puts around the
    /// lambda is the same for every commit, whatever the lambda does - only
    /// a newer platform, a new address or a published source change it - so a
    /// push that leaves it alone is right however much it changed, and
    /// somebody who changes it has changed nothing of the lambda. That is why
    /// the project references SQLite, Entity Framework and Evolve whether
    /// the code uses them or not, keeps the documentation, the tests and the
    /// development space out of the build whether there are any or not, and
    /// makes the snippet asynchronous whether it awaits anything or not: the
    /// commit that starts to is somebody's push, and must not need a project
    /// of its own.
    ///
    /// There is no data in it, as in a published source, and nothing that
    /// only the owner may know - the same commits are read by anybody once
    /// the source is published. AGENTS.md tells an agent how to work in it,
    /// and CLAUDE.md points the agent that reads that file instead to it.
    /// </remarks>
    public static IReadOnlyList<ProjectFile> Repository(RepositoryProject project, IReadOnlyList<LambdaFile> files)
    {
        var name = Identifier(project.PublicKey);

        var result = new List<ProjectFile>
        {
            Text($"{name}.csproj", Csproj(true, ["docs", "tests"], true, true, true, true, awaitsAlways: true)),
            Text(ProjectPaths.Program, RepositoryProgram(project)),
            Text(ProjectPaths.Snippet, ProjectSnippet.ForRepository(files.FirstOrDefault(f => f.Name == LambdaSource.EntryName)?.Code ?? string.Empty), LambdaSource.EntryName)
        };

        foreach (var file in files.Where(f => f.Name != LambdaSource.EntryName))
        {
            result.Add(new ProjectFile(ProjectPaths.Of(file.Name), file.Bytes, file.Name));
        }

        result.Add(Text($"{ProjectPaths.Platform}Usings.cs", Usings(true, true, true)));
        result.Add(Text($"{ProjectPaths.Platform}LambdaEnvironment.cs", Resource("LambdaEnvironment.cs")));
        result.Add(Text($"{ProjectPaths.Platform}Folder.cs", Resource("Folder.cs")));
        result.Add(Text($"{ProjectPaths.Platform}Secrets.cs", Resource("Secrets.cs")));
        result.Add(Text($"{ProjectPaths.Platform}Handlers.cs", Resource("Handlers.cs")));
        result.Add(Text($"{ProjectPaths.Platform}Database.cs", Resource("Database.cs")));

        if (project.License is { } license)
        {
            result.Add(Text("LICENSE", SourceLicenses.Text(license.License, license.Year, license.Holder)));
        }

        result.Add(Text("Dockerfile", Resource("Dockerfile").Replace("{assembly}", name)));

        const string ignored = "bin/\nobj/\nworkspace/\ndatabase/\n";

        result.Add(Text(".gitignore", ignored));
        result.Add(Text(".dockerignore", $"{ignored}docs/\ntests/\ndev/\n.git/\nAGENTS.md\nCLAUDE.md\n"));

        result.Add(Text("AGENTS.md", Resource("Agents.md").Replace("{lambda}", project.PublicKey)
                                                           .Replace("{project}", name)
                                                           .Replace("{address}", project.Address)
                                                           .Replace("{home}", project.Home)));

        result.Add(Text("CLAUDE.md", Resource("Claude.md")));

        return result;
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
    /// compiled into the program the way nothing in them ever was. The
    /// development space is kept out of every item the build gathers, not
    /// only out of what it compiles: once built, it may hold whatever its
    /// build tool installed - tens of thousands of files every build would
    /// otherwise walk - and files of its own the build would trip over.
    /// </remarks>
    /// <param name="context">The folders the documentation and the tests are in, if there are any</param>
    /// <param name="development">Whether there is a development space</param>
    /// <param name="data">Whether the code uses a database, which takes SQLite</param>
    /// <param name="entities">Whether it keeps its records with Entity Framework Core</param>
    /// <param name="evolve">Whether it migrates it with Evolve</param>
    /// <param name="awaitsAlways">
    /// Whether the snippet is made asynchronous whether it waits for anything or not, as the project of a
    /// repository has it - which the compiler only warns about, and need not
    /// </param>
    private static string Csproj(bool assets, IReadOnlyList<string> context, bool development, bool data, bool entities, bool evolve, bool awaitsAlways = false)
    {
        var copy = assets ? "\n\n    <ItemGroup>\n        <None Update=\"assets/**\" CopyToOutputDirectory=\"PreserveNewest\" />\n    </ItemGroup>" : string.Empty;

        if (context.Count > 0)
        {
            copy += $"\n\n    <ItemGroup>\n        <Compile Remove=\"{string.Join(';', context.Select(f => $"{f}/**"))}\" />\n    </ItemGroup>";
        }

        var sqlite = data ? $"\n        <PackageReference Include=\"{SqlitePackage}\" Version=\"{SqliteVersion}\" />" : string.Empty;

        var records = entities ? $"\n        <PackageReference Include=\"{EntityFrameworkPackage}\" Version=\"{SqliteVersion}\" />" : string.Empty;

        var migrations = evolve ? $"\n        <PackageReference Include=\"{EvolvePackage}\" Version=\"{EvolveVersion}\" />" : string.Empty;

        // CS1998: an async method that awaits nothing
        var quiet = awaitsAlways ? "\n        <NoWarn>$(NoWarn);CS1998</NoWarn>" : string.Empty;

        if (development)
        {
            quiet += $"\n        <DefaultItemExcludes>$(DefaultItemExcludes);{ProjectPaths.Development}**</DefaultItemExcludes>";
        }

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>{Framework}</TargetFramework>{quiet}
                </PropertyGroup>

                <ItemGroup>
                    <PackageReference Include="{Package}" Version="{FrameworkVersion}" />{sqlite}{records}{migrations}
                </ItemGroup>{copy}

            </Project>

            """;
    }

    /// <summary>
    /// The host, and a word about where the app came from.
    /// </summary>
    /// <param name="context">The folders the documentation and the tests are in, if there are any</param>
    /// <param name="development">Whether it has a development space</param>
    /// <param name="database">Whether the project carries the app's database; nothing where it has none</param>
    private static string Program(ExportedLambda lambda, string name, bool awaits, IReadOnlyList<string> context, bool development, bool? database)
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

        if (lambda.License is { } license)
        {
            facts.Add(("License", $"{license.License.Id}, see LICENSE"));

            if (license.Page != null)
            {
                facts.Add(("Source", license.Page));
            }
        }

        // a published source is packed once per version and read by anybody,
        // so it carries nothing that changes from one day to the next
        if (lambda.Exported is { } exported)
        {
            facts.Add(("Exported", Day(exported)));
        }

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

        if (development)
        {
            written += "\n//\n// dev/ is what its assets or code are built from, as it was kept beside it on\n// the platform - built by whoever changed it, never by the platform.";
        }

        var mounted = database != null ? " -v \"$PWD/database:/app/database\"" : string.Empty;

        var stored = database switch
        {
            true => $"\n//\n// Its Database is {DatabaseFile}, a SQLite file holding what the app had kept\n// when it was exported.",
            false => $"\n//\n// Its Database is {DatabaseFile}, made empty the first time the app connects.",
            null => string.Empty
        };

        var provided = database != null
            ? "the Workspace\n// the app writes to, the Assets it shipped with (in assets/), the Secret it\n// reads, from environment variables of the same name, and the Database it\n// keeps its records in."
            : "the Workspace\n// the app writes to, the Assets it shipped with (in assets/), and the\n// Secret it reads, from environment variables of the same name.";

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
            //   docker run -p 8080:8080{variables}{mounted} -v {tag}-data:/app/workspace {tag}
            //
            // Project.cs holds the code of the lambda and the other .cs files are its
            // own. Platform/ stands in for what the platform provided: {provided}{stored}{environment}{written}

            using GenHTTP.Engine.Internal;
            using GenHTTP.Modules.Practices;

            await Host.Create()
                      .Handler({handler})
                      .Defaults()
                      .RunAsync();

            """;
    }

    /// <summary>
    /// The host of a lambda's repository, and a word about where it is.
    /// </summary>
    /// <remarks>
    /// Without anything that changes from one version to the next - the
    /// number, the change, the secrets the code reads - since it is the same
    /// in every commit, including those a push makes.
    /// </remarks>
    private static string RepositoryProgram(RepositoryProject project)
    {
        var name = Identifier(project.PublicKey);

        var tag = name.ToLowerInvariant();

        return $"""
            // This app is a lambda on GenHTTP Lambda ({project.Home}), where you describe
            // an app - or let your coding agent write it - and it is online at an address
            // of its own a moment later, with every version kept and a way back to each.
            //
            //   Lambda   {project.PublicKey} ({project.Address})
            //   GenHTTP  {FrameworkVersion}
            //
            // This repository is the lambda: every commit of main is one of its versions,
            // every other branch a feature being worked on. AGENTS.md says how to work on it.
            //
            // It is served by GenHTTP, an embeddable web server for .NET. What it can do,
            // and how: https://genhttp.org/documentation/
            //
            //   dotnet run                    then open http://localhost:8080/
            //
            //   docker build -t {tag} .
            //   docker run -p 8080:8080 -v {tag}-data:/app/workspace {tag}
            //
            // Project.cs holds the code of the lambda and the other .cs files here are its
            // own. Platform/ stands in for what the platform provides: the Workspace the
            // app writes to (workspace/), the Assets it ships with (assets/), the Secret it
            // reads, from environment variables of the same name, and the Database it keeps
            // its records in (database/database.db, made empty the first time it connects).
            // docs/ says what the app is for and why it is built the way it is, and tests/
            // how it is tested. dev/, where there is one, is what the assets or code are
            // built from - built by whoever changes it, never by the platform.

            using GenHTTP.Engine.Internal;
            using GenHTTP.Modules.Practices;

            await Host.Create()
                      .Handler(await Project.CreateAsync())
                      .Defaults()
                      .RunAsync();

            """;
    }

    /// <summary>
    /// What every lambda may name without a using.
    /// </summary>
    /// <remarks>
    /// The platform puts the same imports at the top of every file it
    /// compiles, which is why the other files of a lambda have none. Global
    /// here, so they compile as they are.
    /// </remarks>
    /// <remarks>
    /// SQLite, Entity Framework Core and Evolve only where the project
    /// references them, which is where the code uses them - an import of a
    /// package that is not there does not compile.
    /// </remarks>
    private static string Usings(bool data, bool entities, bool evolve)
    {
        var builder = new StringBuilder();

        builder.Append("// What the platform imported into every file of the lambda.").Append('\n');
        builder.Append('\n');

        foreach (var import in ModuleCatalog.Imports)
        {
            var referenced = import switch
            {
                ModuleCatalog.EvolveImport => evolve,
                ModuleCatalog.EntityFrameworkImport => entities,
                _ => data
            };

            if (ModuleCatalog.IsData(import) && !referenced)
            {
                continue;
            }

            builder.Append($"global using {import};").Append('\n');
        }

        builder.Append('\n');
        builder.Append("global using Platform;").Append('\n');
        builder.Append("global using static Platform.LambdaScope;").Append('\n');

        return builder.ToString();
    }

    #endregion

    #region Plumbing

    /// <summary>
    /// The folder the project of a lambda is packed into, which is also the
    /// name of the project.
    /// </summary>
    public static string Folder(string publicKey) => Identifier(publicKey);

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

    private static ProjectFile Text(string path, string content, string? name = null) => new(path, Encoding.UTF8.GetBytes(content), name);

    private static void Write(ZipArchive archive, string path, string content)
        => Write(archive, path, Encoding.UTF8.GetBytes(content));

    private static void WriteFile(ZipArchive archive, string path, string file)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);

        using var stream = entry.Open();

        using var source = File.OpenRead(file);

        source.CopyTo(stream);
    }

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
/// <param name="Exported">When the export was made, or nothing for a project packed to be published</param>
/// <param name="Secrets">The names of the secrets it keeps or reads, which become environment variables - never their values</param>
/// <param name="License">The license its source is published under, if its owner published it</param>
public sealed record ExportedLambda(string PublicKey, int Version, DateTime Saved, string? Change, string? Address, DateTime? Exported,
                                    IReadOnlyList<string>? Secrets = null, ExportedLicense? License = null);

/// <summary>
/// The license a packed project is under, written into its LICENSE.
/// </summary>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
/// <param name="Page">Where the source is published, if the installation knows its own address</param>
public sealed record ExportedLicense(SourceLicense License, string Holder, string? Page);

/// <summary>
/// One file of the project a lambda's repository holds.
/// </summary>
/// <param name="Path">Where it is in the project</param>
/// <param name="Content">What it holds</param>
/// <param name="Name">The file of the lambda it is made from, or nothing for one the platform puts around it</param>
public sealed record ProjectFile(string Path, byte[] Content, string? Name);

/// <summary>
/// What the project of a lambda's repository says about the lambda.
/// </summary>
/// <param name="PublicKey">The name of the lambda, which names the project</param>
/// <param name="Address">Where it runs</param>
/// <param name="Home">Where the installation is, which its files link to</param>
/// <param name="License">The license its source is published under, while it is</param>
public sealed record RepositoryProject(string PublicKey, string Address, string Home, RepositoryLicense? License);

/// <summary>
/// The license the project of a repository is under, written into its LICENSE.
/// </summary>
/// <param name="Year">The year the license names, which is the year the commit was made in</param>
/// <param name="Holder">Who holds the copyright, as the license names them</param>
public sealed record RepositoryLicense(SourceLicense License, int Year, string Holder);
