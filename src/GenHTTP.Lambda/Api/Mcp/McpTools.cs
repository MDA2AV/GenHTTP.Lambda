using System.Text.Json.Nodes;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

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
public sealed class McpTools(IMetaService meta, IWorkspaceService workspace, LambdaTelemetry telemetry, LogBook book, LambdaOptions options)
{

    #region Catalogue

    /// <summary>
    /// The tools, as the protocol describes them.
    /// </summary>
    public JsonArray Describe() =>
    [
        Tool("create_lambda", "Create a lambda", Effect.Create,
             "Create a lambda. Returns its public address and a private editor key, the only way back in. Nothing is online until deploy.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["publicKey"] = Field("string", "Requested address: lower case letters, digits and dashes. Generated if omitted."),
                     ["template"] = Field("string", $"Starting point, one of: {string.Join(", ", TemplateCatalog.Groups.SelectMany(g => g.Templates).Select(t => t.Id))}."),
                     ["acceptTerms"] = Field("boolean", "Must be true: the user accepts the terms in platform_guide (free shared machine, deployments may be removed, nothing malicious).")
                 },
                 ["required"] = new JsonArray("acceptTerms")
             }),

        Tool("write_code", "Save all files", Effect.Save,
             "Save all files of the lambda as a new version, replacing the previous set. lambda.cs returns the handler; other .cs files hold types; any other file is an asset, served as is and reachable as Assets. Say why with prompt and change - the owner reads them in the version history. Pass deploy: true to publish it in the same call. To change only some files, use change_code.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key from create_lambda."),
                     ["prompt"] = Field("string", $"What you were asked to do, in the words of whoever asked - the request this version answers. Kept with the version so the owner can see why it exists. Optional, up to {VersionNote.MaxPrompt} characters."),
                     ["change"] = Field("string", $"What this version changes, in one line written for the owner - 'Adds a leaderboard that keeps the ten best scores', not 'updated lambda.cs'. Optional, up to {VersionNote.MaxChange} characters."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "lambda.cs first.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", ".cs files are compiled; anything else ('www/app.css', 'logo.png') is an asset."),
                                 ["code"] = Field("string", "Contents, base64 if encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for binary assets; omit otherwise.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     },
                     ["deploy"] = Field("boolean", "Also deploy the new version, so it goes live at once.")
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("change_code", "Change some files", Effect.Save,
             "Change some files of the newest version and save the result as a new version: add or replace files, remove files, or replace text within a file. Everything not named stays as it is, so there is no need to resend unchanged files. Pass deploy: true to publish it in the same call.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["prompt"] = Field("string", $"What you were asked to do, in the words of whoever asked. Kept with the version. Optional, up to {VersionNote.MaxPrompt} characters."),
                     ["change"] = Field("string", $"What this version changes, in one line written for the owner. Optional, up to {VersionNote.MaxChange} characters."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "Files to add, or to replace where one of that name exists.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", ".cs files are compiled; anything else ('www/app.css', 'logo.png') is an asset."),
                                 ["code"] = Field("string", "Contents, base64 if encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for binary assets; omit otherwise.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     },
                     ["remove"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "Names of files to remove.",
                         ["items"] = new JsonObject { ["type"] = "string" }
                     },
                     ["edits"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "Text replacements, applied after files and remove.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["file"] = Field("string", "The file to change."),
                                 ["find"] = Field("string", "Text that occurs exactly once in the file."),
                                 ["replace"] = Field("string", "What to put in its place.")
                             },
                             ["required"] = new JsonArray("file", "find", "replace")
                         }
                     },
                     ["deploy"] = Field("boolean", "Also deploy the new version, so it goes live at once.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("check_code", "Check that code compiles", Effect.Read,
             "Compile without saving or deploying; returns diagnostics with file and line. Does not build the handler, so deploy can still refuse a route whose return type cannot be served.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "lambda.cs first.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", ".cs files are compiled; anything else is an asset."),
                                 ["code"] = Field("string", "Contents, base64 if encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for binary assets; omit otherwise.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     }
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("deploy", "Deploy a version", Effect.Replace,
             "Deploy (publish) a saved version so it goes live at its public address. Returns diagnostics on failure.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Defaults to the newest.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("read_lambda", "Read a lambda", Effect.Read,
             $"A lambda's status (online version, latest version, expiry), the recent versions with what each was asked for and changed, and the files of one version - in full when they come to at most {ReadBudget:N0} characters, otherwise by name and length, with file to read one. Read the history before changing what you did not write.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Defaults to the newest."),
                     ["file"] = Field("string", "Return only this file of the version, in full, however large the rest is.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("read_logs", "Read a lambda's logs", Effect.Read,
             "What a deployed lambda has been doing: its recent requests and how they were answered, what it printed, the errors it threw with their stack traces, and how much traffic it has had in the last hour and day. Call it after deploying to see that it works, and first when something is reported broken.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["level"] = Field("string", "The lowest level worth reading: 'info' for everything, 'warn' for problems, 'error' for failures. Left out, 'info'."),
                     ["since"] = Field("integer", "The cursor a previous call answered with, to read only what is new since then."),
                     ["limit"] = Field("integer", "At most this many lines, the newest. Left out, 100.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("upload_file", "Upload a workspace file", Effect.Replace,
             "Write a file to the lambda's workspace, a runtime directory it can read, write and serve. Takes effect immediately without a deploy - the way to ship a front end that changes independently of the code.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["path"] = Field("string", "Relative to the workspace; slashes make folders, e.g. 'site/app.css'."),
                     ["content"] = Field("string", "Text, or base64 with encoding set."),
                     ["encoding"] = Field("string", "'base64' for binary; omit otherwise.")
                 },
                 ["required"] = new JsonArray("privateKey", "path", "content")
             }),

        Tool("list_files", "List workspace files", Effect.Read,
             "List the lambda's workspace with size and last write per file. Runtime files only; the code is in read_lambda.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("delete_file", "Delete a workspace file", Effect.Replace,
             "Remove a file, or a folder with its contents, from the workspace.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["path"] = Field("string", "Relative to the workspace.")
                 },
                 ["required"] = new JsonArray("privateKey", "path")
             }),

        Tool("list_examples", "List the examples", Effect.Read,
             "Running example lambdas, basic and advanced. Read one before writing code.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }),

        Tool("read_example", "Read an example", Effect.Read,
             "All files of one running example.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["id"] = Field("string", $"One of: {string.Join(", ", ExampleCatalog.All.Select(e => e.Id))}.")
                 },
                 ["required"] = new JsonArray("id")
             }),

        Tool("platform_guide", "Read the platform guide", Effect.Read,
             "Rules for writing a lambda: what the snippet returns, what is imported, what is refused, limits and terms.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() })
    ];

    #endregion

    #region Dispatch

    /// <summary>
    /// Runs a tool and describes what happened.
    /// </summary>
    /// <param name="origin">Scheme and host the caller reached this server at, to build absolute links with</param>
    public async ValueTask<JsonObject> CallAsync(string name, JsonObject arguments, string origin = "")
    {
        try
        {
            return name switch
            {
                "create_lambda" => await CreateAsync(arguments, origin),
                "write_code" => await WriteAsync(arguments, origin),
                "change_code" => await ChangeAsync(arguments, origin),
                "check_code" => await CheckAsync(arguments),
                "deploy" => await DeployAsync(arguments, origin),
                "read_lambda" => await ReadAsync(arguments, origin),
                "read_logs" => await LogsAsync(arguments),
                "upload_file" => await UploadAsync(arguments),
                "list_files" => await FilesAsync(arguments),
                "delete_file" => await RemoveAsync(arguments),
                "list_examples" => Examples(origin),
                "read_example" => Example(arguments, origin),
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
            return McpProtocol.Refuse($"Failed: {e.Message}");
        }
    }

    private async ValueTask<JsonObject> CreateAsync(JsonObject arguments, string origin)
    {
        if (Flag(arguments, "acceptTerms") != true)
        {
            return McpProtocol.Refuse(
                "acceptTerms must be true. Show the user the terms from platform_guide first.");
        }

        var lambda = await meta.CreateAsync(Text(arguments, "publicKey"), Text(arguments, "template"));

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            privateKey = lambda.PrivateKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            editorUrl = $"{origin}/editor/{lambda.PrivateKey}",
            next = "write_code with deploy: true.",
            warning = "The editor key cannot be recovered. Give it to the user."
        });
    }

    private async ValueTask<JsonObject> WriteAsync(JsonObject arguments, string origin)
    {
        var files = Files(arguments, out var complaint);

        if (files == null)
        {
            return McpProtocol.Refuse(complaint!);
        }

        return await SaveAsync(arguments, files, origin);
    }

    private async ValueTask<JsonObject> ChangeAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        List<LambdaFile>? changed = [];

        if (arguments.ContainsKey("files") && (changed = ParseFiles(arguments, out var complaint)) == null)
        {
            return McpProtocol.Refuse(complaint!);
        }

        var remove = (arguments["remove"] as JsonArray)?.Select(n => n?.GetValue<string>() ?? "").ToList();

        var edits = (arguments["edits"] as JsonArray)?.OfType<JsonObject>()
                                                     .Select(e => new FileEdit(Text(e, "file") ?? "", Text(e, "find") ?? "", Text(e, "replace") ?? ""))
                                                     .ToList();

        var files = LambdaChanges.Apply(await Api.VersionResource.LatestAsync(meta, privateKey), changed, remove, edits);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return McpProtocol.Refuse(invalid);
        }

        return await SaveAsync(arguments, files, origin);
    }

    /// <summary>
    /// Stores the files as a new version and deploys it if the arguments ask for that.
    /// </summary>
    private async ValueTask<JsonObject> SaveAsync(JsonObject arguments, IReadOnlyList<LambdaFile> files, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var note = new VersionNote(Text(arguments, "prompt"), Text(arguments, "change"), VersionOrigins.Agent);

        var version = await meta.SaveAsync(privateKey, LambdaSource.Serialize(files), note);

        // said only when it is missing, and as a request rather than a
        // refusal: the code matters more than the note about it
        var reminder = version.Change == null
            ? "Pass change (one line on what the version does) and prompt (what you were asked) next time; the owner reads them in the version history."
            : null;

        if (Flag(arguments, "deploy") != true)
        {
            return McpProtocol.Say(new
            {
                ok = true,
                version = version.Version,
                next = "deploy",
                note = reminder
            });
        }

        return await DeployAsync(privateKey, version.Version, origin, reminder);
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

    /// <summary>
    /// Puts a file into the workspace of a lambda.
    /// </summary>
    /// <remarks>
    /// The editor has a panel for this and an agent had nothing, which made
    /// serving a front end from the workspace something it could be told
    /// about and not do.
    /// </remarks>
    private async ValueTask<JsonObject> UploadAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var path = Required(arguments, "path");

        var content = Required(arguments, "content");

        var id = await meta.GetIdAsync(privateKey);

        if (id == null)
        {
            return McpProtocol.Refuse("There is no lambda with that editor key.");
        }

        byte[] bytes;

        if (Text(arguments, "encoding") == "base64")
        {
            try
            {
                bytes = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                return McpProtocol.Refuse("The content is not valid base64.");
            }
        }
        else
        {
            bytes = System.Text.Encoding.UTF8.GetBytes(content);
        }

        using var stream = new MemoryStream(bytes);

        var written = await workspace.WriteAsync(id.Value, path, stream);

        return McpProtocol.Say(new
        {
            ok = true,
            written.Path,
            written.Size,
            note = "Served immediately, no deploy needed."
        });
    }

    private async ValueTask<JsonObject> FilesAsync(JsonObject arguments)
    {
        var id = await meta.GetIdAsync(Required(arguments, "privateKey"));

        if (id == null)
        {
            return McpProtocol.Refuse("There is no lambda with that editor key.");
        }

        var listing = await workspace.ListAsync(id.Value);

        return McpProtocol.Say(new
        {
            files = listing.Files.Select(f => new { f.Path, f.Size, f.Modified }),
            listing.Folders,
            listing.UsedBytes,
            listing.QuotaBytes
        });
    }

    private async ValueTask<JsonObject> RemoveAsync(JsonObject arguments)
    {
        var id = await meta.GetIdAsync(Required(arguments, "privateKey"));

        if (id == null)
        {
            return McpProtocol.Refuse("There is no lambda with that editor key.");
        }

        var path = Required(arguments, "path");

        await workspace.DeleteAsync(id.Value, path);

        return McpProtocol.Say(new { ok = true, path });
    }

    private ValueTask<JsonObject> DeployAsync(JsonObject arguments, string origin)
        => DeployAsync(Required(arguments, "privateKey"), Number(arguments, "version"), origin);

    private async ValueTask<JsonObject> DeployAsync(string privateKey, int? version, string origin, string? reminder = null)
    {
        var result = await meta.DeployAsync(privateKey, version, VersionOrigins.Agent);

        if (!result.Success)
        {
            return McpProtocol.Say(new
            {
                ok = false,
                problem = "Not deployed. The diagnostics say whether compiling or building the handler failed.",
                diagnostics = result.Diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message })
            }, failed: true);
        }

        var lambda = result.Lambda!;

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            version = lambda.ActiveVersion,
            onlineUntil = lambda.DeployedUntil,
            note = reminder ?? "Deploying again extends onlineUntil. Once it has been called, read_logs shows how it answered."
        });
    }

    /// <summary>
    /// What a deployed lambda has been saying and how much it is used.
    /// </summary>
    /// <remarks>
    /// An agent that deploys and never looks has no way to tell working code
    /// from code that throws on the first request. This is the same view the
    /// owner gets in the control center, with the same thing left out: who
    /// the visitors were.
    /// </remarks>
    private async ValueTask<JsonObject> LogsAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var id = await meta.GetIdAsync(privateKey)
              ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var lambda = await meta.GetAsync(privateKey);

        var since = arguments.TryGetPropertyValue("since", out var cursor) && cursor is JsonValue value && value.TryGetValue<long>(out var from)
                  ? from
                  : 0;

        var (lines, next, missed) = book.Read(since, null, Level(Text(arguments, "level")),
                                              Math.Clamp(Number(arguments, "limit") ?? 100, 1, 1000), lambdaId: id);

        var traffic = telemetry.Describe(id);

        return McpProtocol.Say(new
        {
            ok = true,
            online = lambda?.ActiveVersion != null,
            version = lambda?.ActiveVersion,
            traffic = new
            {
                lastHour = new { requests = traffic.Minutes.Sum(m => m.Requests), serverErrors = traffic.Minutes.Sum(m => m.Failed), clientErrors = traffic.Minutes.Sum(m => m.Rejected) },
                lastDay = new { requests = traffic.Quarters.Sum(m => m.Requests), serverErrors = traffic.Quarters.Sum(m => m.Failed), clientErrors = traffic.Quarters.Sum(m => m.Rejected) },
                countedSince = traffic.Since
            },
            lines = lines.Select(l => new { l.At, l.Level, l.Source, l.Text, detail = l.Detail, repeats = l.Repeats > 1 ? l.Repeats : (int?)null }),
            cursor = next,
            missed,
            capturingOutput = options.CaptureLambdaOutput,
            note = lines.Count == 0
                ? "Nothing yet. Requests appear here once somebody calls the lambda - call its public address and read again."
                : "Requests are the source 'Requests'; what the lambda printed is 'stdout' and 'stderr'; a handler that threw appears with its stack trace in detail. Pass cursor as since to read only what is new."
        });
    }

    private static Microsoft.Extensions.Logging.LogLevel Level(string? level) => level?.Trim().ToLowerInvariant() switch
    {
        "debug" => Microsoft.Extensions.Logging.LogLevel.Debug,
        "warn" or "warning" => Microsoft.Extensions.Logging.LogLevel.Warning,
        "error" => Microsoft.Extensions.Logging.LogLevel.Error,
        _ => Microsoft.Extensions.Logging.LogLevel.Information
    };

    private async ValueTask<JsonObject> ReadAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var lambda = await meta.GetAsync(privateKey)
                  ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var version = Number(arguments, "version") ?? lambda.LatestVersion;

        IReadOnlyList<LambdaFile> files = [];

        LambdaVersionContent? content = null;

        if (version is { } wanted)
        {
            content = await meta.GetVersionAsync(privateKey, wanted);

            files = LambdaSource.Parse(content.Code);
        }

        var history = await meta.GetVersionsAsync(privateKey);

        var only = Text(arguments, "file");

        object listing;

        var omitted = false;

        if (only != null)
        {
            var one = files.FirstOrDefault(f => f.Name == only)
                   ?? throw LambdaException.NotFound($"Version {version} has no file '{only}'. It has: {string.Join(", ", files.Select(f => f.Name))}.");

            listing = new[] { new { one.Name, one.Code } };
        }
        else if (files.Sum(f => f.Code.Length) <= ReadBudget)
        {
            listing = files.Select(f => new { f.Name, f.Code });
        }
        else
        {
            listing = files.Select(f => new { f.Name, length = f.Code.Length });

            omitted = true;
        }

        return McpProtocol.Say(new
        {
            ok = true,
            lambda.PublicKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            lambda.ActiveVersion,
            lambda.LatestVersion,
            online = lambda.ActiveVersion != null,
            lambda.DeployedUntil,
            lambda.KeptUntil,
            version,
            prompt = content?.Prompt,
            change = content?.Change,
            // the why of the recent past, so a change made on top of somebody
            // else's work can follow what they were trying to do - the one line
            // each, since a prompt can be a page and this is read every time
            history = history.Take(10).Select(v => new { v.Version, v.Created, v.Change, v.Origin }),
            files = listing,
            filesOmitted = omitted ? true : (bool?)null,
            note = omitted
                ? $"The files come to {files.Sum(f => f.Code.Length):N0} characters, more than one answer carries. Pass file to read one of them."
                : null
        });
    }

    /// <summary>
    /// How much file content read_lambda sends in one answer.
    /// </summary>
    /// <remarks>
    /// Clients cap what a tool may answer, and a lambda of any size went over:
    /// the answer failed as a whole and the agent saw nothing at all, not even
    /// the status. Under the budget every file comes back as before; over it
    /// the answer lists names and lengths, and file fetches one in full.
    /// </remarks>
    private const int ReadBudget = 30_000;

    private static JsonObject Examples(string origin) => McpProtocol.Say(new
    {
        ok = true,
        examples = ExampleCatalog.All.Select(e => new
        {
            e.Id,
            level = e.Level,
            e.Name,
            e.Description,
            url = $"{origin}/lambda/{e.PublicKey}/",
            files = TemplateCatalog.FilesFor(e.Id, e.PublicKey).Select(f => f.Name)
        })
    });

    private static JsonObject Example(JsonObject arguments, string origin)
    {
        var id = Text(arguments, "id");

        var example = ExampleCatalog.Find(id);

        if (example == null)
        {
            return McpProtocol.Refuse($"No example '{id}'. See list_examples.");
        }

        return McpProtocol.Say(new
        {
            ok = true,
            example.Id,
            example.Name,
            example.Description,
            url = $"{origin}/lambda/{example.PublicKey}/",
            files = ExampleCatalog.FilesFor(example).Select(f => new { f.Name, f.Code })
        });
    }

    private JsonObject Guide() => McpProtocol.Say(new
    {
        ok = true,
        preferTheApi = "If you can make HTTP requests, the REST API at https://genhttp.dev/api/v1/openapi.json does the same as these tools and costs fewer tokens, because files are sent directly. GET /api/v1/lambdas/{privateKey}/versions/{version}/zip downloads a version, POST /api/v1/lambdas/{privateKey}/versions/zip saves a zip of all files as a new version - so edit locally and push once. Every endpoint that saves a version takes ?deploy=true. Many environments cannot reach it; then use these tools.",
        whatALambdaIs = "C# that returns a GenHTTP handler, served at /lambda/{publicKey}/. No Main and no project: the snippet is the program.",
        theSnippet = new
        {
            file = LambdaSource.EntryName,
            mustReturn = "An IHandler or a builder of one: Inline.Create(), Layout.Create(), Content.From(...), Websocket.Functional(), ...",
            example = "return Inline.Create().Get(() => new Greeting(\"hello\"));\n\nrecord Greeting(string Text);",
            anonymousTypes = "Routes cannot return anonymous types; declare a record."
        },
        moreThanOneFile = new
        {
            howItWorks = "Other .cs files hold types, compiled into the same namespace as the snippet.",
            limit = LambdaSource.MaxFiles
        },
        assets = new
        {
            what = "Any file not ending in .cs. Served as is, never compiled, not counted against the code budget.",
            shipping = "Send with the code: { name: \"www/app.css\", code: \"body { margin: 0 }\" }. Binary files as base64 with encoding \"base64\".",
            reading = "Assets.Tree(), Assets.Files(), Assets.App() (single page application: index.html answers unmatched paths), Assets.Exists / ReadText / ReadBytes / List / Folders.",
            folders = "Each takes an optional folder: Assets.App(\"site\") serves site/ at the root, so site/app.css is requested as /app.css.",
            inOtherFiles = "Assets means this only in the top-level code of lambda.cs. In other files, and in types, Assets is the Files module's type of the same name - use LambdaEnvironment.Assets there.",
            serving = "return Layout.Create().Add(\"api\", api).Add(Assets.App(\"site\"));",
            contentTypes = "Inferred from the file extension.",
            limits = new
            {
                bytes = options.MaxAssetBytes,
                count = LambdaSource.MaxAssets,
                names = "Letters, digits, dashes, underscores, dots and slashes. No leading slash, no .."
            }
        },
        takingItAway = "GET /api/v1/lambdas/{privateKey}/export returns the lambda as a standalone zipped .NET project with no dependency on this platform. Worth telling the user.",
        importedForYou = ModuleCatalog.Imports,
        storage = new
        {
            what = "Workspace: a private directory the lambda can read and write at runtime, for anything that must outlive a request.",
            surface = new[]
            {
                "Workspace.ReadText(name) / WriteText(name, text)",
                "Workspace.ReadBytes(name) / WriteBytes(name, bytes)",
                "Workspace.Exists(name) / Delete(name) / List()",
                "Workspace.Tree() / Tree(folder) - as a resource tree",
                "Workspace.Files() / Files(folder) - a handler that serves it",
                "Workspace.App() / App(folder) - a single page application over it",
                "Workspace.Folders() / CreateFolder(name)",
                "Workspace.Root - where it is on disk"
            },
            reach = "Workspace can be used from every file, including types in other .cs files.",
            note = "Nothing else on the file system is reachable. There is no Append."
        },
        servingAFrontEnd = new
        {
            withTheCode = new
            {
                how = "write_code with the files (slashes make folders), deploy, serve with Assets.App() or Assets.App(\"site\").",
                whenToPreferIt = "The front end is part of the program: versioned, rolled back and cloned together with the code. Simplest to write in one pass.",
                mind = "Counts against the code budget; every change needs a deploy.",
                example = "read_example \"site\""
            },
            inTheWorkspace = new
            {
                how = "Deploy a lambda returning Layout.Create().Add(Workspace.App()), then upload_file index.html and the rest. Served immediately.",
                whenToPreferIt = "The files change more often than the code, someone else replaces them, or there are many. No redeploys, no code budget.",
                mind = "Not versioned and not cloned. Workspace.App() without a folder serves everything the lambda writes - use a folder if it writes anything else.",
                example = "read_example \"uploads\""
            },
            underTheHood = "App() is SinglePageApplication.From(tree).ServerSideRouting() over Assets.Tree() or Workspace.Tree().",
            doNotDoBoth = "Serving both at the same address makes it unclear which one answers."
        },
        generatedContent = new
        {
            tree = "VirtualTree.Create().Add(\"app.css\", Resource.FromString(css).Type(new ContentType(\"text/css\"))) builds a tree in memory.",
            singlePage = "Content.From(Resource.FromString(html).Type(new ContentType(\"text/html; charset=utf-8\")))"
        },
        sayWhy = new
        {
            what = "Every write_code takes two optional notes that are kept with the version: prompt, the request you were answering in the words it was asked in, and change, one line on what this version does. The owner reads them in the version history of the control center, next to the code and a diff against the version before.",
            why = "The code says what was done. Only you know why, and the next agent to touch this lambda - or you, a week later - reads the history with read_lambda before changing anything.",
            goodChange = "Adds a leaderboard that keeps the ten best scores on the server",
            badChange = "Updated lambda.cs",
            limits = new { prompt = VersionNote.MaxPrompt, change = VersionNote.MaxChange }
        },
        afterDeploying = new
        {
            what = "read_logs shows what the lambda has been doing: each request and its status, what it printed, and any exception it threw with the stack trace, plus its traffic over the last hour and day.",
            when = "After a deploy, call the public address and read the logs to see that it answered. When somebody says it is broken, read the logs before reading the code."
        },
        whenThingsAreChecked = new
        {
            checkCode = "Compiles only.",
            deploy = "Compiles and builds the handler. A route with a return type that cannot be served passes check_code and fails here."
        },
        refused = new
        {
            what = "Reflection, processes, the environment, the file system, and anything else that reaches the host.",
            why = "All lambdas share one process."
        },
        thingsThatCatchPeopleOut = new[]
        {
            "Request bodies bind by type: a bare string parameter is null. Take a record.",
            "A websocket cannot read the request it was upgraded from (the query throws). Send what it needs as the first frame.",
            "Concurrent writes to one socket corrupt it. Guard broadcasts with a semaphore.",
            "REST routes serialize camel case; match that on sockets.",
            "Your own type called e.g. File is fine; only the refused framework type of that name is blocked.",
            "Ship stylesheets and scripts as assets, not string constants: a raw string literal ends at the first \"\"\", and assets cost no code budget."
        },
        limits = new
        {
            code = $"{options.MaxCodeLength} characters across all files",
            deployment = $"online while used; offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits",
            retention = $"removed about {(int)options.Retention.TotalDays} days after the last of either"
        },
        terms = SystemResource.Terms
    });

    #endregion

    #region Arguments

    private static JsonObject Tool(string name, string title, Effect effect, string description, JsonObject schema) => new()
    {
        ["name"] = name,
        ["title"] = title,
        ["description"] = description,
        ["inputSchema"] = schema,
        ["annotations"] = new JsonObject
        {
            ["title"] = title,
            ["readOnlyHint"] = effect.ReadOnly,
            ["destructiveHint"] = effect.Destructive,
            ["idempotentHint"] = effect.Idempotent,
            // every tool acts on this platform and nothing beyond it
            ["openWorldHint"] = false
        }
    };

    /// <summary>
    /// What calling a tool does to the platform, as the protocol's hints say it.
    /// </summary>
    /// <remarks>
    /// Clients read these to decide what to ask the user before a call - a
    /// read can go through, a deploy is worth a question. Without them every
    /// tool looks like the worst case, which is what the defaults assume.
    /// </remarks>
    private readonly record struct Effect(bool ReadOnly, bool Destructive, bool Idempotent)
    {
        /// <summary>Looks and changes nothing.</summary>
        public static Effect Read => new(true, false, true);

        /// <summary>Adds something new and replaces nothing.</summary>
        public static Effect Create => new(false, false, false);

        /// <summary>Adds a version, and with deploy: true replaces what is online.</summary>
        public static Effect Save => new(false, true, false);

        /// <summary>Replaces or removes what was there, the same way however often.</summary>
        public static Effect Replace => new(false, true, true);
    }

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
        var files = ParseFiles(arguments, out complaint);

        if (files == null)
        {
            return null;
        }

        complaint = LambdaSource.Validate(files);

        return complaint == null ? files : null;
    }

    /// <summary>
    /// The files an argument list carries, without checking them as a whole.
    /// </summary>
    private static List<LambdaFile>? ParseFiles(JsonObject arguments, out string? complaint)
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

        return files;
    }

    #endregion

}
