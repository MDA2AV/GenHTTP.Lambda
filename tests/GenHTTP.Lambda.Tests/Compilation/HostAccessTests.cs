using GenHTTP.Lambda.Tests.Infrastructure;

namespace GenHTTP.Lambda.Tests.Compilation;

/// <summary>
/// A lambda reaches the disk, the environment and the logging of the host
/// through no library at all, however the code is written.
/// </summary>
/// <remarks>
/// Each of these was a way onto the host before the guard read names for what
/// they mean and a lambda was compiled against an allow-list of assemblies.
/// </remarks>
[TestClass]
public sealed class HostAccessTests
{

    private const string Served = "\nreturn Content.From(Resource.FromString(\"x\"));";

    [TestMethod]
    [DataRow("var s = new StreamReader(\"/data/secrets.key\").ReadToEnd();", "a reader opened on a path")]
    [DataRow("var w = new StreamWriter(\"/data/x\");", "a writer opened on a path")]
    [DataRow("var d = System.Xml.Linq.XDocument.Load(\"/data/x.xml\");", "a document loaded from a path")]
    [DataRow("new System.Xml.Linq.XDocument().Save(\"/data/x.xml\");", "a document saved to a path")]
    [DataRow("var r = System.Xml.XmlReader.Create(\"/data/x.xml\");", "an XML reader on a path")]
    [DataRow("var r = new System.Xml.XmlUrlResolver();", "a resolver that opens file:// addresses")]
    [DataRow("var a = new System.Net.Mail.Attachment(\"/data/lambda.db\");", "a mail attachment read from a path")]
    [DataRow("var s = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);\ns.SendFile(\"/data/lambda.db\");", "a file sent down a socket")]
    [DataRow("var e = new System.Net.Sockets.UnixDomainSocketEndPoint(\"/var/run/docker.sock\");", "a socket of the machine")]
    [DataRow("var t = new System.Data.DataTable();\nt.ReadXml(\"/data/x.xml\");", "a table read from a path")]
    [DataRow("var assets = GenHTTP.Modules.Files.Assets.From(\"/data\");", "the files of any directory served as assets")]
    [DataRow("var asset = Asset.From(\"/data/lambda.db\");", "any file served as an asset")]
    [DataRow("using (var c = Database.GetConnection())\n{\n    c.LoadExtension(\"/tmp/x.so\");\n}", "native code loaded into the lambda's connection")]
    [DataRow("var r = System.Resources.ResourceManager.CreateFileBasedResourceManager(\"x\", \"/data\", null);", "a resource set read from a directory")]
    [DataRow("using X = System.IO.StreamReader;\nvar s = new X(\"/data/secrets.key\");", "an alias of a type whose path overload is refused")]
    public async Task APathOnTheHostIsRefused(string code, string what)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code + Served);

        Assert.IsFalse(outcome.Success, $"{what} must be refused");
        Assert.IsTrue(outcome.Diagnostics.Any(d => d.Id == "LAMBDA0001"), $"{what} is refused by the guard, saying why: {string.Join("; ", outcome.Diagnostics.Select(d => d.Message))}");
    }

    [TestMethod]
    [DataRow("using System.IO.MemoryMappedFiles;\nvar m = MemoryMappedFile.CreateFromFile(\"/data/lambda.db\");", "memory mapped files")]
    [DataRow("var m = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(\"/data/lambda.db\");", "memory mapped files, written out")]
    [DataRow("System.IO.Compression.ZipFile.ExtractToDirectory(\"/tmp/x.zip\", \"/app\");", "zip extraction to a directory")]
    [DataRow("System.Formats.Tar.TarFile.ExtractToDirectory(\"/tmp/x.tar\", \"/app\", true);", "tar extraction to a directory")]
    [DataRow("var s = Microsoft.VisualBasic.FileIO.FileSystem.ReadAllText(\"/data/secrets.key\");", "Visual Basic's file system")]
    [DataRow("var e = Microsoft.VisualBasic.Interaction.Environ(\"LAMBDA_ADMIN_TOKEN\");", "Visual Basic's environment")]
    [DataRow("var p = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(\"/data\");", "a file provider of ASP.NET Core")]
    [DataRow("var c = new Microsoft.Extensions.Configuration.ConfigurationBuilder();", "a configuration that reads the environment")]
    [DataRow("var r = System.Net.WebRequest.Create(\"file:///data/lambda.db\");", "a web request for a file:// address")]
    [DataRow("var b = new System.Net.WebClient().DownloadData(\"file:///data/lambda.db\");", "a web client for a file:// address")]
    [DataRow("var r = new MaxMind.Db.Reader(\"/data/lambda.db\");", "a library the server uses for itself")]
    [DataRow("var w = new System.IO.FileSystemWatcher(\"/data\");", "watching the file system")]
    [DataRow("var e = new System.IO.Enumeration.FileSystemEnumerable<string>(\"/data\", (ref System.IO.Enumeration.FileSystemEntry e) => e.ToFullPath());", "enumerating the file system")]
    public async Task ALibraryThatReachesTheHostIsNotThere(string code, string what)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code + Served);

        Assert.IsFalse(outcome.Success, $"{what} must not be available");
    }

    [TestMethod]
    [DataRow("return Inline.Create().Get((IRequest request) => { request.Server.Logging.AddProvider(null); return \"x\"; });", "a provider added to the platform's logging")]
    [DataRow("return Inline.Create().Get((IRequest request) => request.Server.Handler.ToString());", "the platform's handler")]
    [DataRow("return Inline.Create().Get((IRequest request) => request.Server.EndPoints.Count);", "the platform's end points")]
    [DataRow("return Layout.Create().Add(\"x\", new Tap());\n\nclass Tap : IHandler\n{\n    public ValueTask PrepareAsync(IServer server) { server.Logging.AddProvider(null); return ValueTask.CompletedTask; }\n    public ValueTask<IResponse?> HandleAsync(IRequest request) => new((IResponse?)null);\n}", "the logging of the server a handler is handed")]
    [DataRow("Console.SetOut(new StringWriter());" + Served, "the console replaced")]
    [DataRow("using static System.Console;\nSetOut(new StringWriter());" + Served, "the console replaced through a using static")]
    public async Task ThePlatformsServerAndConsoleAreRefused(string code, string what)
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync(code);

        Assert.IsFalse(outcome.Success, $"{what} must be refused");
        Assert.IsTrue(outcome.Diagnostics.Any(d => d.Id == "LAMBDA0001"), $"{what} is refused by the guard: {string.Join("; ", outcome.Diagnostics.Select(d => d.Message))}");
    }

    [TestMethod]
    public async Task ALineDirectiveOfTheAuthorsOwnIsRefused()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        // a span directive would move the code into what reads as the
        // platform's scaffolding, which the guard does not inspect
        var outcome = await fixture.Deployments.ValidateAsync("#line (1, 1) - (1, 10) \"x.cs\"\nvar s = new StreamReader(\"/data/secrets.key\");" + Served);

        Assert.IsFalse(outcome.Success);
    }

    [TestMethod]
    public async Task StreamsXmlAndHandlersOfItsOwnStayAvailable()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var outcome = await fixture.Deployments.ValidateAsync("""
            using System.IO.Compression;
            using System.Xml.Linq;

            var document = XDocument.Parse("<feed><item>one</item></feed>");
            var loaded = XDocument.Load(new StringReader(document.ToString()));

            var buffer = new MemoryStream();

            using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, true))
            {
                using var entry = new StreamWriter(zip.CreateEntry("feed.xml").Open());
                entry.Write(loaded.ToString());
            }

            using (var gzip = new GZipStream(new MemoryStream(), CompressionLevel.Fastest))
            {
                gzip.Write(buffer.ToArray());
            }

            var text = new StreamReader(new MemoryStream(buffer.ToArray())).ReadToEnd();

            Console.WriteLine(typeof(Greeting).Name + text.Length);

            return Layout.Create().Add("hello", new Greeting());

            class Greeting : IHandler
            {
                // handed the server, to pass on and to read what it is
                public ValueTask PrepareAsync(IServer server) => server.Development ? ValueTask.CompletedTask : ValueTask.CompletedTask;

                public ValueTask<IResponse?> HandleAsync(IRequest request)
                    => new(request.Respond().Content("hello").Build());
            }
            """);

        Assert.IsTrue(outcome.Success, string.Join("; ", outcome.Diagnostics.Select(d => d.Message)));
    }

}
