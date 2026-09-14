using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Mcp;

/// <summary>
/// What an agent can actually do here.
/// </summary>
/// <remarks>
/// The set is the shape of the job: make a lambda, write code into it, check
/// that it compiles, put it online. Reading the examples is in here too,
/// because the fastest way to learn what this platform will accept is to read
/// something it is already running.
///
/// Every tool answers with an object rather than prose. A model reads the text
/// and a program reads the structured copy, and both are the same thing.
/// </remarks>
public sealed class McpTools(IMetaService meta, LambdaOptions options)
{

    #region Catalogue

    /// <summary>
    /// The tools, as the protocol describes them.
    /// </summary>
    public JsonArray Describe() =>
    [
        Tool("create_lambda",
             "Create a lambda and get the two links that belong to it: a public address where what you build is served, and a private editor link that is the only way back in. Nothing is online until you deploy.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["publicKey"] = Field("string", "The address to ask for, lower case letters, digits and dashes. Left out, one is generated."),
                     ["template"] = Field("string", $"What to start from. One of: {string.Join(", ", TemplateCatalog.Groups.SelectMany(g => g.Templates).Select(t => t.Id))}."),
                     ["acceptTerms"] = Field("boolean", "Must be true. The person you are acting for accepts that this is a free shared machine, that anything deployed may be removed, and that nothing malicious goes on it.")
                 },
                 ["required"] = new JsonArray("acceptTerms")
             }),

        Tool("write_code",
             "Store the source of a lambda as a new version. The first file is always lambda.cs, the snippet that returns a handler; further .cs files hold types; anything that is not .cs is an asset - a stylesheet, a script, a page, an image - which is served rather than compiled and reached from the snippet as Assets. This does not put anything online.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key from create_lambda."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "The files, lambda.cs first.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", "The first must be lambda.cs. Ending in .cs to be compiled; anything else, such as 'www/app.css' or 'logo.png', is an asset and is served as it is."),
                                 ["code"] = Field("string", "The contents, or base64 when encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for an asset that is not text. Leave it out otherwise.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     }
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("check_code",
             "Compile the C# without storing or deploying it, and get the compiler's complaints back with the file and line they are about. Cheaper than deploying to find out. It does not build the handler, which happens at deploy - a route whose return type GenHTTP cannot serve compiles here and is refused there.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "The files to compile, lambda.cs first.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", "Ending in .cs to be compiled; anything else is an asset and is not."),
                                 ["code"] = Field("string", "The contents, or base64 when encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for an asset that is not text.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     }
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("deploy",
             "Put a stored version online. Answers with whether it compiled, what the compiler said if not, and the address it is now being served at.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Which version. Left out, the newest.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("read_lambda",
             "What a lambda is: which version is online, how many there are, when it stops being served and the code of the version you ask for.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Which version to read. Left out, the newest.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("list_examples",
             "The working lambdas this installation keeps online, basic and advanced. Read one before writing anything: they are the shortest description of what this platform will accept.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }),

        Tool("read_example",
             "Every file of one example, as it is running right now.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["id"] = Field("string", $"Which one: {string.Join(", ", ExampleCatalog.All.Select(e => e.Id))}.")
                 },
                 ["required"] = new JsonArray("id")
             }),

        Tool("platform_guide",
             "How to write a lambda that compiles here: what the snippet has to return, what is imported for you, what is refused, and what the limits are.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() })
    ];

    #endregion

    #region Dispatch

    /// <summary>
    /// Runs a tool and describes what happened.
    /// </summary>
    public async ValueTask<JsonObject> CallAsync(string name, JsonObject arguments)
    {
        try
        {
            return name switch
            {
                "create_lambda" => await CreateAsync(arguments),
                "write_code" => await WriteAsync(arguments),
                "check_code" => await CheckAsync(arguments),
                "deploy" => await DeployAsync(arguments),
                "read_lambda" => await ReadAsync(arguments),
                "list_examples" => Examples(),
                "read_example" => Example(arguments),
                "platform_guide" => Guide(),
                _ => McpProtocol.Refuse($"There is no tool called '{name}'.")
            };
        }
        catch (LambdaException e)
        {
            // the platform refusing something is an answer, not a fault: the
            // model should read why and try again rather than see a protocol
            // error and give up
            return McpProtocol.Refuse(e.Message);
        }
        catch (Exception e)
        {
            return McpProtocol.Refuse($"That did not work: {e.Message}");
        }
    }

    private async ValueTask<JsonObject> CreateAsync(JsonObject arguments)
    {
        if (Flag(arguments, "acceptTerms") != true)
        {
            return McpProtocol.Refuse(
                "acceptTerms has to be true. Show the terms to whoever you are acting for first - platform_guide has them.");
        }

        var lambda = await meta.CreateAsync(Text(arguments, "publicKey"), Text(arguments, "template"));

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            privateKey = lambda.PrivateKey,
            publicUrl = $"/lambda/{lambda.PublicKey}/",
            editorUrl = $"/editor/{lambda.PrivateKey}",
            next = "write_code, then deploy. Nothing is online until you deploy.",
            warning = "The editor key is the only way back into this lambda and there is no way to recover it. Give it to the person you are acting for."
        });
    }

    private async ValueTask<JsonObject> WriteAsync(JsonObject arguments)
    {
        var files = Files(arguments, out var complaint);

        if (files == null)
        {
            return McpProtocol.Refuse(complaint!);
        }

        var version = await meta.SaveAsync(Required(arguments, "privateKey"), LambdaSource.Serialize(files));

        return McpProtocol.Say(new
        {
            ok = true,
            version = version.Version,
            next = "deploy, or check_code first if you would rather see the compiler's complaints without storing anything."
        });
    }

    private async ValueTask<JsonObject> CheckAsync(JsonObject arguments)
    {
        var files = Files(arguments, out var complaint);

        if (files == null)
        {
            return McpProtocol.Refuse(complaint!);
        }

        var outcome = await meta.CheckAsync(Required(arguments, "privateKey"), LambdaSource.Serialize(files));

        return McpProtocol.Say(new
        {
            ok = outcome.Success,
            compiles = outcome.Success,
            diagnostics = outcome.Diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message })
        });
    }

    private async ValueTask<JsonObject> DeployAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var result = await meta.DeployAsync(privateKey, Number(arguments, "version"));

        if (!result.Success)
        {
            return McpProtocol.Say(new
            {
                ok = false,
                problem = "Nothing was put online. The messages say whether it failed to compile or whether the handler it returned could not be served - the second happens here rather than in check_code.",
                diagnostics = result.Diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message })
            }, failed: true);
        }

        var lambda = result.Lambda!;

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            publicUrl = $"/lambda/{lambda.PublicKey}/",
            version = lambda.ActiveVersion,
            onlineUntil = lambda.DeployedUntil,
            note = "Deploying again extends it. Whatever the snippet returned is being served at the address above."
        });
    }

    private async ValueTask<JsonObject> ReadAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var lambda = await meta.GetAsync(privateKey)
                  ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var version = Number(arguments, "version") ?? lambda.LatestVersion;

        IReadOnlyList<LambdaFile> files = [];

        if (version is { } wanted)
        {
            files = LambdaSource.Parse((await meta.GetVersionAsync(privateKey, wanted)).Code);
        }

        return McpProtocol.Say(new
        {
            ok = true,
            lambda.PublicKey,
            publicUrl = $"/lambda/{lambda.PublicKey}/",
            lambda.ActiveVersion,
            lambda.LatestVersion,
            online = lambda.ActiveVersion != null,
            lambda.DeployedUntil,
            lambda.KeptUntil,
            version,
            files = files.Select(f => new { f.Name, f.Code })
        });
    }

    private static JsonObject Examples() => McpProtocol.Say(new
    {
        ok = true,
        examples = ExampleCatalog.All.Select(e => new
        {
            e.Id,
            level = e.Level,
            e.Name,
            e.Description,
            url = $"/lambda/{e.PublicKey}/",
            files = TemplateCatalog.FilesFor(e.Id, e.PublicKey).Select(f => f.Name)
        })
    });

    private static JsonObject Example(JsonObject arguments)
    {
        var id = Text(arguments, "id");

        var example = ExampleCatalog.Find(id);

        if (example == null)
        {
            return McpProtocol.Refuse($"There is no example called '{id}'. Ask list_examples.");
        }

        return McpProtocol.Say(new
        {
            ok = true,
            example.Id,
            example.Name,
            example.Description,
            url = $"/lambda/{example.PublicKey}/",
            files = ExampleCatalog.FilesFor(example).Select(f => new { f.Name, f.Code })
        });
    }

    private JsonObject Guide() => McpProtocol.Say(new
    {
        ok = true,
        whatALambdaIs = "A snippet of C# that returns a GenHTTP handler. Whatever it returns is served at /lambda/{publicKey}/. There is no Main, no project and no build: the snippet is the program.",
        theSnippet = new
        {
            file = LambdaSource.EntryName,
            mustReturn = "An IHandler or something that builds one - Inline.Create(), Layout.Create(), Content.From(...), Websocket.Functional() and so on.",
            example = "return Inline.Create().Get(() => new Greeting(\"hello\"));\n\nrecord Greeting(string Text);",
            anonymousTypes = "A route may not return an anonymous type. Declare a record and return that."
        },
        moreThanOneFile = new
        {
            howItWorks = "Further .cs files are ordinary C# holding types. They are compiled into the same namespace, so the snippet reaches them without a using.",
            limit = LambdaSource.MaxFiles
        },
        assets = new
        {
            what = "Any file whose name does not end in .cs. It is served as it is, never compiled, and does not spend any of the code budget.",
            shipping = "Send it with the code: { name: \"www/app.css\", code: \"body { margin: 0 }\" }. For anything that is not text, base64 it and set encoding to \"base64\".",
            reading = "The snippet reaches them as Assets: Assets.Tree() is a resource tree, Assets.Files() a handler that serves them, Assets.App() a single page application whose index.html answers any path that matches no file. Assets.Exists / ReadText / ReadBytes / List / Folders are there too.",
            folders = "Every one of those takes a folder name as well: Assets.App(\"site\") serves the folder site as the application, with site/index.html as its shell, and the folder's own name is not part of any address - site/app.css is asked for as /app.css. Put the front end in a folder and the root of the lambda stays free for anything else.",
            serving = "return Layout.Create().Add(\"api\", api).Add(Assets.App(\"site\"));",
            contentTypes = "Inferred from the extension, so name things properly and nothing else has to be said.",
            limits = new
            {
                bytes = options.MaxAssetBytes,
                count = LambdaSource.MaxAssets,
                names = "Letters, digits, dashes, underscores, dots and slashes. Folders are allowed; leading slashes and .. are not."
            },
            whyNotAStringConstant = "Because a raw string literal ends at the first \"\"\" inside it, which ordinary JavaScript contains, and because base64 in a string costs a third more than the bytes it carries."
        },
        importedForYou = ModuleCatalog.Imports,
        storage = new
        {
            what = "A Workspace object, which is a private directory this lambda may read and write. Use it for anything that has to outlive a request. Assets are what the lambda shipped; the workspace is what it has written since.",
            surface = new[]
            {
                "Workspace.ReadText(name) / WriteText(name, text)",
                "Workspace.ReadBytes(name) / WriteBytes(name, bytes)",
                "Workspace.Exists(name) / Delete(name) / List()",
                "Workspace.Tree() - the folder as a resource tree",
                "Workspace.Files() - a handler that serves the folder",
                "Workspace.Root - where it is on disk"
            },
            note = "Nothing else on the file system is reachable, and there is no Append."
        },
        servingAPage = new
        {
            fromAssets = "Assets.App() is the whole of it for a single page application: index.html is the shell and unmatched paths are answered with it, so client side routes are real addresses.",
            fromAFolder = "Assets.App(\"site\") does the same for one folder, which is what to use when the page is not the only thing the lambda ships. Name the files site/index.html, site/app.css, site/app.js and serve them with that one line; they are reached at /, /app.css, /app.js.",
            fromStrings = "VirtualTree.Create().Add(\"app.css\", Resource.FromString(css).Type(new ContentType(\"text/css\"))) builds a tree in memory, for when a file is generated rather than shipped.",
            oneFile = "Content.From(Resource.FromString(html).Type(new ContentType(\"text/html; charset=utf-8\"))) serves a single page with no tree at all.",
            revalidation = "Trees answer with an ETag and a 304, so a browser stops re-fetching what it already has."
        },
        whenThingsAreChecked = new
        {
            checkCode = "Compiles the C#. It does not build the handler.",
            deploy = "Compiles and then builds the handler, which is where a route whose return type cannot be served is refused. A lambda can pass check_code and be refused by deploy for that reason."
        },
        refused = new
        {
            what = "Reflection, processes, the environment, the file system, and anything that would reach the host rather than serve a request.",
            why = "Every lambda here runs in one process beside everybody else's."
        },
        thingsThatCatchPeopleOut = new[]
        {
            "A request body is bound by its type. Asking for a bare string hands you null; take a record instead.",
            "A websocket cannot read the request it was upgraded from - reaching for the query throws. Send anything it needs as the first frame.",
            "Two writes to the same socket at once corrupt it. Put a semaphore around a broadcast.",
            "The REST routes answer in camel case, so anything sent down a socket should match.",
            "A name you declare yourself is yours: a helper called File is fine. It is the type of that name, reached through its namespace, that is refused.",
            "Do not paste a stylesheet or a script into a string constant. Ship it as an asset - it is served as it is and costs none of the code budget."
        },
        limits = new
        {
            code = $"{options.MaxCodeLength} characters across all files",
            deployment = $"about {(int)options.DeploymentLifetime.TotalHours} hours online, extended by deploying again",
            retention = $"removed about {(int)options.Retention.TotalDays} days after it was last touched"
        },
        terms = SystemResource.Terms
    });

    #endregion

    #region Arguments

    private static JsonObject Tool(string name, string description, JsonObject schema) => new()
    {
        ["name"] = name,
        ["description"] = description,
        ["inputSchema"] = schema
    };

    private static JsonObject Field(string type, string description) => new()
    {
        ["type"] = type,
        ["description"] = description
    };

    private static string? Text(JsonObject arguments, string name)
        => arguments.TryGetPropertyValue(name, out var value) && value is JsonValue text && text.TryGetValue<string>(out var result)
         ? result
         : null;

    private static string Required(JsonObject arguments, string name)
        => Text(arguments, name) ?? throw LambdaException.Invalid($"'{name}' is needed and was not given.");

    private static int? Number(JsonObject arguments, string name)
        => arguments.TryGetPropertyValue(name, out var value) && value is JsonValue number && number.TryGetValue<int>(out var result)
         ? result
         : null;

    private static bool? Flag(JsonObject arguments, string name)
        => arguments.TryGetPropertyValue(name, out var value) && value is JsonValue flag && flag.TryGetValue<bool>(out var result)
         ? result
         : null;

    /// <summary>
    /// The files an argument list carries, or null and a complaint.
    /// </summary>
    private static IReadOnlyList<LambdaFile>? Files(JsonObject arguments, out string? complaint)
    {
        complaint = null;

        if (!arguments.TryGetPropertyValue("files", out var value) || value is not JsonArray array)
        {
            complaint = "'files' has to be an array of { name, code }, with lambda.cs first.";
            return null;
        }

        var files = new List<LambdaFile>();

        foreach (var entry in array)
        {
            if (entry is not JsonObject file)
            {
                complaint = "Every file has to be an object with a name and some code.";
                return null;
            }

            files.Add(new LambdaFile(Text(file, "name") ?? "", Text(file, "code") ?? "", Text(file, "encoding")));
        }

        complaint = LambdaSource.Validate(files);

        return complaint == null ? files : null;
    }

    #endregion

}
