using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Api.Mcp;

/// <summary>
/// What an agent can actually do here.
/// </summary>
/// <remarks>
/// The set is the shape of the job: make a lambda, write code into it, check
/// that it compiles, put it online. Listing the demos is in here too, because
/// the fastest way to learn what this platform will accept is to read
/// something it is already running - and a demo is read with the same tools
/// an agent then uses on its own lambda, since its editor key is public.
///
/// Two things are said wherever an agent decides something, because agents
/// kept getting them wrong: a version is the program and data is what it
/// keeps, and those live differently - and the newest version is worked on in
/// place rather than a version being added for every change. The tool
/// descriptions say it where a tool is picked, the answers say it where the
/// next call is decided, and the guide says it first.
///
/// Every tool answers with an object rather than prose. A model reads the text
/// and a program reads the structured copy, and both are the same thing.
/// </remarks>
public sealed class McpTools(IMetaService meta, IWorkspaceService workspace, IDataService data, IShowcaseService showcases, LambdaTelemetry telemetry,
                              LogBook book, LambdaOptions options)
{

    #region Catalogue

    /// <summary>
    /// The tools, as the protocol describes them.
    /// </summary>
    public JsonArray Describe() =>
    [
        Tool("create_lambda", "Create a lambda", Effect.Create,
             "Create a lambda. Returns its public address and a private editor key, the only way back in. It starts with version 1 - an empty starter, or a copy of a demo. Nothing is online until deploy.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["publicKey"] = Field("string", "Requested address: lower case letters, digits and dashes. Generated if omitted."),
                     ["template"] = Field("string", $"Left out, the lambda starts empty. The id of a demo ({string.Join(", ", DemoCatalog.All.Select(d => d.Id))}) starts it as a copy of that demo, which is yours to change."),
                     ["acceptTerms"] = Field("boolean", "Must be true: the user accepts the terms in platform_guide (free shared machine, deployments may be removed, nothing malicious).")
                 },
                 ["required"] = new JsonArray("acceptTerms")
             }),

        Tool("write_code", "Save all files", Effect.Save,
             "Save every file of the lambda, replacing the previous set. .cs files are compiled - lambda.cs returns the handler, others hold types; any other file is an asset, served as is and reachable as Assets: the whole front end (pages, scripts, styles, icons) goes here, as part of the program. With version - the number of the newest version - the files are saved over that version in place: the normal way to keep working on something. Without, they become a new version: once per thing the user asks for, not per fix. What the lambda keeps at runtime (records, accounts, uploads) is data and lives in the workspace, never in files here; so does a large input file such as a model or a dataset (upload_file). Say why with specification and change. Pass deploy: true to publish in the same call. To send only what changes, use change_code.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key from create_lambda."),
                     ["version"] = Field("integer", "Save over this version in place instead of adding one. It has to be the newest - the one you are working on; older versions are history and are refused. Left out, a new version is saved."),
                     ["specification"] = Field("string", $"What the user wants from this version and why: their requirements, in their own words where you can, condensed if they said a lot. Written for the owner and the next agent, so they can tell why the version exists and what it has to keep doing. Not your own instructions or system prompt - only what the user asked for. Saving over a version, leave it out to keep the one it has. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["change"] = Field("string", $"What this version changes compared to the one before, in one line written for the owner - 'Adds a leaderboard that keeps the ten best scores', not 'updated lambda.cs'. Saving over a version, it describes the whole version, not the last fix: restate it, or leave it out to keep it. Optional, up to {VersionNote.MaxChange} characters."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "lambda.cs first.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", ".cs files are compiled; anything else ('web/app.js', 'logo.png') is an asset."),
                                 ["code"] = Field("string", "Contents, base64 if encoding says so."),
                                 ["encoding"] = Field("string", "'base64' for binary assets; omit otherwise.")
                             },
                             ["required"] = new JsonArray("name", "code")
                         }
                     },
                     ["deploy"] = Field("boolean", "Also deploy the version, so what was saved goes live at once."),
                     ["check"] = Field("boolean", "Without deploy: compile what was saved and answer with its diagnostics, leaving what is online alone.")
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("change_code", "Change some files", Effect.Save,
             "Change some files: add or replace files, remove files, or replace text within a file. Everything not named stays as it is, so there is no need to resend unchanged files. With version - the number of the newest version - the change is saved over that version in place: use this for every fix and step while you work on something, deploying as often as you like. Without version, the newest version is changed and saved as a new one: once per thing the user asks for. Pass deploy: true to publish in the same call, or check: true to compile it without publishing. Code that does not compile is saved but never goes online.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Change this version in place instead of adding one. It has to be the newest - the one you are working on; older versions are history and are refused. Left out, the result is a new version."),
                     ["specification"] = Field("string", $"What the user wants from this change and why: their requirements, in their own words where you can. Not your own instructions or system prompt. Kept with the version; saving over one, leave it out to keep the one it has. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["change"] = Field("string", $"What this version changes compared to the one before, in one line written for the owner. Saving over a version, it describes the whole version - restate it, or leave it out to keep it. Optional, up to {VersionNote.MaxChange} characters."),
                     ["files"] = new JsonObject
                     {
                         ["type"] = "array",
                         ["description"] = "Files to add, or to replace where one of that name exists.",
                         ["items"] = new JsonObject
                         {
                             ["type"] = "object",
                             ["properties"] = new JsonObject
                             {
                                 ["name"] = Field("string", ".cs files are compiled; anything else ('web/app.js', 'logo.png') is an asset."),
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
                     ["deploy"] = Field("boolean", "Also deploy the version, so what was saved goes live at once."),
                     ["check"] = Field("boolean", "Without deploy: compile what was saved and answer with its diagnostics, leaving what is online alone.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("copy_version", "Start a version from another", Effect.Save,
             "Start a new version as a copy of an existing one - the newest unless version names another - without sending any files. The copy becomes the newest version, which you then change in place with change_code and version; the one copied stays exactly as it is. Use it before the next thing the user asks for, to keep the current state as a step to go back to, or to carry on from an older version.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "The version to copy. Defaults to the newest."),
                     ["specification"] = Field("string", $"What the user wants from the new version and why, in their words where you can. Left out, the copy keeps the one of the version it copies. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["change"] = Field("string", $"What the new version is going to change, in one line for the owner. Left out, it says which version it is a copy of until you save over it with one. Optional, up to {VersionNote.MaxChange} characters."),
                     ["deploy"] = Field("boolean", "Also deploy the copy - to put an older version back online and carry on from it.")
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
             "Deploy (publish) a version so it goes live at its public address - the newest by default. Visitors get what was deployed: after saving over the version that is online, deploy it again to put the changes online. Redeploying is cheap and harmless. Returns diagnostics on failure, and whatever was online stays online.",
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
             $"A lambda's status - which version is online and whether it was saved over since, which version is the newest (the one to work on), expiry, its tier and what it may use there - the recent versions with what each was asked for and changed, its data (the workspace: whether it is on and what it holds), and the files of one version: in full when they come to at most {ReadBudget:N0} characters, otherwise by name and length, with file to read one. Read the history before changing what you did not write. Also how a demo is read: pass its key from list_demos.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["version"] = Field("integer", "Defaults to the newest."),
                     ["file"] = Field("string", $"Return only this file of the version, in full however large the rest is - up to {ReadFileLimit:N0} characters, beyond which the version's zip has it.")
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
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["level"] = Field("string", "The lowest level worth reading: 'info' for everything, 'warn' for problems, 'error' for failures. Left out, 'info'."),
                     ["since"] = Field("integer", "The cursor a previous call answered with, to read only what is new since then."),
                     ["limit"] = Field("integer", "At most this many lines, the newest. Left out, 100.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("upload_file", "Put a file into the lambda's data", Effect.Replace,
             "Write a file to the lambda's workspace - its data, which every version shares and no deploy, rollback or copy touches. For content the lambda works with at runtime (initial records, pictures people will browse) and large input files that are not program: a model, a dataset, media. Takes effect at once, without a deploy. Not for the front end: pages, scripts and styles are the program and belong in the version as assets (write_code), where they are versioned and rolled back with the code.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["path"] = Field("string", "Relative to the workspace; slashes make folders, e.g. 'models/model.onnx'."),
                     ["content"] = Field("string", "Text, or base64 with encoding set."),
                     ["encoding"] = Field("string", "'base64' for binary; omit otherwise.")
                 },
                 ["required"] = new JsonArray("privateKey", "path", "content")
             }),

        Tool("list_files", "List the lambda's data", Effect.Read,
             "The lambda's data: whether its workspace is switched on, and every file in it with size and last write - the same whichever version is online. The code and assets of a version are in read_lambda.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("delete_file", "Delete a file from the lambda's data", Effect.Replace,
             "Remove a file, or a folder with its contents, from the workspace - the lambda's data, shared by every version. No deploy or rollback brings it back.",
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

        Tool("showcase", "Showcase a lambda", Effect.Replace,
             $"List a lambda on the public showcase page, change its entry, or take it off. Not part of building: only do this when the user asks for it. With only privateKey it returns the current entry. An entry needs a title, a description and a picture (a screenshot or short GIF of the lambda in use); it is listed while the lambda is online. {ShowcaseLimits.Tone}",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["title"] = Field("string", $"What it is, in a few words - 'Pub quiz scoreboard', not 'The ultimate quiz experience'. Up to {ShowcaseLimits.MaxTitle} characters."),
                     ["description"] = Field("string", $"One to three plain sentences on what a visitor can do with it. Up to {ShowcaseLimits.MaxDescription} characters."),
                     ["image"] = Field("string", $"The picture, base64: PNG, JPEG, GIF or WebP, up to {options.MaxShowcaseImageBytes / 1024 / 1024} MB. Needed for a new entry; left out, the current one is kept."),
                     ["remove"] = Field("boolean", "Take the lambda off the showcase instead.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("list_demos", "List the demos", Effect.Read,
             "Demos this platform keeps online, each a finished lambda showing one way to build something: a REST API over records, registration and login, a websocket game, uploads, live updates. Their keys are public and read only: read the closest one with read_lambda (and list_files, read_logs) before writing similar code. create_lambda with a demo's id as template starts from a copy.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }),

        Tool("platform_guide", "Read the platform guide", Effect.Read,
             "How this platform works - read it first: what a version is and what data is and how long each lives, what the snippet returns, what is imported, what is refused, limits and terms.",
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
                "copy_version" => await CopyAsync(arguments, origin),
                "check_code" => await CheckAsync(arguments),
                "deploy" => await DeployAsync(arguments, origin),
                "read_lambda" => await ReadAsync(arguments, origin),
                "read_logs" => await LogsAsync(arguments),
                "upload_file" => await UploadAsync(arguments),
                "list_files" => await FilesAsync(arguments),
                "delete_file" => await RemoveAsync(arguments),
                "showcase" => await ShowcaseAsync(arguments, origin),
                "list_demos" => Demos(origin),
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
            version = lambda.LatestVersion,
            next = $"write_code with deploy: true saves your code as version {lambda.LatestVersion + 1} and puts it online. Then keep working in that version: pass its number as version to change_code or write_code for every further change.",
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

        return await SaveAsync(arguments, files, Number(arguments, "version"), origin);
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

        var into = Number(arguments, "version");

        // changed where it is, the version named is what the change applies
        // to; saved as a new one, the change applies to the newest
        var current = into is { } target
            ? LambdaSource.Parse((await meta.GetVersionAsync(privateKey, target)).Code)
            : await Api.VersionResource.LatestAsync(meta, privateKey);

        var files = LambdaChanges.Apply(current, changed, remove, edits);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return McpProtocol.Refuse(invalid);
        }

        return await SaveAsync(arguments, files, into, origin);
    }

    /// <summary>
    /// Stores the files - over the version named, or as a new one - and
    /// deploys them if the arguments ask for that.
    /// </summary>
    /// <param name="into">The version to save over, or nothing for a new one</param>
    private async ValueTask<JsonObject> SaveAsync(JsonObject arguments, IReadOnlyList<LambdaFile> files, int? into, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var note = new VersionNote(Text(arguments, "specification"), Text(arguments, "change"), VersionOrigins.Agent);

        var code = LambdaSource.Serialize(files);

        var version = into is { } target
            ? await meta.UpdateAsync(privateKey, target, code, note)
            : await meta.SaveAsync(privateKey, code, note);

        // said only when it is missing, and as a request rather than a
        // refusal: the code matters more than the note about it
        var reminder = version.Change == null
            ? "Pass change (one line on what the version does) and specification (what the user wants, and why); the owner reads them in the version history."
            : null;

        // what to do next, which is where the habit of a version per fix is
        // either kept or broken
        var keepWorking = into == null
            ? $"Version {version.Version} is new, and now the newest. Keep working in it: pass version: {version.Version} to change_code or write_code to save every further fix over it, with deploy: true to try each one. Start another version only for the next thing the user asks for."
            : $"Saved over version {version.Version} (revision {version.Revision}). Keep saving over it while you work on this.";

        if (Flag(arguments, "deploy") != true)
        {
            if (Flag(arguments, "check") != true)
            {
                return McpProtocol.Say(new
                {
                    ok = true,
                    version = version.Version,
                    revision = version.Revision,
                    savedOver = into != null ? true : (bool?)null,
                    next = $"deploy, to put it online. {keepWorking}",
                    note = reminder
                });
            }

            /*
             * Compiled here rather than left to check_code, which wants every
             * file sent again: an agent that changed one line with change_code
             * would have to read the whole lambda back to find out whether the
             * line compiles. The version is saved either way.
             */
            var outcome = await meta.CheckAsync(privateKey, LambdaSource.Serialize(files));

            return McpProtocol.Say(new
            {
                ok = true,
                version = version.Version,
                revision = version.Revision,
                savedOver = into != null ? true : (bool?)null,
                compiles = outcome.Success,
                diagnostics = outcome.Diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message }),
                next = outcome.Success
                    ? $"deploy, when it should go online. {keepWorking}"
                    : $"fix the diagnostics with change_code and version: {version.Version}, which saves over the same version.",
                note = reminder
            });
        }

        return await DeployAsync(privateKey, version.Version, origin, keepWorking, reminder);
    }

    private async ValueTask<JsonObject> CopyAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var note = new VersionNote(Text(arguments, "specification"), Text(arguments, "change"), VersionOrigins.Agent);

        var copy = await meta.CopyAsync(privateKey, Number(arguments, "version"), note);

        var keepWorking = $"Version {copy.Version} is the newest now. Change it in place - change_code or write_code with version: {copy.Version} - and deploy it as often as you like; the version it copies stays as it was.";

        if (Flag(arguments, "deploy") == true)
        {
            return await DeployAsync(privateKey, copy.Version, origin, keepWorking);
        }

        return McpProtocol.Say(new
        {
            ok = true,
            version = copy.Version,
            copied = Number(arguments, "version"),
            copy.Change,
            next = keepWorking
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

    /// <summary>
    /// Puts a file into the workspace of a lambda.
    /// </summary>
    /// <remarks>
    /// The editor has a panel for this and an agent had nothing, which made
    /// putting a model or a dataset beside the code something it could be told
    /// about and not do.
    /// </remarks>
    private async ValueTask<JsonObject> UploadAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var path = Required(arguments, "path");

        var content = Required(arguments, "content");

        var id = await meta.RequireEditableAsync(privateKey);

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

        var written = await workspace.WriteAsync(id, path, stream);

        return McpProtocol.Say(new
        {
            ok = true,
            written.Path,
            written.Size,
            note = "In the lambda's data: there at once, no deploy needed, and shared by every version."
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
            enabled = listing.Enabled,
            files = listing.Files.Select(f => new { f.Path, f.Size, f.Modified }),
            listing.Folders,
            listing.UsedBytes,
            listing.QuotaBytes,
            note = listing.Enabled
                ? null
                : "The owner switched the workspace off: it holds nothing, and code that uses Workspace fails. Only the owner switches it on again - ask the user."
        });
    }

    private async ValueTask<JsonObject> RemoveAsync(JsonObject arguments)
    {
        var id = await meta.RequireEditableAsync(Required(arguments, "privateKey"));

        var path = Required(arguments, "path");

        await workspace.DeleteAsync(id, path);

        return McpProtocol.Say(new { ok = true, path });
    }

    private ValueTask<JsonObject> DeployAsync(JsonObject arguments, string origin)
        => DeployAsync(Required(arguments, "privateKey"), Number(arguments, "version"), origin);

    /// <param name="next">What to do after this, as the call that saved the version sees it</param>
    /// <param name="reminder">What the call that saved the version should have passed and did not</param>
    private async ValueTask<JsonObject> DeployAsync(string privateKey, int? version, string origin, string? next = null, string? reminder = null)
    {
        var result = await meta.DeployAsync(privateKey, version, VersionOrigins.Agent);

        if (!result.Success)
        {
            /*
             * Which version was refused, and what is online instead. A save
             * with deploy: true that does not compile has still made a
             * version, and an answer without its number left an agent - and
             * whatever reports on it - unable to say which one it was, or
             * whether anything had gone offline. Nothing has: a refused
             * deployment leaves the one before it running.
             */
            var current = await meta.GetAsync(privateKey);

            return McpProtocol.Say(new
            {
                ok = false,
                problem = "Not deployed. The diagnostics say whether compiling or building the handler failed. Whatever was online before still is.",
                version = version ?? current?.LatestVersion,
                stillOnline = current?.ActiveVersion,
                diagnostics = result.Diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message }),
                next = (version ?? current?.LatestVersion) is { } saved
                    ? $"Fix it with change_code and version: {saved}, which saves over the same version, and deploy: true."
                    : null
            }, failed: true);
        }

        var lambda = result.Lambda!;

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            domainUrl = DomainUrl(lambda),
            version = lambda.ActiveVersion,
            revision = lambda.ActiveRevision,
            onlineUntil = lambda.DeployedUntil,
            next = next ?? "Call the public address, then read_logs to see how it answered.",
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

        var stores = await data.ListAsync(privateKey);

        var only = Text(arguments, "file");

        IEnumerable<object> listing;

        string? note = null;

        if (only != null)
        {
            var one = files.FirstOrDefault(f => f.Name == only)
                   ?? throw LambdaException.NotFound($"Version {version} has no file '{only}'. It has: {string.Join(", ", files.Select(f => f.Name))}.");

            if (one.Code.Length <= ReadFileLimit)
            {
                listing = [new { one.Name, one.Code }];
            }
            else
            {
                listing = [new { one.Name, length = one.Code.Length }];

                note = $"{one.Name} comes to {one.Code.Length:N0} characters, more than is sent here. {Archive(version)} has every file of this version.";
            }
        }
        else if (files.Sum(f => (long)f.Code.Length) <= ReadBudget)
        {
            listing = files.Select(f => new { f.Name, f.Code });
        }
        else
        {
            listing = files.Select(f => new { f.Name, length = f.Code.Length });

            note = $"The files come to {files.Sum(f => (long)f.Code.Length):N0} characters, more than one answer carries. Pass file to read one of them, up to {ReadFileLimit:N0} characters; {Archive(version)} has every file.";
        }

        return McpProtocol.Say(new
        {
            ok = true,
            lambda.PublicKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            domainUrl = DomainUrl(lambda),
            lambda.Tier,
            limits = Limits(Enum.Parse<LambdaTier>(lambda.Tier)),
            lambda.ActiveVersion,
            lambda.ActiveRevision,
            // the version online was saved over after it went online, so what
            // visitors get is not what read_lambda shows until it is deployed
            onlineChanged = lambda.ActiveChanged ? true : (bool?)null,
            lambda.LatestVersion,
            workOn = lambda.LatestVersion is { } newest
                ? $"Version {newest} is the newest: save further changes over it with version: {newest}. Versions before it are history."
                : null,
            online = lambda.ActiveVersion != null,
            lambda.DeployedUntil,
            lambda.KeptUntil,
            version,
            revision = content?.Revision,
            specification = content?.Specification,
            change = content?.Change,
            // the why of the recent past, so a change made on top of somebody
            // else's work can follow what they were trying to do - the one line
            // each, since a specification can be a page and this is read every time
            history = history.Take(10).Select(v => new { v.Version, v.Created, v.Change, v.Origin, revision = v.Revision > 1 ? v.Revision : (int?)null, v.Modified }),
            // what the lambda keeps, which no version holds and none brings back
            data = stores.ToDictionary(s => s.Kind, s => (object)new { s.Enabled, items = s.Items, s.UsedBytes, s.QuotaBytes }),
            files = listing,
            filesOmitted = note != null ? true : (bool?)null,
            note
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

    /// <summary>
    /// The largest file read_lambda sends when it is asked for one by name.
    /// </summary>
    /// <remarks>
    /// A premium lambda may ship a hundred megabytes. An answer is built
    /// whole before any of it is sent, and past a megabyte it is more than an
    /// agent reads in one piece anyway - the zip of the version has every file,
    /// and streams it.
    /// </remarks>
    private const int ReadFileLimit = 1024 * 1024;

    private static string Archive(int? version) => $"GET /api/v1/lambdas/{{privateKey}}/versions/{version}/zip";

    /// <summary>
    /// What a lambda in the given tier may use, as read_lambda reports it.
    /// </summary>
    private object Limits(LambdaTier tier)
    {
        var workspace = options.WorkspaceOf(tier);

        return new
        {
            codeCharacters = options.MaxCodeLengthOf(tier),
            assetBytes = options.MaxAssetBytesOf(tier),
            workspaceBytes = workspace.Quota
        };
    }

    /// <summary>
    /// Reads, writes or removes the showcase entry of a lambda.
    /// </summary>
    /// <remarks>
    /// One tool rather than three: it is used rarely, and each tool listed is
    /// read by every agent on every conversation whether it showcases
    /// anything or not.
    /// </remarks>
    private async ValueTask<JsonObject> ShowcaseAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        if (Flag(arguments, "remove") == true)
        {
            await showcases.RemoveAsync(privateKey);

            return McpProtocol.Say(new { ok = true, showcased = false });
        }

        var title = Text(arguments, "title");
        var description = Text(arguments, "description");
        var encoded = Text(arguments, "image");

        ShowcaseInfo? entry;

        if (title == null && description == null && encoded == null)
        {
            entry = await showcases.GetAsync(privateKey);
        }
        else
        {
            byte[]? image = null;

            if (!string.IsNullOrEmpty(encoded))
            {
                try
                {
                    image = Convert.FromBase64String(encoded);
                }
                catch (FormatException)
                {
                    return McpProtocol.Refuse("The image is not valid base64.");
                }
            }

            // a change of one field keeps the others, as the tool promises
            var current = await showcases.GetAsync(privateKey);

            entry = await showcases.SaveAsync(privateKey, new ShowcaseDraft(title ?? current?.Title, description ?? current?.Description, image));
        }

        if (entry == null)
        {
            return McpProtocol.Say(new
            {
                ok = true,
                showcased = false,
                note = "Not in the showcase. Pass title, description and image to add it - only if the user asked for that."
            });
        }

        return McpProtocol.Say(new
        {
            ok = true,
            showcased = true,
            entry.Title,
            entry.Description,
            entry.Online,
            page = $"{origin}/showcase",
            note = entry.Online ? null : "Listed once the lambda is online again - deploy it."
        });
    }

    private static JsonObject Demos(string origin) => McpProtocol.Say(new
    {
        ok = true,
        demos = DemoCatalog.All.Select(d => new
        {
            d.Id,
            d.Name,
            d.Description,
            shows = d.Shows,
            readWhen = d.ReadWhen,
            privateKey = d.Key,
            url = $"{origin}/lambda/{d.Key}/",
            files = DemoCatalog.FilesFor(d).Select(f => f.Name)
        }),
        howToRead = "read_lambda with the demo's privateKey returns every file and the version history; list_files shows what it keeps as data; read_logs shows how it answers real traffic. Open the url to use it.",
        readOnly = "Anything that would change a demo is refused. To build on one, create_lambda with its id as template - that gives a lambda of your own with the same files."
    });

    /// <summary>
    /// Where the lambda answers besides its path, for an agent to call and to
    /// tell the user about. Always HTTPS, which plain requests are redirected
    /// to - the operator installs a certificate for the domain.
    /// somebody else's domain.
    /// </summary>
    private static string? DomainUrl(LambdaInfo lambda)
        => LambdaDescription.Serves(lambda.Tier, lambda.Domain) ? $"https://{lambda.Domain}/" : null;

    private JsonObject Guide() => McpProtocol.Say(new
    {
        ok = true,
        preferTheApi = "If you can make HTTP requests, the REST API at https://genhttp.dev/api/v1/openapi.json does the same as these tools and costs fewer tokens, because files are sent directly. GET /api/v1/lambdas/{privateKey}/versions/{version}/zip downloads a version; PUT /api/v1/lambdas/{privateKey}/versions/{version}/zip saves a zip of all files over the newest version, POST /api/v1/lambdas/{privateKey}/versions/zip as a new one - so edit locally and push as often as it takes. Every endpoint that saves a version takes ?deploy=true. Many environments cannot reach it; then use these tools.",
        whatALambdaIs = "C# that returns a GenHTTP handler, served at /lambda/{publicKey}/. No Main and no project: the snippet is the program.",
        lifecycle = new
        {
            twoKinds = "A lambda holds two kinds of things, and they live differently. Versions are the program. Data is what the program keeps. Getting this right is most of getting a lambda right.",
            versions = new
            {
                what = "A version is the program: every .cs file and every asset - index.html, scripts, styles, icons, the whole front end. They are saved, deployed and rolled back together, and nothing else is in a version.",
                workInTheNewest = "The newest version is the one being worked on. Change it in place - change_code or write_code with version set to its number - and deploy it as often as you like while you fix, try and adjust. Saving over it and deploying again is normal and harms nothing.",
                newVersions = "Start a new version once per thing the user asks for, not once per edit: write_code or change_code without version saves one, and copy_version starts one from any version without sending files. Three versions for one request is a history nobody can read.",
                history = "Every version before the newest is history: kept exactly as it was, to read, compare and roll back to. It cannot be changed; to carry on from an old version, copy it - the copy becomes the newest.",
                online = "Deploying builds a version and puts it online. Visitors get exactly what was deployed until the next deploy - saving over the version that is online changes nothing they see until you deploy it again. read_lambda says onlineChanged when that is the case.",
                kept = $"The newest {options.MaxVersions} versions are kept; older ones are removed, never the one online. Fewer, meaningful versions keep more of the history that matters."
            },
            data = new
            {
                what = "Data belongs to the lambda, not to a version: every version reads and writes the same data. It is where everything the program keeps goes - records, accounts, scores, uploads, anything users create or change.",
                lifetime = "Data outlives every save, deploy, rollback and copy - no version holds it, so none of them changes it or brings an earlier state back. It goes only when the lambda is deleted, or when its owner switches that kind of data off, which deletes what it held.",
                kinds = "The workspace - a private directory of files - is the kind there is now. More kinds, such as a database and secrets, will be added the same way; read_lambda lists what a lambda has under data.",
                optIn = "The owner decides which kinds of data a lambda has. The workspace is on unless the owner switched it off: list_files and read_lambda say whether it is. You cannot switch data on - if it is off, ask the user.",
                beReady = "Data can be empty: a new lambda has none, and an owner can clear it. Have the code create what it needs on first use, and say so plainly where something it expects is missing, rather than fail."
            },
            whereThingsGo = new
            {
                theProgram = "In the version, as assets: the app itself - C#, and the whole front end including a single page application's HTML, JavaScript, CSS, images and fonts. Ship it with write_code under a folder such as web/ and serve it with Assets.App(\"web\").",
                theData = "In the workspace: everything the lambda writes while it runs, everything users create or upload, and large input files that are not program - a model, a dataset, media. Write it with Workspace from the code, or put a file there with upload_file.",
                neverTheOtherWay = "Never keep user data in assets: they are read only while the lambda runs and replaced on every deploy. Never upload the front end to the workspace: it would not be versioned, a rollback would not bring the matching pages back, and a copy of a version would lack them."
            },
            theLambda = new
            {
                lifetime = $"A free lambda goes offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits, and is removed - versions, data and all - about {(int)options.Retention.TotalDays} days after the last of either. A premium lambda stays online and is kept.",
                deleting = "Deleting a lambda deletes its versions and its data together. Nothing else deletes versions one by one."
            }
        },
        demos = new
        {
            what = "Finished lambdas this platform keeps online, each showing one way to build something. list_demos says what each one shows and when to read it.",
            which = DemoCatalog.All.Select(d => $"{d.Id}: {d.Name}"),
            how = "Their editor keys are public and read only. read_lambda with a demo's key gives its files and history; read the closest one before writing similar code, and follow its patterns. create_lambda with its id as template starts from a copy."
        },
        paths = new
        {
            rule = "Use relative paths for every link, script, stylesheet, image, fetch, form action, websocket and redirect: \"api/items\", \"app.css\", \"./\". No leading slash, and never /lambda/{publicKey}/ or the full address.",
            why = "The same lambda answers at /lambda/{publicKey}/ on this platform and, in the premium tier, at the root of a domain of its own. A path starting with / leaves the lambda on the platform; a hard-coded /lambda/{publicKey}/ does not exist on the domain.",
            pages = "A page at the root of the lambda resolves \"api/items\" against the lambda. A page one level deeper needs \"../api/items\" - or keep the pages at the root.",
            websockets = "Build the address from the page: new URL(\"play\", location.href) with the scheme swapped to ws: or wss:.",
            inCSharp = "Redirect.To(\"other\") and Location headers take relative paths too. Never build an absolute URL from the request's host and /lambda/."
        },
        theSnippet = new
        {
            file = LambdaSource.EntryName,
            mustReturn = "An IHandler or a builder of one: Inline.Create(), Layout.Create(), Content.From(...), Websocket.Functional(), ...",
            example = "return Inline.Create().Get(() => new Greeting(\"hello\"));\n\nrecord Greeting(string Text);",
            anonymousTypes = "Routes cannot return anonymous types; declare a record."
        },
        moreThanOneFile = new
        {
            howItWorks = "Other .cs files hold types, compiled into the same namespace as the snippet. Workspace works in all of them. Assets means the lambda's own files only in lambda.cs - elsewhere it is LambdaEnvironment.Assets.",
            howMany = "Any number. Only what the C# comes to together is limited - see limits."
        },
        assets = new
        {
            what = "Any file not ending in .cs: part of the version, served as is, never compiled, not counted against the code budget. The front end belongs here.",
            shipping = "Send with the code: { name: \"web/app.css\", code: \"body { margin: 0 }\" }. Binary files as base64 with encoding \"base64\".",
            reading = "Assets.Tree(), Assets.Files(), Assets.App() (single page application: index.html answers unmatched paths), Assets.Exists / ReadText / ReadBytes / List / Folders. Read only: a lambda cannot write its assets.",
            folders = "Each takes an optional folder: Assets.App(\"site\") serves site/ at the root, so site/app.css is requested as /app.css.",
            inOtherFiles = "Assets means this only in the top-level code of lambda.cs. In other files, and in types, Assets is the Files module's type of the same name - use LambdaEnvironment.Assets there.",
            serving = "return Layout.Create().Add(\"api\", api).Add(Assets.App(\"web\"));",
            contentTypes = "Inferred from the file extension.",
            size = "Every version keeps its own copy of its assets and is read whole to be saved and deployed, so a large file that is data rather than program - a model, a dataset, video, a library of pictures - belongs in the workspace, where it is kept once.",
            limits = new
            {
                bytes = options.MaxAssetBytesOf(LambdaTier.Free),
                premiumBytes = options.MaxAssetBytesOf(LambdaTier.Premium),
                count = "Any number: only what they come to is counted.",
                names = "Letters, digits, dashes, underscores, dots and slashes. No leading slash, no .."
            }
        },
        workspace = new
        {
            what = "The lambda's data, for now: a private directory it reads and writes at runtime, for anything that must outlive a request or a deployment. The same for every version.",
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
            fromOutside = "upload_file puts a file there, list_files lists it, delete_file removes one. Over HTTP, for more than a tool call carries: PUT /api/v1/lambdas/{privateKey}/files/{path}/content with the file itself as the body (curl -T model.onnx ...), the slashes of the path encoded as %2F - streamed to the disk, however large. Or let the lambda fetch it once with HttpClient and keep it with Workspace.WriteBytes.",
            serving = "Workspace.Files(\"uploads\") serves a folder of what users uploaded - data, served as data. The app's own pages are assets.",
            off = "If the owner switched the workspace off, every Workspace member throws an exception saying so. list_files says whether it is on.",
            limits = new
            {
                bytes = options.WorkspaceOf(LambdaTier.Free).Quota,
                premiumBytes = options.WorkspaceOf(LambdaTier.Premium).Quota,
                counted = $"Only the room all files take together: any number of files, each as large as the room allows. Every file takes whole blocks of {WorkspaceLimits.Block} bytes, at least one, and so does every folder.",
                exact = "list_files answers with the quota of the lambda at hand."
            },
            note = "Nothing else on the file system is reachable. There is no Append."
        },
        servingAFrontEnd = new
        {
            rule = "The front end is part of the program, so it is part of the version: ship index.html, scripts, styles and images as assets and serve them with Assets.App(\"web\"). It is then deployed, rolled back and copied together with the API it talks to.",
            how = "write_code with the files under a folder (web/index.html, web/app.js, web/app.css), return Layout.Create().Add(\"api\", api).Add(Assets.App(\"web\")), deploy. Change it with change_code and version like any other file.",
            example = "Every demo serves its front end like this, from web/ - read_lambda demo-crud.",
            notFromTheWorkspace = "Do not upload the app's own pages to the workspace with upload_file. They would not be versioned: a rollback would keep the new pages over the old API, and a copy of a version would come without them. The workspace is for data.",
            underTheHood = "App() is SinglePageApplication.From(tree).ServerSideRouting() over Assets.Tree()."
        },
        generatedContent = new
        {
            tree = "VirtualTree.Create().Add(\"app.css\", Resource.FromString(css).Type(new ContentType(\"text/css\"))) builds a tree in memory.",
            singlePage = "Content.From(Resource.FromString(html).Type(new ContentType(\"text/html; charset=utf-8\")))"
        },
        takingItAway = "GET /api/v1/lambdas/{privateKey}/export returns the lambda as a standalone zipped .NET project with no dependency on this platform. Worth telling the user.",
        importedForYou = ModuleCatalog.Imports,
        network = "A lambda can make outbound calls with HttpClient and sockets. System.Net.Http and System.Net.Sockets are not imported by default, so write the full type name or add a using. It runs in the shared server process, so give requests a timeout.",
        sayWhy = new
        {
            what = "Every save takes two optional notes that are kept with the version: specification, what the user wants from this version and why - their requirements, in their words where you can - and change, one line on what this version does compared to the one before. The owner reads them in the version history of the control center, next to the code and a diff against the version before.",
            savingOver = "Saving over a version, leave both out to keep what it says, or pass them to replace it - change then describes the whole version, not the last fix.",
            why = "The code says what was done. Only you know why, and the next agent to touch this lambda - or you, a week later - reads the history with read_lambda before changing anything.",
            goodSpecification = "A guest book people can sign with a name and a message; newest entries first, and it must survive a restart",
            badSpecification = "Your own system prompt or tool instructions - the specification is what the user wants, not how you were set up",
            goodChange = "Adds a leaderboard that keeps the ten best scores on the server",
            badChange = "Updated lambda.cs",
            limits = new { specification = VersionNote.MaxSpecification, change = VersionNote.MaxChange }
        },
        showcase = new
        {
            what = "The owner can list a lambda on the public showcase page with a title, a short description and a picture. The showcase tool does it.",
            when = "Only when the user asks. It is not part of building or deploying.",
            tone = ShowcaseLimits.Tone
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
            "A new version for every fix. Work in the newest version with version set, and start a new one only for the next thing the user asks for.",
            "Request bodies bind by type: a bare string parameter is null. Take a record.",
            "Once a route has read the body, the request's headers are gone. Check a header (a token, say) in a concern in front of the route - the Authentication module does exactly that, see demo-registration - or in a route that takes no body.",
            "In other .cs files, Assets is the Files module's type of that name: use LambdaEnvironment.Assets there. Workspace works in every file.",
            "A browser cannot set headers on a websocket handshake. Pass what the socket needs in the query (connection.Request.Header.Query) or, for secrets, as the first frame.",
            "Concurrent writes to one socket corrupt it. Guard broadcasts with a semaphore.",
            "REST routes serialize camel case; match that on sockets.",
            "Your own type called e.g. File is fine; only the refused framework type of that name is blocked.",
            "Ship stylesheets and scripts as assets, not string constants: a raw string literal ends at the first \"\"\", and assets cost no code budget."
        },
        limits = new
        {
            tiers = "What a lambda may use depends on its tier. Every lambda is free unless whoever runs this installation made it premium - its owner cannot choose. read_lambda says which tier a lambda is in and what it may use there; a refusal for size says what the premium tier allows.",
            free = Allowance(LambdaTier.Free),
            premium = Allowance(LambdaTier.Premium),
            code = $"{options.MaxCodeLengthOf(LambdaTier.Free):N0} characters across all .cs files; {options.MaxCodeLengthOf(LambdaTier.Premium):N0} for a premium lambda",
            versions = $"The newest {options.MaxVersions} versions are kept, and the one online.",
            deployment = $"A free lambda is online while used, and offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits. A premium one stays online.",
            retention = $"A free lambda is removed about {(int)options.Retention.TotalDays} days after the last of either. A premium one is kept."
        },
        terms = SystemResource.Terms
    });

    /// <summary>
    /// What a lambda in the given tier may use, as the guide says it.
    /// </summary>
    private string Allowance(LambdaTier tier)
    {
        var workspace = options.WorkspaceOf(tier);

        return $"Code: {options.MaxCodeLengthOf(tier):N0} characters, in any number of .cs files. "
             + $"Assets: {Size(options.MaxAssetBytesOf(tier))} in all, any number of them. "
             + $"Workspace: {Size(workspace.Quota)} in all, in any number of files.";
    }

    private static string Size(long bytes) => bytes switch
    {
        >= 1L << 30 when bytes % (1L << 30) == 0 => $"{bytes >> 30} GB",
        >= 1L << 20 when bytes % (1L << 20) == 0 => $"{bytes >> 20} MB",
        _ => $"{bytes >> 10:N0} KB"
    };

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

        /// <summary>Adds a version or saves over the newest, and with deploy: true replaces what is online.</summary>
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
