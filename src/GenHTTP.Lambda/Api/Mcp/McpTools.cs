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
             "Store code as a new version of a lambda. Takes one file or several: the first is always lambda.cs, the snippet that returns a handler, and the rest are ordinary C# holding types. This does not put anything online.",
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
                                 ["name"] = Field("string", "Ending in .cs. The first must be lambda.cs."),
                                 ["code"] = Field("string", "The contents of the file.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     }
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("check_code",
             "Compile code without storing or deploying it, and get the compiler's complaints back with the file and line they are about. Cheaper than deploying to find out.",
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
                                 ["name"] = Field("string", "Ending in .cs."),
                                 ["code"] = Field("string", "The contents of the file.")
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
                problem = "It did not compile, so nothing was put online.",
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
            howItWorks = "Files after lambda.cs are ordinary C# holding types. They are compiled into the same namespace, so the snippet reaches them without a using.",
            limit = LambdaSource.MaxFiles
        },
        importedForYou = ModuleCatalog.Imports,
        storage = new
        {
            what = "A Workspace object, which is a private directory this lambda may read and write. Use it for anything that has to outlive a request.",
            how = "Workspace.WriteText(\"things.json\", json); Workspace.ReadText(\"things.json\"); Workspace.Exists(\"things.json\")",
            note = "Nothing else on the file system is reachable."
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
            "The REST routes answer in camel case, so anything sent down a socket should match."
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

            files.Add(new LambdaFile(Text(file, "name") ?? "", Text(file, "code") ?? ""));
        }

        complaint = LambdaSource.Validate(files);

        return complaint == null ? files : null;
    }

    #endregion

}
