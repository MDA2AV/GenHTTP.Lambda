using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Features;
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
/// that it compiles, put it online - and to change one that exists, work in a
/// feature beside it and merge that once it is right. Listing the demos is in
/// here too, because the fastest way to learn what this platform will accept
/// is to read something it is already running - and a demo is read with the
/// same tools an agent then uses on its own lambda, since its editor key is
/// public.
///
/// Two things are said wherever an agent decides something, because agents
/// kept getting them wrong. A version is the program and data is what it
/// keeps, and those live differently. And versions never change: a change to
/// a lambda that exists is made in a feature, tried at the feature's own
/// address against a copy of the data, and merged into the next version once,
/// rather than saved and deployed as a version per attempt. The tool
/// descriptions say it where a tool is picked, the answers say it where the
/// next call is decided, and the guide says it first.
///
/// The tools that save, deploy, read and reach into the data take an optional
/// feature, and then act on the feature rather than on the lambda - so the
/// same few tools do both, and nothing is learned twice.
///
/// Every tool answers with an object rather than prose. A model reads the text
/// and a program reads the structured copy, and both are the same thing.
/// </remarks>
public sealed class McpTools(IMetaService meta, IWorkspaceService workspace, IDataService data, IFeatureService features, IShowcaseService showcases,
                              LambdaTelemetry telemetry, LogBook book, LambdaOptions options)
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
             "Save every file, replacing the previous set: as a new version of the lambda, or - with feature - into that feature. .cs files are compiled - lambda.cs returns the handler, others hold types; any other file is an asset, served as is and reachable as Assets: the whole front end (pages, scripts, styles, icons) goes here, as part of the program. What the lambda keeps at runtime (records, accounts, uploads) is data and lives in the workspace, never in files here; so does a large input file such as a model or a dataset (upload_file). Say why with specification and change. deploy: true publishes in the same call - a version at the public address, a feature at its preview address. To send only what changes, use change_code. To change a lambda that is already in use, work in a feature.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key from create_lambda."),
                     ["feature"] = Field("string", "Save into this feature (from create_feature) instead of saving a new version. The lambda and what it has online are not touched."),
                     ["specification"] = Field("string", $"What the user wants from this version and why: their requirements, in their own words where you can, condensed if they said a lot. Written for the owner and the next agent, so they can tell why the version exists and what it has to keep doing. Not your own instructions or system prompt - only what the user asked for. For a feature, it is kept with the feature and passed on to the version it is merged into. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["change"] = Field("string", $"What this version changes, in one line written for the owner - 'Adds a leaderboard that keeps the ten best scores', not 'updated lambda.cs'. For a feature, it describes the whole feature, not the last fix. Optional, up to {VersionNote.MaxChange} characters."),
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
                     ["deploy"] = Field("boolean", "Also deploy what was saved: a version at the public address, a feature at its preview address."),
                     ["check"] = Field("boolean", "Without deploy: compile what was saved and answer with its diagnostics, putting nothing online.")
                 },
                 ["required"] = new JsonArray("privateKey", "files")
             }),

        Tool("change_code", "Change some files", Effect.Save,
             "Change some files: add or replace files, remove files, or replace text within a file. Everything not named stays as it is, so there is no need to resend unchanged files. With feature, the change is saved into that feature - the way to work on a lambda that exists: change it as often as you like, deploy: true to try it at the feature's preview address, merge_feature when it is right. Without feature, the newest version is changed and saved as a new version. check: true compiles without publishing. Code that does not compile is saved but never goes online.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "Change this feature (from create_feature) instead of saving a new version. The lambda and what it has online are not touched."),
                     ["specification"] = Field("string", $"What the user wants from this change and why: their requirements, in their own words where you can. Not your own instructions or system prompt. Kept with the version or the feature. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["change"] = Field("string", $"What this changes, in one line written for the owner. For a feature, it describes the whole feature, not the last fix. Optional, up to {VersionNote.MaxChange} characters."),
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
                     ["deploy"] = Field("boolean", "Also deploy what was saved: a version at the public address, a feature at its preview address."),
                     ["check"] = Field("boolean", "Without deploy: compile what was saved and answer with its diagnostics, putting nothing online.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("create_feature", "Start a feature", Effect.Create,
             "Start a feature: a place to change a lambda without touching what it has online. It branches off a version - the newest unless base names another - with a copy of its files and a copy of the lambda's data, and gets an address of its own to try it at. Change it with change_code or write_code and feature (deploy: true puts it online at its preview address), test it there, and merge_feature once it does what was asked. The lambda goes on serving its visitors from the version online the whole time.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["name"] = Field("string", $"What the feature is, in a few words for the owner - 'Leaderboard', 'Dark mode'. Up to {FeatureService.MaxName} characters."),
                     ["specification"] = Field("string", $"What the user wants from it and why, in their words where you can. Kept with the feature and passed on to the version it is merged into. Optional, up to {VersionNote.MaxSpecification} characters."),
                     ["base"] = Field("integer", "The version to branch off. Defaults to the newest - leave it out unless the user asked to start from an older one.")
                 },
                 ["required"] = new JsonArray("privateKey", "name")
             }),

        Tool("update_feature", "Change a feature's name, notes or base", Effect.Replace,
             "Rename a feature, change what it says about itself, or move its base. merge_feature refuses a feature that is not based on the newest version, since merging it would undo what was saved after it branched off: bring the newer versions' changes into the feature first, then move base to the newest version here. Nothing checks that the changes really are in - that is up to you.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "The feature, from create_feature or read_lambda."),
                     ["name"] = Field("string", "A new name."),
                     ["specification"] = Field("string", "What the user wants from it and why."),
                     ["change"] = Field("string", "What it changes, in one line - what the version it is merged into will say."),
                     ["base"] = Field("integer", "The version the feature is now based on, once that version's changes are in its files.")
                 },
                 ["required"] = new JsonArray("privateKey", "feature")
             }),

        Tool("merge_feature", "Merge a feature into the lambda", Effect.Save,
             "Make a feature's files the next version of the lambda, and delete the feature with its preview and its copy of the data - the lambda's own data is not touched. Refused while the feature is not based on the newest version (update_feature says how to get it there), and while its code does not compile. deploy: true puts the new version online at once; otherwise deploy it when the user wants it live.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "The feature."),
                     ["specification"] = Field("string", "What the new version keeps as what the user wanted. Left out, the feature's."),
                     ["change"] = Field("string", "What the new version says it changes. Left out, the feature's."),
                     ["deploy"] = Field("boolean", "Also put the new version online.")
                 },
                 ["required"] = new JsonArray("privateKey", "feature")
             }),

        Tool("delete_feature", "Delete a feature", Effect.Replace,
             "Throw a feature away: its files, its preview and its copy of the data. The lambda is not touched. For a feature the user does not want after all.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "The feature.")
                 },
                 ["required"] = new JsonArray("privateKey", "feature")
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

        Tool("deploy", "Deploy a version or a feature", Effect.Replace,
             "Deploy (publish) a saved version so it goes live at its public address - the newest by default. With feature, deploy that feature's files to its own preview address instead, against its copy of the data, leaving the lambda alone. Returns diagnostics on failure, and whatever was online stays online.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["version"] = Field("integer", "Defaults to the newest."),
                     ["feature"] = Field("string", "Deploy this feature to its preview address instead of a version to the lambda's.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("read_lambda", "Read a lambda", Effect.Read,
             $"A lambda's status (online version, newest version, expiry, its tier and what it may use there), its open features, the recent versions with what each was asked for and changed, its data (the workspace: whether it is on and what it holds), and the files of one version - or, with feature, of that feature. Files come in full when they add up to at most {ReadBudget:N0} characters, otherwise by name and length, with file to read one. Read the history before changing what you did not write. Also how a demo is read: pass its key from list_demos.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["version"] = Field("integer", "Defaults to the newest."),
                     ["feature"] = Field("string", "Read this feature - its files, its base, and which newer versions it would have to take in before it can be merged - instead of a version."),
                     ["file"] = Field("string", $"Return only this file, in full however large the rest is - up to {ReadFileLimit:N0} characters, beyond which the zip has it.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("read_logs", "Read a lambda's logs", Effect.Read,
             "What a deployed lambda has been doing: its recent requests and how they were answered, what it printed, the errors it threw with their stack traces, and how much traffic it has had in the last hour and day. With feature, what that feature's preview has been doing instead, kept apart from the lambda's visitors. Call it after deploying to see that it works, and first when something is reported broken.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["feature"] = Field("string", "Read what this feature's preview did instead."),
                     ["level"] = Field("string", "The lowest level worth reading: 'info' for everything, 'warn' for problems, 'error' for failures. Left out, 'info'."),
                     ["since"] = Field("integer", "The cursor a previous call answered with, to read only what is new since then."),
                     ["limit"] = Field("integer", "At most this many lines, the newest. Left out, 100.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("upload_file", "Put a file into the lambda's data", Effect.Replace,
             "Write a file to the lambda's workspace - its data, which every version shares and no deploy, rollback or merge touches. With feature, to that feature's copy of the data instead, for trying things out without touching the real one. For content the lambda works with at runtime (initial records, pictures people will browse) and large input files that are not program: a model, a dataset, media. Takes effect at once, without a deploy. Not for the front end: pages, scripts and styles are the program and belong in the version as assets (write_code).",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "Write to this feature's copy of the data instead."),
                     ["path"] = Field("string", "Relative to the workspace; slashes make folders, e.g. 'models/model.onnx'."),
                     ["content"] = Field("string", "Text, or base64 with encoding set."),
                     ["encoding"] = Field("string", "'base64' for binary; omit otherwise.")
                 },
                 ["required"] = new JsonArray("privateKey", "path", "content")
             }),

        Tool("list_files", "List the lambda's data", Effect.Read,
             "The lambda's data: whether its workspace is switched on, and every file in it with size and last write - the same whichever version is online. With feature, that feature's copy of it. The code and assets of a version are in read_lambda.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["feature"] = Field("string", "List this feature's copy of the data instead.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("delete_file", "Delete a file from the lambda's data", Effect.Replace,
             "Remove a file, or a folder with its contents, from the workspace - the lambda's data, shared by every version. No deploy or rollback brings it back. With feature, from that feature's copy instead.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "Delete from this feature's copy of the data instead."),
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
             "How this platform works - read it first: what a version, a feature and data are and how long each lives, what the snippet returns, what is imported, what is refused, limits and terms.",
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
                "create_feature" => await CreateFeatureAsync(arguments, origin),
                "update_feature" => await UpdateFeatureAsync(arguments, origin),
                "merge_feature" => await MergeFeatureAsync(arguments, origin),
                "delete_feature" => await DeleteFeatureAsync(arguments),
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
            next = "write_code with deploy: true, and fix what needs fixing with change_code and deploy: true - a new lambda needs no feature. Once people use it, make further changes in a feature (create_feature).",
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

        // a feature is changed from what it holds - and only if that is still
        // what it holds when the change is saved; a new version from the newest
        IReadOnlyList<LambdaFile> current;

        int? read = null;

        if (Text(arguments, "feature") is { } feature)
        {
            var content = await features.GetAsync(privateKey, feature);

            current = LambdaSource.Parse(content.Code);

            read = content.Feature.Revision;
        }
        else
        {
            current = await Api.VersionResource.LatestAsync(meta, privateKey);
        }

        var files = LambdaChanges.Apply(current, changed, remove, edits);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return McpProtocol.Refuse(invalid);
        }

        return await SaveAsync(arguments, files, origin, read);
    }

    /// <summary>
    /// Stores the files - as a new version, or into the feature named - and
    /// deploys or checks them if the arguments ask for that.
    /// </summary>
    private async ValueTask<JsonObject> SaveAsync(JsonObject arguments, IReadOnlyList<LambdaFile> files, string origin, int? read = null)
    {
        var privateKey = Required(arguments, "privateKey");

        var note = new VersionNote(Text(arguments, "specification"), Text(arguments, "change"), VersionOrigins.Agent);

        var code = LambdaSource.Serialize(files);

        if (Text(arguments, "feature") is { } feature)
        {
            return await SaveFeatureAsync(arguments, privateKey, feature, code, note, origin, read);
        }

        var version = await meta.SaveAsync(privateKey, code, note);

        // said only when it is missing, and as a request rather than a
        // refusal: the code matters more than the note about it
        var reminder = version.Change == null
            ? "Pass change (one line on what the version does) and specification (what the user wants, and why) next time; the owner reads them in the version history."
            : null;

        if (Flag(arguments, "deploy") != true)
        {
            if (Flag(arguments, "check") != true)
            {
                return McpProtocol.Say(new
                {
                    ok = true,
                    version = version.Version,
                    next = "deploy",
                    note = reminder
                });
            }

            /*
             * Compiled here rather than left to check_code, which wants every
             * file sent again: an agent that changed one line with change_code
             * would have to read the whole lambda back to find out whether the
             * line compiles. The version is saved either way.
             */
            var outcome = await meta.CheckAsync(privateKey, code);

            return McpProtocol.Say(new
            {
                ok = true,
                version = version.Version,
                compiles = outcome.Success,
                diagnostics = Diagnostics(outcome.Diagnostics),
                next = outcome.Success ? "deploy, when it should go online" : "fix the diagnostics with change_code",
                note = reminder
            });
        }

        return await DeployVersionAsync(privateKey, version.Version, origin, reminder);
    }

    /// <summary>
    /// Stores the files as the feature's, and puts its preview online or
    /// compiles it if the arguments ask for that.
    /// </summary>
    private async ValueTask<JsonObject> SaveFeatureAsync(JsonObject arguments, string privateKey, string feature, string code, VersionNote note, string origin,
                                                         int? read)
    {
        var saved = await features.SaveAsync(privateKey, feature, code, note, read);

        if (Flag(arguments, "deploy") == true)
        {
            return await DeployFeatureAsync(privateKey, feature, origin);
        }

        if (Flag(arguments, "check") == true)
        {
            var outcome = await meta.CheckAsync(privateKey, code);

            return McpProtocol.Say(new
            {
                ok = true,
                feature = Brief(saved, origin),
                compiles = outcome.Success,
                diagnostics = Diagnostics(outcome.Diagnostics),
                next = outcome.Success
                    ? $"deploy with feature: '{saved.Key}' to try it at its preview address"
                    : $"fix the diagnostics with change_code and feature: '{saved.Key}'"
            });
        }

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(saved, origin),
            next = $"Saved into the feature; nothing is online yet. Try it with deploy and feature: '{saved.Key}' (or deploy: true on the next save), then merge_feature when it does what was asked."
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
            diagnostics = Diagnostics(outcome.Diagnostics)
        });
    }

    #endregion

    #region Features

    private async ValueTask<JsonObject> CreateFeatureAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var created = await features.CreateAsync(privateKey, new FeatureDraft(Text(arguments, "name"), Text(arguments, "specification"),
                                                                              Number(arguments, "base"), VersionOrigins.Agent));

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(created, origin),
            next = $"Change it with change_code (or write_code) and feature: '{created.Key}' - it holds the files of version {created.Base} and a copy of the lambda's data. deploy: true on a save puts it online at previewUrl, never at the lambda's address; read_logs with feature shows how it answers. merge_feature when it does what was asked."
        });
    }

    private async ValueTask<JsonObject> UpdateFeatureAsync(JsonObject arguments, string origin)
    {
        var updated = await features.UpdateAsync(Required(arguments, "privateKey"), Required(arguments, "feature"),
                                                 new FeatureUpdate(Text(arguments, "name"), Text(arguments, "specification"), Text(arguments, "change"),
                                                                   Number(arguments, "base")));

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(updated, origin),
            next = updated.Mergeable
                ? $"It is based on the newest version, so merge_feature can merge it once it does what was asked."
                : $"It is based on version {updated.Base}, the newest is {updated.Newest}: bring the newer versions' changes in and move base to {updated.Newest} before merging."
        });
    }

    private async ValueTask<JsonObject> MergeFeatureAsync(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var note = new VersionNote(Text(arguments, "specification"), Text(arguments, "change"), VersionOrigins.Agent);

        var merged = await features.MergeAsync(privateKey, Required(arguments, "feature"), note, Flag(arguments, "deploy") == true);

        if (!merged.Merged)
        {
            return McpProtocol.Say(new
            {
                ok = false,
                problem = "Not merged: the feature's code does not compile. Nothing changed - fix it with change_code and feature, then merge again.",
                diagnostics = Diagnostics(merged.Diagnostics)
            }, failed: true);
        }

        var version = merged.Version!;

        if (merged.Deployment is not { } deployment)
        {
            return McpProtocol.Say(new
            {
                ok = true,
                merged = true,
                version = version.Version,
                next = $"The feature is now version {version.Version}, and gone - its preview and its copy of the data with it; the lambda's own data is as it was. deploy puts version {version.Version} online when the user wants it live."
            });
        }

        if (!deployment.Success)
        {
            var current = await meta.GetAsync(privateKey);

            return McpProtocol.Say(new
            {
                ok = false,
                merged = true,
                problem = $"Merged as version {version.Version}, but it did not go online: building its handler failed. Whatever was online before still is.",
                version = version.Version,
                stillOnline = current?.ActiveVersion,
                diagnostics = Diagnostics(deployment.Diagnostics)
            }, failed: true);
        }

        var lambda = deployment.Lambda!;

        return McpProtocol.Say(new
        {
            ok = true,
            merged = true,
            publicKey = lambda.PublicKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            domainUrl = DomainUrl(lambda),
            version = lambda.ActiveVersion,
            onlineUntil = lambda.DeployedUntil,
            next = "Merged and online. Call the public address, then read_logs to see how it answers."
        });
    }

    private async ValueTask<JsonObject> DeleteFeatureAsync(JsonObject arguments)
    {
        var feature = Required(arguments, "feature");

        await features.DeleteAsync(Required(arguments, "privateKey"), feature);

        return McpProtocol.Say(new { ok = true, deleted = feature.Trim().ToLowerInvariant() });
    }

    /// <summary>
    /// The versions saved after a feature's base, as a sentence begins with them.
    /// </summary>
    private static string Later(FeatureInfo feature)
        => feature.Newest - feature.Base == 1 ? $"version {feature.Newest} was" : $"versions {feature.Base + 1} to {feature.Newest} were";

    /// <summary>
    /// Said when a feature's code links to the lambda by its full path, which
    /// from the preview is the live lambda - with its real data.
    /// </summary>
    private static string? Leaks(string code, string publicKey)
        => code.Contains($"/lambda/{publicKey}/", StringComparison.Ordinal) || code.Contains($"/lambda/{publicKey}\"", StringComparison.Ordinal)
            ? $"The code links to /lambda/{publicKey}/ by its full path. From the preview that is the live lambda, with its real data - not the feature's copy. Use relative paths (\"api/items\", not \"/lambda/{publicKey}/api/items\")."
            : null;

    /// <summary>
    /// A feature as an agent reads it: where it answers, what it is based on,
    /// and whether it can be merged.
    /// </summary>
    private static object Brief(FeatureInfo feature, string origin) => new
    {
        feature = feature.Key,
        feature.Name,
        @base = feature.Base,
        newest = feature.Newest,
        mergeable = feature.Mergeable,
        previewUrl = $"{origin}{feature.Path}",
        previewOnline = feature.Online,
        // said only where it matters: a preview online that serves an earlier save
        previewOutdated = feature.Online && !feature.Current ? true : (bool?)null,
        feature.Change
    };

    #endregion

    #region Data

    /// <summary>
    /// Puts a file into the workspace of a lambda, or of one of its features.
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

        var (id, feature) = await WorkspaceOfAsync(privateKey, Text(arguments, "feature"), true);

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

        var written = await workspace.WriteAsync(id, path, stream, featureId: feature);

        return McpProtocol.Say(new
        {
            ok = true,
            written.Path,
            written.Size,
            note = feature == null
                ? "In the lambda's data: there at once, no deploy needed, and shared by every version."
                : "In the feature's copy of the data: its preview sees it at once; the lambda's own data is not touched."
        });
    }

    private async ValueTask<JsonObject> FilesAsync(JsonObject arguments)
    {
        var (id, feature) = await WorkspaceOfAsync(Required(arguments, "privateKey"), Text(arguments, "feature"), false);

        var listing = await workspace.ListAsync(id, feature);

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
        var (id, feature) = await WorkspaceOfAsync(Required(arguments, "privateKey"), Text(arguments, "feature"), true);

        var path = Required(arguments, "path");

        await workspace.DeleteAsync(id, path, feature);

        return McpProtocol.Say(new { ok = true, path });
    }

    /// <summary>
    /// The workspace a call is about: the lambda's own, or a feature's copy.
    /// </summary>
    private async ValueTask<(long LambdaId, long? FeatureId)> WorkspaceOfAsync(string privateKey, string? feature, bool editable)
    {
        if (feature != null)
        {
            var (lambdaId, featureId) = await features.RequireAsync(privateKey, feature, editable);

            return (lambdaId, featureId);
        }

        if (editable)
        {
            return (await meta.RequireEditableAsync(privateKey), null);
        }

        return (await meta.GetIdAsync(privateKey) ?? throw LambdaException.NotFound("There is no lambda with that editor key."), null);
    }

    #endregion

    #region Deploying

    private ValueTask<JsonObject> DeployAsync(JsonObject arguments, string origin)
        => Text(arguments, "feature") is { } feature
         ? DeployFeatureAsync(Required(arguments, "privateKey"), feature, origin)
         : DeployVersionAsync(Required(arguments, "privateKey"), Number(arguments, "version"), origin);

    private async ValueTask<JsonObject> DeployVersionAsync(string privateKey, int? version, string origin, string? reminder = null)
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
                diagnostics = Diagnostics(result.Diagnostics),
                next = "Fix the diagnostics with change_code and deploy: true. If people already use the lambda, fix it in a feature (create_feature) instead, so every attempt is tried at its own address rather than saved as a version."
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
            onlineUntil = lambda.DeployedUntil,
            next = "Call the public address, then read_logs to see how it answered. While you are still building it, fix with change_code and deploy: true; once people use it, make changes in a feature (create_feature), which is tried at an address of its own and merged into the next version once it is right.",
            note = reminder ?? "Deploying again extends onlineUntil."
        });
    }

    private async ValueTask<JsonObject> DeployFeatureAsync(string privateKey, string feature, string origin)
    {
        var result = await features.DeployAsync(privateKey, feature);

        if (!result.Success)
        {
            return McpProtocol.Say(new
            {
                ok = false,
                problem = "The preview was not deployed. The diagnostics say whether compiling or building the handler failed. Whatever the preview served before, it still does; the lambda was not touched.",
                feature = Brief(result.Feature, origin),
                diagnostics = Diagnostics(result.Diagnostics),
                next = $"Fix the diagnostics with change_code and feature: '{result.Feature.Key}', deploy: true."
            }, failed: true);
        }

        // links that leave the preview for the live lambda, said while there is still time
        var lambda = await meta.GetAsync(privateKey);

        var warning = lambda != null ? Leaks((await features.GetAsync(privateKey, feature)).Code, lambda.PublicKey) : null;

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(result.Feature, origin),
            previewUrl = $"{origin}{result.Feature.Path}",
            warning,
            next = result.Feature.Mergeable
                ? $"Call previewUrl, and read_logs with feature: '{result.Feature.Key}' to see how it answered. It works on its own copy of the data. When it does what was asked: merge_feature (deploy: true to put it live)."
                : $"Call previewUrl, and read_logs with feature to see how it answered. Before merging: {Later(result.Feature)} saved after this feature's base, version {result.Feature.Base} - bring those changes in and move the base to {result.Feature.Newest} with update_feature."
        });
    }

    #endregion

    #region Reading

    /// <summary>
    /// What a deployed lambda - or the preview of one of its features - has
    /// been saying, and how much the lambda is used.
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

        var feature = Text(arguments, "feature");

        var id = await meta.GetIdAsync(privateKey)
              ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var lines = FeatureLines.None;

        if (feature != null)
        {
            lines = FeatureLines.Of((await features.RequireAsync(privateKey, feature, false)).FeatureId);
        }

        var lambda = await meta.GetAsync(privateKey);

        var since = arguments.TryGetPropertyValue("since", out var cursor) && cursor is JsonValue value && value.TryGetValue<long>(out var from)
                  ? from
                  : 0;

        var (said, next, missed) = book.Read(since, null, Level(Text(arguments, "level")),
                                             Math.Clamp(Number(arguments, "limit") ?? 100, 1, 1000), lambdaId: id, feature: lines);

        var shown = said.Select(l => new { l.At, l.Level, l.Source, l.Text, detail = l.Detail, repeats = l.Repeats > 1 ? l.Repeats : (int?)null });

        var note = said.Count == 0
            ? feature == null
                ? "Nothing yet. Requests appear here once somebody calls the lambda - call its public address and read again."
                : "Nothing yet. Requests appear here once somebody calls the feature's preview - call its previewUrl and read again."
            : "Requests are the source 'Requests'; what the code printed is 'stdout' and 'stderr'; a handler that threw appears with its stack trace in detail. Pass cursor as since to read only what is new.";

        if (feature != null)
        {
            // a preview is not counted, so there is no traffic to report
            return McpProtocol.Say(new
            {
                ok = true,
                feature,
                lines = shown,
                cursor = next,
                missed,
                capturingOutput = options.CaptureLambdaOutput,
                note
            });
        }

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
            lines = shown,
            cursor = next,
            missed,
            capturingOutput = options.CaptureLambdaOutput,
            note
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

        var history = await meta.GetVersionsAsync(privateKey);

        var open = await features.ListAsync(privateKey);

        var wanted = Text(arguments, "feature");

        // read for a feature, the data it works on is its own copy
        var stores = await data.ListAsync(privateKey, wanted);

        int? version = null;

        LambdaVersionContent? content = null;

        FeatureContent? feature = null;

        IReadOnlyList<LambdaFile> files = [];

        if (wanted != null)
        {
            feature = await features.GetAsync(privateKey, wanted);

            files = LambdaSource.Parse(feature.Code);
        }
        else
        {
            version = Number(arguments, "version") ?? lambda.LatestVersion;

            if (version is { } which)
            {
                content = await meta.GetVersionAsync(privateKey, which);

                files = LambdaSource.Parse(content.Code);
            }
        }

        var archive = feature != null
            ? $"GET /api/v1/lambdas/{{privateKey}}/features/{feature.Feature.Key}/zip"
            : $"GET /api/v1/lambdas/{{privateKey}}/versions/{version}/zip";

        var only = Text(arguments, "file");

        IEnumerable<object> listing;

        string? note = null;

        if (only != null)
        {
            var one = files.FirstOrDefault(f => f.Name == only)
                   ?? throw LambdaException.NotFound($"There is no file '{only}' here. There are: {string.Join(", ", files.Select(f => f.Name))}.");

            if (one.Code.Length <= ReadFileLimit)
            {
                listing = [new { one.Name, one.Code }];
            }
            else
            {
                listing = [new { one.Name, length = one.Code.Length }];

                note = $"{one.Name} comes to {one.Code.Length:N0} characters, more than is sent here. {archive} has every file.";
            }
        }
        else if (files.Sum(f => (long)f.Code.Length) <= ReadBudget)
        {
            listing = files.Select(f => new { f.Name, f.Code });
        }
        else
        {
            listing = files.Select(f => new { f.Name, length = f.Code.Length });

            note = $"The files come to {files.Sum(f => (long)f.Code.Length):N0} characters, more than one answer carries. Pass file to read one of them, up to {ReadFileLimit:N0} characters; {archive} has every file.";
        }

        object? working = null;

        if (feature != null)
        {
            var info = feature.Feature;

            working = new
            {
                feature = info.Key,
                info.Name,
                info.Specification,
                info.Change,
                @base = info.Base,
                mergeable = info.Mergeable,
                previewUrl = $"{origin}{info.Path}",
                previewOnline = info.Online,
                // what it would have to take in before it can be merged, so
                // the agent knows which versions to read and compare
                newerVersions = info.Mergeable
                    ? null
                    : history.Where(v => v.Version > info.Base).OrderBy(v => v.Version).Select(v => new { v.Version, v.Change }),
                next = info.Mergeable
                    ? "It is based on the newest version: merge_feature merges it once it does what was asked."
                    : $"Before it can be merged, bring in what newerVersions changed (read_lambda with version, compare with version {info.Base}), then update_feature with base: {info.Newest}."
            };
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
            lambda.LatestVersion,
            online = lambda.ActiveVersion != null,
            lambda.DeployedUntil,
            lambda.KeptUntil,
            // the work under way beside the lambda, which a change should
            // continue rather than start over where one fits
            features = open.Select(f => new { feature = f.Key, f.Name, @base = f.Base, mergeable = f.Mergeable, previewOnline = f.Online, f.Modified }),
            workIn = lambda.ActiveVersion != null && feature == null
                ? "The lambda is online: make changes in a feature - create_feature, or continue one listed under features."
                : null,
            // what the lambda keeps, which no version holds and none brings back
            data = stores.ToDictionary(s => s.Kind, s => (object)new { s.Enabled, items = s.Items, s.UsedBytes, s.QuotaBytes }),
            version,
            specification = content?.Specification,
            change = content?.Change,
            feature = working,
            // the why of the recent past, so a change made on top of somebody
            // else's work can follow what they were trying to do - the one line
            // each, since a specification can be a page and this is read every time
            history = history.Take(10).Select(v => new { v.Version, v.Created, v.Change, v.Origin }),
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
            workspaceBytes = workspace.Quota,
            features = options.MaxFeatures
        };
    }

    #endregion

    #region Other

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
    /// </summary>
    private static string? DomainUrl(LambdaInfo lambda)
        => LambdaDescription.Serves(lambda.Tier, lambda.Domain) ? $"https://{lambda.Domain}/" : null;

    private static IEnumerable<object> Diagnostics(IReadOnlyList<CompilationDiagnostic> diagnostics)
        => diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message });

    #endregion

    #region Guide

    private JsonObject Guide() => McpProtocol.Say(new
    {
        ok = true,
        preferTheApi = "If you can make HTTP requests, the REST API at https://genhttp.dev/api/v1/openapi.json does the same as these tools and costs fewer tokens, because files are sent directly. GET /api/v1/lambdas/{privateKey}/versions/{version}/zip downloads a version; a feature is downloaded from and put back to /api/v1/lambdas/{privateKey}/features/{feature}/zip (GET, PUT) - so edit locally and push as often as it takes. POST /api/v1/lambdas/{privateKey}/versions/zip saves a zip as a new version. Every endpoint that saves takes ?deploy=true. Many environments cannot reach it; then use these tools.",
        whatALambdaIs = "C# that returns a GenHTTP handler, served at /lambda/{publicKey}/. No Main and no project: the snippet is the program.",
        lifecycle = new
        {
            threeThings = "A lambda holds versions, features and data, and they live differently. Versions are the program as it was saved. Features are changes being worked on beside it. Data is what the program keeps. Getting this right is most of getting a lambda right.",
            versions = new
            {
                what = "A version is the program: every .cs file and every asset - index.html, scripts, styles, icons, the whole front end. They are saved, deployed and rolled back together, and nothing else is in a version.",
                immutable = "A version never changes once it is saved. Deploying puts one online at the lambda's address, deploying an older one rolls back, and every version can be read back and put online again exactly as it was.",
                kept = $"The newest {options.MaxVersions} versions are kept; older ones are removed, never the one online. Fewer, meaningful versions keep more of the history that matters - one per thing the user asked for, not one per attempt."
            },
            features = new
            {
                what = "A feature is a change being worked on beside the lambda. It branches off a version with a copy of its files and a copy of the lambda's data, is changed in place as often as it takes, and has an address of its own, /features/{feature}/, to try it at. Features have no versions of their own.",
                when = "Change a lambda that exists - above all one that is online or in use - in a feature. The lambda goes on serving its visitors from the version online, untouched, while you work.",
                how = "create_feature, then change_code or write_code with feature (deploy: true puts it online at its previewUrl), read_logs with feature to see it answer, and merge_feature once it does what was asked - deploy: true puts the merged version live. delete_feature throws one away.",
                data = "A feature works on its own copy of the lambda's data, taken when it was created. Its preview can write, break and delete anything there without the lambda noticing. Merging discards the copy: the lambda keeps its own data, so code that changes how data is stored has to keep reading what is already there.",
                merging = "Merging makes the feature's files the next version. Only a feature based on the newest version is merged, so it cannot undo a version saved after it branched off - by the user, by another agent, or by merging another feature.",
                rebasing = "When a feature is behind - read_lambda with feature lists the newerVersions - bring what they changed into the feature yourself: read those versions, compare them with the feature's base, and apply the same changes with change_code and feature. Then move the base with update_feature (base: the newest) and merge. Nothing merges or rebases for you, and nothing checks that the changes really are in.",
                several = $"A lambda may have up to {options.MaxFeatures} features open. Continue one that fits rather than starting another; read_lambda lists them."
            },
            data = new
            {
                what = "Data belongs to the lambda, not to a version: every version reads and writes the same data. It is where everything the program keeps goes - records, accounts, scores, uploads, anything users create or change.",
                lifetime = "Data outlives every save, deploy, rollback and merge - no version holds it, so none of them changes it or brings an earlier state back. It goes only when the lambda is deleted, or when its owner switches that kind of data off, which deletes what it held.",
                kinds = "The workspace - a private directory of files - is the kind there is now. More kinds, such as a database and secrets, will be added the same way; read_lambda lists what a lambda has under data.",
                optIn = "The owner decides which kinds of data a lambda has. The workspace is on unless the owner switched it off: list_files and read_lambda say whether it is. You cannot switch data on - if it is off, ask the user.",
                beReady = "Data can be empty: a new lambda has none, and an owner can clear it. Have the code create what it needs on first use, and say so plainly where something it expects is missing, rather than fail."
            },
            whereThingsGo = new
            {
                theProgram = "In the version, as assets: the app itself - C#, and the whole front end including a single page application's HTML, JavaScript, CSS, images and fonts. Ship it with write_code under a folder such as web/ and serve it with Assets.App(\"web\").",
                theData = "In the workspace: everything the lambda writes while it runs, everything users create or upload, and large input files that are not program - a model, a dataset, media. Write it with Workspace from the code, or put a file there with upload_file.",
                neverTheOtherWay = "Never keep user data in assets: they are read only while the lambda runs and replaced on every deploy. Never upload the front end to the workspace: it would not be versioned, a rollback would not bring the matching pages back, and a feature would work on a copy of it."
            },
            flows = new
            {
                newLambda = "Nothing is online and nobody uses it yet: create_lambda, write_code with deploy: true, fix with change_code and deploy: true until it works. No feature needed.",
                changeALambda = "It exists and may be in use: create_feature (or continue an open one), change it with feature until its preview does what was asked, then merge_feature with deploy: true - unless the user wants to look at the preview first, in which case leave the feature and give them its previewUrl."
            },
            theLambda = new
            {
                lifetime = $"A free lambda goes offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits, and is removed - versions, features, data and all - about {(int)options.Retention.TotalDays} days after the last of either. A premium lambda stays online and is kept.",
                deleting = "Deleting a lambda deletes its versions, its features and its data together. Nothing else deletes versions one by one."
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
            why = "The same lambda answers at /lambda/{publicKey}/ on this platform, at the root of a domain of its own in the premium tier, and - as a feature - at /features/{feature}/. A path starting with / leaves the lambda: a hard-coded /lambda/{publicKey}/ does not exist on the domain, and from a preview it reaches the live lambda - so a feature's page would read and write the real data instead of its copy.",
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
            what = "The lambda's data, for now: a private directory it reads and writes at runtime, for anything that must outlive a request or a deployment. The same for every version; a feature has a copy of its own.",
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
            fromOutside = "upload_file puts a file there, list_files lists it, delete_file removes one - each with feature for a feature's copy. Over HTTP, for more than a tool call carries: PUT /api/v1/lambdas/{privateKey}/files/{path}/content with the file itself as the body (curl -T model.onnx ...), the slashes of the path encoded as %2F - streamed to the disk, however large. Or let the lambda fetch it once with HttpClient and keep it with Workspace.WriteBytes.",
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
            rule = "The front end is part of the program, so it is part of the version: ship index.html, scripts, styles and images as assets and serve them with Assets.App(\"web\"). It is then deployed, rolled back and merged together with the API it talks to.",
            how = "write_code with the files under a folder (web/index.html, web/app.js, web/app.css), return Layout.Create().Add(\"api\", api).Add(Assets.App(\"web\")), deploy. Change it like any other file.",
            example = "Every demo serves its front end like this, from web/ - read_lambda demo-crud.",
            notFromTheWorkspace = "Do not upload the app's own pages to the workspace with upload_file. They would not be versioned: a rollback would keep the new pages over the old API, and a feature would work on a copy of them that its merge throws away. The workspace is for data.",
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
            what = "Every save takes two optional notes: specification, what the user wants and why - their requirements, in their words where you can - and change, one line on what it does. A version keeps them; a feature keeps them and passes them on to the version it is merged into. The owner reads them in the version history, next to the code and a diff against the version before.",
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
            what = "read_logs shows what the lambda has been doing: each request and its status, what it printed, and any exception it threw with the stack trace, plus its traffic over the last hour and day. With feature, what the feature's preview did.",
            when = "After a deploy, call the address and read the logs to see that it answered. When somebody says it is broken, read the logs before reading the code."
        },
        whenThingsAreChecked = new
        {
            checkCode = "Compiles only.",
            deploy = "Compiles and builds the handler. A route with a return type that cannot be served passes check_code and fails here.",
            merge = "merge_feature compiles the feature before it becomes a version."
        },
        refused = new
        {
            what = "Reflection, processes, the environment, the file system, and anything else that reaches the host.",
            why = "All lambdas share one process."
        },
        thingsThatCatchPeopleOut = new[]
        {
            "A new version for every attempt at changing a lambda that is online. Work in a feature, try it at its preview address, and merge it once.",
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
            features = $"Up to {options.MaxFeatures} features open at once; each holds a copy of the files and of the workspace, within the same limits.",
            deployment = $"A free lambda is online while used, and offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits. A premium one stays online. A feature's preview of a free lambda goes offline after as long without being worked on.",
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

        /// <summary>Adds a version or changes a feature, and with deploy: true replaces what is online.</summary>
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
