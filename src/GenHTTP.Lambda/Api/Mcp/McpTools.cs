using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;
using GenHTTP.Lambda.Api.Infrastructure;

using Microsoft.Extensions.Logging;

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
/// A third is said the same way: every version keeps what is written about
/// it - what the app is for and why, how it is built and why, how it is
/// tested - in .lambda/ beside its program, and an agent writes it with a new
/// lambda and updates it with every change. read_lambda hands it over before
/// the files, and every save that leaves a page out says so.
///
/// The tools that save, deploy, read and reach into the data take an optional
/// feature, and then act on the feature rather than on the lambda - so the
/// same few tools do both, and nothing is learned twice.
///
/// Every tool answers with an object rather than prose. A model reads the text
/// and a program reads the structured copy, and both are the same thing.
///
/// Every tool that did something logs what it did, as the API does - reads
/// included, since what an agent looks at is how it goes about its work. See
/// <see cref="OperationLog"/> for what is left out.
/// </remarks>
public sealed class McpTools(IMetaService meta, IWorkspaceService workspace, IDataService data, IFeatureService features, ISecretService secrets,
                              IDatabaseService databases, IShowcaseService showcases, ISourceService sources, LambdaTelemetry telemetry, LogBook book,
                              LambdaOptions options, ILogger<McpTools> logger)
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
                     ["acceptTerms"] = Field("boolean", "Must be true: the user accepts the terms in platform_guide (free shared machine, deployments may be removed, nothing malicious)."),
                     ["view"] = Field("string", ViewField)
                 },
                 ["required"] = new JsonArray("acceptTerms")
             }),

        Tool("update_lambda", "Change a lambda's settings", Effect.Replace,
             "Change how a lambda's editor opens: view 'Simple' shows the app, how it is doing and a box to ask for a change - for an owner who does not write code - and 'Full' every section, the code, files, data, versions and logs included. Only the default: whoever opens the editor can switch for themselves, and that choice stays theirs. Only do this when the user asks for it.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["view"] = Field("string", ViewField)
                 },
                 ["required"] = new JsonArray("privateKey", "view")
             }),

        Tool("write_code", "Save all files", Effect.Save,
             "Save every file, replacing the previous set: as a new version of the lambda, or - with feature - into that feature. .cs files are compiled - lambda.cs returns the handler, others hold types; files under .lambda/ are what is written about the program - docs/product.md, docs/decisions.md, tests/README.md and the tests' scripts and data - kept with the version, never compiled or served; any other file is an asset, served as is and reachable as Assets: the whole front end (pages, scripts, styles, icons) goes here, as part of the program. What the lambda keeps at runtime is data, never files here: records and accounts in the database (a DbContext of Entity Framework Core on Database.GetConnection(), used synchronously; its schema as Evolve migrations shipped here in migrations/), uploads in the workspace - and so is a large input file such as a model or a dataset (upload_file). Send the documentation and tests with the code: written with a new lambda, updated with every change, in proportion to the lambda - a few lines and one quick check for a small one. Say why with specification and change. deploy: true publishes in the same call - a version at the public address, a feature at its preview address. To send only what changes, use change_code. To change a lambda that is already in use, work in a feature.",
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
                                 ["name"] = Field("string", ".cs files are compiled; .lambda/docs/... and .lambda/tests/... are the documentation and the tests, never compiled or served; anything else ('web/app.js', 'logo.png') is an asset."),
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
             "Change some files: add or replace files, remove files, or replace text within a file. Everything not named stays as it is, so there is no need to resend unchanged files. With feature, the change is saved into that feature - the way to work on a lambda that exists: change it as often as you like, deploy: true to try it at the feature's preview address, merge_feature when it is right. Without feature, the newest version is changed and saved as a new version. Update what the change affects in .lambda/docs/ and .lambda/tests/ in the same change - an edit of a passage is enough. check: true compiles without publishing. Code that does not compile is saved but never goes online.",
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
                                 ["name"] = Field("string", ".cs files are compiled; .lambda/docs/... and .lambda/tests/... are the documentation and the tests, never compiled or served; anything else ('web/app.js', 'logo.png') is an asset."),
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
             "Start a feature: a place to change a lambda without touching what it has online. It branches off a version - the newest unless base names another - with a copy of its files (its documentation and tests in .lambda/ included) and a copy of the lambda's data, and gets an address of its own to try it at. Change it with change_code or write_code and feature (deploy: true puts it online at its preview address), test it there, and merge_feature once it does what was asked. The lambda goes on serving its visitors from the version online the whole time.",
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
             "Make a feature's files the next version of the lambda, and delete the feature with its preview and its copy of the data - the lambda's own data is not touched. Its .lambda/ becomes the version's documentation and tests, so bring them up to date in the feature first, and run its tests against the preview. Refused while the feature is not based on the newest version (update_feature says how to get it there), and while its code does not compile. deploy: true puts the new version online at once; otherwise deploy it when the user wants it live.",
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
                                 ["name"] = Field("string", ".cs files are compiled; files under .lambda/ are documentation and tests and never compiled; anything else is an asset."),
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
             $"A lambda's status (online version, newest version, expiry, its tier and what it may use there), its open features, the recent versions with what each was asked for and changed, its data (the database, the workspace and the secrets: whether each is on, what it holds, and which secrets the code reads that have no value yet), and one version - or, with feature, that feature: its documentation (what the app is and why, the technical decisions), how it is tested, and its files. The documentation comes first and in full up to {ContextBudget:N0} characters; the files come in full when they add up to at most {ReadBudget:N0} characters, otherwise by name and length, with file to read one - a test script or its data too. Read the documentation and the history before changing what you did not write. Also how a demo is read: pass its key from list_demos.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["version"] = Field("integer", "Defaults to the newest."),
                     ["feature"] = Field("string", "Read this feature - its files, its base, and which newer versions it would have to take in before it can be merged - instead of a version."),
                     ["file"] = Field("string", $"Return only this file, in full however large the rest is - up to {ReadFileLimit:N0} characters, beyond which the zip has it. Any file of the version, '.lambda/tests/smoke.mjs' included.")
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
             "Write a file to the lambda's workspace - its data, which every version shares and no deploy, rollback or merge touches. With feature, to that feature's copy of the data instead, for trying things out without touching the real one. For files the lambda works with at runtime (pictures people will browse) and large input files that are not program: a model, a dataset, media. Records belong in the database, where the code writes them. Takes effect at once, without a deploy. Not for the front end: pages, scripts and styles are the program and belong in the version as assets (write_code).",
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

        Tool("enable_data", "Switch on a kind of data", Effect.Replace,
             "Switch on a kind of data the lambda needs and does not have: 'database' (a SQLite database for its records - off until switched on, which makes it, empty), 'secrets' (off until switched on) or 'workspace' (on unless the owner switched it off). Do it when what you build needs it - records need the database, an API key for a service it calls needs secrets. Takes effect at once, without a deploy; switching the database on starts the lambda again on its next request, so its migrations run. There is no tool to switch one off: that deletes what it held, and is the owner's to do in the editor.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["kind"] = Field("string", $"Which kind: {string.Join(" or ", DataKinds.All.Select(k => $"'{k.Id}'"))}.")
                 },
                 ["required"] = new JsonArray("privateKey", "kind")
             }),

        Tool("read_database", "Read the lambda's database", Effect.Read,
             $"The lambda's database - its records, shared by every version: whether it is switched on, how full it is, and its tables and views with their columns and how many rows each holds. With table, a page of that table's rows, newest first, up to {MaxDatabaseRows} at a time. Read only: the lambda writes it, through Database.GetConnection(). Use it to see that migrations were applied and records were written. With feature, that feature's copy.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["feature"] = Field("string", "Read this feature's copy of the database instead."),
                     ["table"] = Field("string", "A table or view to read the rows of; left out, the tables are listed."),
                     ["offset"] = Field("integer", "Rows to skip, for the next page."),
                     ["limit"] = Field("integer", $"Rows to read, up to {MaxDatabaseRows}. Left out, 20."),
                     ["order"] = Field("string", "A column to sort by; left out, the order the rows were written in."),
                     ["ascending"] = Field("boolean", "Oldest or smallest first, rather than newest or largest.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("set_secret", "Store a secret", Effect.Replace,
             $"Store an API key, password or token under a name the code reads with Secret.Read(\"NAME\") - never put one in the code, an asset, the workspace, a log line or a note. The value is sealed and can never be read back, by you or the owner; only the lambda reads it, from its next call on, without a deploy. Only set a value the user gave you for this: when you do not have it, write the code with Secret.Read and tell the user to set it under Data > Secrets in the editor - read_lambda and list_secrets list what is still missing. Secrets must be switched on first (enable_data). With feature, into that feature's copy (a sandbox key to try it with). Up to {SecretVault.MaxSecrets} secrets of {SecretVault.MaxValue / 1024} KB each.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "Store it in this feature's copy of the secrets instead."),
                     ["name"] = Field("string", "Letters, digits and underscores, not starting with a digit - conventionally upper case: 'STRIPE_KEY'. The name of an environment variable in an exported project."),
                     ["value"] = Field("string", "The value, as the user gave it.")
                 },
                 ["required"] = new JsonArray("privateKey", "name", "value")
             }),

        Tool("list_secrets", "List a lambda's secrets", Effect.Read,
             "The lambda's secrets by name - never their values: whether secrets are switched on, what is stored and when, which names the code reads with Secret.Read and have no value yet (missing) - what the user still has to set - and which it only uses when they are there, checked with Secret.Exists (optional). With feature, that feature's copy.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key, or the key of a demo."),
                     ["feature"] = Field("string", "List this feature's copy instead.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("delete_secret", "Delete a secret", Effect.Replace,
             "Remove a secret. Code that reads it with Secret.Read fails from its next call on. Only when the user asks for it, or a secret you stored yourself is no longer used. With feature, from that feature's copy instead.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["feature"] = Field("string", "Delete from this feature's copy instead."),
                     ["name"] = Field("string", "The name, as stored - names are case sensitive.")
                 },
                 ["required"] = new JsonArray("privateKey", "name")
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

        Tool("open_source", "Publish a lambda's source code", Effect.Replace,
             $"Publish a lambda's source code at /source/{{publicKey}} for anybody to read, star and download as a .NET project, change the license it is published under, or take it down. What is published is the program and what is written about it - every version's code, front end, documentation and tests, and the history of what each changed - never its data: not the database, not the workspace, not a secret's value. Not part of building: only do this when the user asks for it, and ask which license if they did not say (MIT unless they want another). With only privateKey it returns whether it is published. Once published, everything in every version is public, older ones included: keep keys, passwords and personal data out of the files - in secrets and the database.",
             new JsonObject
             {
                 ["type"] = "object",
                 ["properties"] = new JsonObject
                 {
                     ["privateKey"] = Field("string", "The editor key."),
                     ["license"] = Field("string", $"The SPDX identifier of the license: {string.Join(", ", SourceLicenses.All.Select(l => l.Id))}. Left out, it stays as it is - {SourceLicenses.Default} when first published."),
                     ["author"] = Field("string", $"Who holds the copyright, as the license names them - the user's name or organization, only if they gave it. Left out, it stays as it is; empty, the license names 'the authors of' the lambda. Up to {SourceLicenses.MaxAuthor} characters."),
                     ["remove"] = Field("boolean", "Take the source down instead. Its stars are kept for when it is published again.")
                 },
                 ["required"] = new JsonArray("privateKey")
             }),

        Tool("list_demos", "List the demos", Effect.Read,
             "Demos this platform keeps online, each a finished lambda showing one way to build something: a REST API over records, registration and login, a websocket game, uploads, live updates. Their keys are public and read only: read the closest one with read_lambda (and list_files, read_logs) before writing similar code. create_lambda with a demo's id as template starts from a copy.",
             new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }),

        Tool("platform_guide", "Read the platform guide", Effect.Read,
             "How this platform works - read it first: what a version, a feature and data are and how long each lives, the documentation and tests every version keeps beside its program, what the snippet returns, what is imported, what is refused, limits and terms.",
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
                "create_lambda" => Create(arguments, origin),
                "update_lambda" => Update(arguments, origin),
                "write_code" => await WriteAsync(arguments, origin),
                "change_code" => await ChangeAsync(arguments, origin),
                "create_feature" => await CreateFeatureAsync(arguments, origin),
                "update_feature" => UpdateFeature(arguments, origin),
                "merge_feature" => await MergeFeatureAsync(arguments, origin),
                "delete_feature" => DeleteFeature(arguments),
                "check_code" => await CheckAsync(arguments),
                "deploy" => await DeployAsync(arguments, origin),
                "read_lambda" => Read(arguments, origin),
                "read_logs" => Logs(arguments),
                "upload_file" => await UploadAsync(arguments),
                "list_files" => Files(arguments),
                "delete_file" => Remove(arguments),
                "enable_data" => EnableData(arguments),
                "read_database" => await DatabaseAsync(arguments),
                "set_secret" => SetSecret(arguments),
                "list_secrets" => Secrets(arguments),
                "delete_secret" => DeleteSecret(arguments),
                "showcase" => Showcase(arguments, origin),
                "open_source" => OpenSource(arguments, origin),
                "list_demos" => ListDemos(origin),
                "platform_guide" => ReadGuide(),
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

    private JsonObject Create(JsonObject arguments, string origin)
    {
        if (Flag(arguments, "acceptTerms") != true)
        {
            return McpProtocol.Refuse(
                "acceptTerms must be true. Show the user the terms from platform_guide first.");
        }

        var view = EditorViews.Parse(Text(arguments, "view")) ?? EditorView.Full;

        var lambda = meta.Create(Text(arguments, "publicKey"), Text(arguments, "template"), view);

        logger.LogInformation("Created lambda {Lambda} template {Template} view {View}", lambda.PublicKey,
                              Text(arguments, "template") ?? "(none)", lambda.View);

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            privateKey = lambda.PrivateKey,
            publicUrl = $"{origin}/lambda/{lambda.PublicKey}/",
            editorUrl = $"{origin}/editor/{lambda.PrivateKey}",
            view = lambda.View,
            next = "write_code with deploy: true, and fix what needs fixing with change_code and deploy: true - a new lambda needs no feature. Send .lambda/docs/product.md, .lambda/docs/decisions.md and .lambda/tests/README.md with the code - short for a small app (platform_guide, documentationAndTests). Once people use it, make further changes in a feature (create_feature).",
            warning = "The editor key cannot be recovered. Give it to the user."
        });
    }

    private JsonObject Update(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var view = EditorViews.Parse(Required(arguments, "view"))!.Value;

        var lambda = meta.ChangeView(privateKey, view);

        logger.LogInformation("Set view of lambda {Lambda} to {View}", lambda.PublicKey, lambda.View);

        return McpProtocol.Say(new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            editorUrl = $"{origin}/editor/{lambda.PrivateKey}",
            view = lambda.View,
            note = "The editor opens in this view for everybody who has not switched it for themselves; whoever has keeps their own."
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
            var content = features.Get(privateKey, feature);

            current = LambdaSource.Parse(content.Code);

            read = content.Feature.Revision;
        }
        else
        {
            current = Api.VersionResource.Latest(meta, privateKey);
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

        // said where the next save is decided, and as a request rather than a
        // refusal - like the note below: the code matters more
        var documentation = Unwritten(files);

        if (Text(arguments, "feature") is { } feature)
        {
            return await SaveFeatureAsync(arguments, privateKey, feature, code, files.Count, note, origin, read, documentation);
        }

        var version = meta.Save(privateKey, code, note);

        logger.LogInformation("Saved lambda {Lambda} version {Version} files {Files}", meta.PublicKeyOf(privateKey), version.Version, files.Count);

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
                    note = reminder,
                    documentation
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
                note = reminder,
                documentation
            });
        }

        return await DeployVersionAsync(privateKey, version.Version, origin, reminder, documentation);
    }

    /// <summary>
    /// Stores the files as the feature's, and puts its preview online or
    /// compiles it if the arguments ask for that.
    /// </summary>
    private async ValueTask<JsonObject> SaveFeatureAsync(JsonObject arguments, string privateKey, string feature, string code, int files, VersionNote note,
                                                         string origin, int? read, string? documentation)
    {
        var saved = features.Save(privateKey, feature, code, note, read);

        logger.LogInformation("Saved feature '{Feature}' of lambda {Lambda} files {Files}", saved.Name, meta.PublicKeyOf(privateKey), files);

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
                    : $"fix the diagnostics with change_code and feature: '{saved.Key}'",
                documentation
            });
        }

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(saved, origin),
            next = $"Saved into the feature; nothing is online yet. Try it with deploy and feature: '{saved.Key}' (or deploy: true on the next save), then merge_feature when it does what was asked.",
            documentation
        });
    }

    private async ValueTask<JsonObject> CheckAsync(JsonObject arguments)
    {
        var files = Files(arguments, out var complaint);

        if (files == null)
        {
            return McpProtocol.Refuse(complaint!);
        }

        var privateKey = Required(arguments, "privateKey");

        var outcome = await meta.CheckAsync(privateKey, LambdaSource.Serialize(files));

        logger.LogInformation("Checked code of lambda {Lambda} files {Files} compiles {Compiles}", meta.PublicKeyOf(privateKey), files.Count, outcome.Success);

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

        logger.LogInformation("Created feature '{Feature}' of lambda {Lambda} base {Version}", created.Name, meta.PublicKeyOf(privateKey), created.Base);

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(created, origin),
            next = $"Change it with change_code (or write_code) and feature: '{created.Key}' - it holds the files of version {created.Base} and a copy of the lambda's data. deploy: true on a save puts it online at previewUrl, never at the lambda's address; read_logs with feature shows how it answers. merge_feature when it does what was asked."
        });
    }

    private JsonObject UpdateFeature(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var updated = features.Update(privateKey, Required(arguments, "feature"),
                                                 new FeatureUpdate(Text(arguments, "name"), Text(arguments, "specification"), Text(arguments, "change"),
                                                                   Number(arguments, "base")));

        logger.LogInformation("Updated feature '{Feature}' of lambda {Lambda} base {Version}", updated.Name, meta.PublicKeyOf(privateKey), updated.Base);

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

        var publicKey = meta.PublicKeyOf(privateKey);

        if (merged.Merged)
        {
            logger.LogInformation("Merged feature '{Feature}' of lambda {Lambda} as version {Version}", merged.Name, publicKey, merged.Version?.Version);
        }
        else
        {
            logger.LogInformation("Failed to merge feature '{Feature}' of lambda {Lambda}", merged.Name, publicKey);
        }

        if (merged.Deployment is { } online)
        {
            logger.Deployed(online, publicKey, merged.Version?.Version);
        }

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
            var current = meta.Get(privateKey);

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

    private JsonObject DeleteFeature(JsonObject arguments)
    {
        var feature = Required(arguments, "feature");

        var privateKey = Required(arguments, "privateKey");

        var name = features.Delete(privateKey, feature);

        logger.LogInformation("Deleted feature '{Feature}' of lambda {Lambda}", name, meta.PublicKeyOf(privateKey));

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
    /// <remarks>
    /// The program only: documentation that names the lambda's address is
    /// saying where it is, not linking there from a page. The same goes for
    /// meta tags and a canonical link - og:image needs the full address, since
    /// social networks do not resolve a relative one, and the page itself
    /// never follows it.
    /// </remarks>
    private static string? Leaks(IReadOnlyList<LambdaFile> files, string publicKey)
        => files.Any(f => !f.IsContext && LinksTo(f.Code, publicKey) && LinksTo(Named.Replace(f.Code, string.Empty), publicKey))
            ? $"The code links to /lambda/{publicKey}/ by its full path. From the preview that is the live lambda, with its real data - not the feature's copy. Use relative paths (\"api/items\", not \"/lambda/{publicKey}/api/items\")."
            : null;

    private static bool LinksTo(string code, string publicKey)
        => code.Contains($"/lambda/{publicKey}/", StringComparison.Ordinal) || code.Contains($"/lambda/{publicKey}\"", StringComparison.Ordinal);

    /// <summary>
    /// Where a page names an address rather than linking to it: its meta tags
    /// and its canonical link.
    /// </summary>
    private static readonly Regex Named = new(@"<meta\b[^>]*>|<link\b[^>]*\brel\s*=\s*[""']?canonical\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// What is said when a version or a feature lacks the pages of its
    /// context, where the next save is decided - and nothing when it has them.
    /// </summary>
    private static string? Unwritten(IReadOnlyList<LambdaFile> files)
    {
        var missing = ContextPages.Missing(files);

        if (missing.Count == 0)
        {
            return null;
        }

        return missing.Count == LambdaSource.ExpectedContext.Count
            ? $"Nothing is written about this lambda yet. Add {string.Join(", ", missing)} with your next save: what the app is, who it is for and why; the technical decisions and why; how to test it. platform_guide says more under documentationAndTests."
            : $"Missing: {string.Join(", ", missing)}. Add it with your next save - platform_guide says what goes in it, under documentationAndTests.";
    }

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

        var (id, feature) = WorkspaceOf(privateKey, Text(arguments, "feature"), true);

        WorkspaceEntry written;

        if (Text(arguments, "encoding") == "base64")
        {
            written = await workspace.WriteEncodedAsync(id, path, content, feature);
        }
        else
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            written = await workspace.WriteAsync(id, path, stream, featureId: feature);
        }

        if (Text(arguments, "feature") is { } named)
        {
            logger.LogInformation("Wrote workspace file {Path} of feature '{Feature}' of lambda {Lambda} size {Size}", written.Path,
                                  features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey), written.Size);
        }
        else
        {
            logger.LogInformation("Wrote workspace file {Path} of lambda {Lambda} size {Size}", written.Path, meta.PublicKeyOf(privateKey), written.Size);
        }

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

    private JsonObject Files(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var (id, feature) = WorkspaceOf(privateKey, Text(arguments, "feature"), false);

        var listing = workspace.List(id, feature);

        if (Text(arguments, "feature") is { } named)
        {
            logger.LogInformation("Listed workspace of feature '{Feature}' of lambda {Lambda}", features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Listed workspace of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

        return McpProtocol.Say(new
        {
            enabled = listing.Enabled,
            files = listing.Files.Select(f => new { f.Path, f.Size, f.Modified }),
            listing.Folders,
            listing.UsedBytes,
            listing.QuotaBytes,
            note = listing.Enabled
                ? null
                : "The workspace is switched off: it holds nothing, and code that uses Workspace fails. The owner switched it off, so ask the user before switching it on again with enable_data."
        });
    }

    private JsonObject EnableData(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var store = data.Enable(privateKey, Required(arguments, "kind"));

        logger.LogInformation("Enabled {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return McpProtocol.Say(new
        {
            ok = true,
            kind = store.Kind,
            enabled = store.Enabled,
            next = store.Kind switch
            {
                DataKinds.SecretsId => "Secrets are on, and empty. Store a value the user gave you with set_secret, or tell the user to set it under Data > Secrets in the editor. The code reads it with Secret.Read(\"NAME\").",
                DataKinds.DatabaseId => "The database is on, and empty. Ship its schema as SQL migrations in migrations/ (V1__Create_items.sql) and apply them with Evolve at the top of lambda.cs - see platform_guide under database, or demo-crud. The code keeps its records with a DbContext of Entity Framework Core on the connection Database.GetConnection() opens, synchronously; read_database shows what it holds.",
                _ => "On from the lambda's next request, without a deploy."
            }
        });
    }

    /// <summary>
    /// Stores a secret the user handed the agent.
    /// </summary>
    /// <remarks>
    /// The answer repeats the name and never the value, so the value is in the
    /// conversation once - where the user put it - rather than twice.
    /// </remarks>
    private JsonObject SetSecret(JsonObject arguments)
    {
        var feature = Text(arguments, "feature");

        var privateKey = Required(arguments, "privateKey");

        var stored = secrets.Set(privateKey, Required(arguments, "name"), Required(arguments, "value"), feature);

        // the name only - the value is not repeated anywhere, here least of all
        if (feature != null)
        {
            logger.LogInformation("Set secret {Name} of feature '{Feature}' of lambda {Lambda}", stored.Name, features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Set secret {Name} of lambda {Lambda}", stored.Name, meta.PublicKeyOf(privateKey));
        }

        return McpProtocol.Say(new
        {
            ok = true,
            name = stored.Name,
            usedByCode = stored.Used,
            note = (feature == null
                       ? "Stored in the lambda's secrets: its code reads it from its next call on, without a deploy. "
                       : "Stored in the feature's copy: its preview reads it; the lambda's own secrets are not touched. ")
                 + (stored.Used ? "" : $"No code reads it yet - read it with Secret.Read(\"{stored.Name}\"). ")
                 + "Do not repeat the value anywhere - not in code, notes, logs or your answer."
        });
    }

    private JsonObject Secrets(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var listing = secrets.List(privateKey, Text(arguments, "feature"));

        if (Text(arguments, "feature") is { } named)
        {
            logger.LogInformation("Listed secrets of feature '{Feature}' of lambda {Lambda}", features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Listed secrets of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

        return McpProtocol.Say(new
        {
            enabled = listing.Enabled,
            secrets = listing.Secrets.Select(s => new { s.Name, s.Changed, usedByCode = s.Used }),
            listing.Missing,
            optional = listing.Optional.Count > 0 ? listing.Optional : null,
            limit = listing.Limit,
            note = !listing.Enabled
                ? "Secrets are switched off. enable_data with kind 'secrets' switches them on when what you build needs one."
                : listing.Missing.Count > 0
                    ? $"The code reads {string.Join(", ", listing.Missing)}, which have no value: Secret.Read throws until they are set. Ask the user to set them under Data > Secrets in the editor, or store one they gave you with set_secret."
                    : null
        });
    }

    /// <summary>
    /// The tables of a lambda's database, or a page of the rows of one.
    /// </summary>
    /// <remarks>
    /// Rows go out as objects keyed by column, which is what a model reads
    /// best; the editor gets arrays, which is what a grid draws best.
    /// </remarks>
    private async ValueTask<JsonObject> DatabaseAsync(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var feature = Text(arguments, "feature");

        var table = Text(arguments, "table");

        if (feature != null)
        {
            logger.LogInformation("Read database of feature '{Feature}' of lambda {Lambda} table {Table}", features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey),
                                  table ?? "(all)");
        }
        else
        {
            logger.LogInformation("Read database of lambda {Lambda} table {Table}", meta.PublicKeyOf(privateKey), table ?? "(all)");
        }

        if (table == null)
        {
            var overview = await databases.GetAsync(privateKey, feature);

            return McpProtocol.Say(new
            {
                enabled = overview.Enabled,
                overview.UsedBytes,
                overview.QuotaBytes,
                usedByCode = overview.Used,
                tables = overview.Tables.Select(t => new
                {
                    t.Name,
                    kind = t.Kind == "table" ? null : t.Kind,
                    t.Rows,
                    columns = t.Columns.Select(c => $"{c.Name} {c.Type}{(c.PrimaryKey ? " PRIMARY KEY" : "")}{(c.NotNull && !c.PrimaryKey ? " NOT NULL" : "")}".TrimEnd()),
                    migrations = t.Migrations ? "Evolve's record of the migrations it applied" : null
                }),
                note = !overview.Enabled
                    ? "The database is switched off. enable_data with kind 'database' switches it on when what you build keeps records."
                    : overview.Tables.Count == 0
                        ? "The database is empty: no migration has made a table yet. Ship them in migrations/ and apply them with Evolve as the lambda starts."
                        : null
            });
        }

        var rows = await databases.ReadAsync(privateKey, table, Number(arguments, "offset") ?? 0, Math.Clamp(Number(arguments, "limit") ?? 20, 1, MaxDatabaseRows),
                                             Text(arguments, "order"), Flag(arguments, "ascending") != true, feature);

        return McpProtocol.Say(new
        {
            rows.Table,
            rows.Total,
            rows.Offset,
            rows = rows.Rows.Select(r => rows.Columns.Select((c, i) => (c.Name, Value: r[i])).ToDictionary(p => p.Name, p => p.Value)),
            next = rows.Offset + rows.Rows.Count < rows.Total ? rows.Offset + rows.Rows.Count : (int?)null
        });
    }

    /// <summary>
    /// The most rows read_database answers with at once.
    /// </summary>
    private const int MaxDatabaseRows = 100;

    private JsonObject DeleteSecret(JsonObject arguments)
    {
        var name = Required(arguments, "name");

        var privateKey = Required(arguments, "privateKey");

        var feature = Text(arguments, "feature");

        secrets.Delete(privateKey, name, feature);

        if (feature != null)
        {
            logger.LogInformation("Deleted secret {Name} of feature '{Feature}' of lambda {Lambda}", name.Trim(), features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Deleted secret {Name} of lambda {Lambda}", name.Trim(), meta.PublicKeyOf(privateKey));
        }

        return McpProtocol.Say(new { ok = true, deleted = name.Trim() });
    }

    private JsonObject Remove(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var (id, feature) = WorkspaceOf(privateKey, Text(arguments, "feature"), true);

        var path = Required(arguments, "path");

        workspace.Delete(id, path, feature);

        if (Text(arguments, "feature") is { } named)
        {
            logger.LogInformation("Deleted workspace path {Path} of feature '{Feature}' of lambda {Lambda}", path, features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Deleted workspace path {Path} of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));
        }

        return McpProtocol.Say(new { ok = true, path });
    }

    /// <summary>
    /// The workspace a call is about: the lambda's own, or a feature's copy.
    /// </summary>
    private (long LambdaId, long? FeatureId) WorkspaceOf(string privateKey, string? feature, bool editable)
    {
        if (feature != null)
        {
            var (lambdaId, featureId) = features.Require(privateKey, feature, editable);

            return (lambdaId, featureId);
        }

        if (editable)
        {
            return (meta.RequireEditable(privateKey), null);
        }

        return (meta.GetId(privateKey) ?? throw LambdaException.NotFound("There is no lambda with that editor key."), null);
    }

    #endregion

    #region Deploying

    private ValueTask<JsonObject> DeployAsync(JsonObject arguments, string origin)
        => Text(arguments, "feature") is { } feature
         ? DeployFeatureAsync(Required(arguments, "privateKey"), feature, origin)
         : DeployVersionAsync(Required(arguments, "privateKey"), Number(arguments, "version"), origin);

    private async ValueTask<JsonObject> DeployVersionAsync(string privateKey, int? version, string origin, string? reminder = null, string? documentation = null)
    {
        var result = await meta.DeployAsync(privateKey, version, VersionOrigins.Agent);

        logger.Deployed(result, meta.PublicKeyOf(privateKey), version);

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
            var current = meta.Get(privateKey);

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
            note = reminder ?? "Deploying again extends onlineUntil.",
            documentation
        });
    }

    private async ValueTask<JsonObject> DeployFeatureAsync(string privateKey, string feature, string origin)
    {
        var result = await features.DeployAsync(privateKey, feature);

        logger.Previewed(result, meta.PublicKeyOf(privateKey));

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
        var lambda = meta.Get(privateKey);

        var files = LambdaSource.Parse((features.Get(privateKey, feature)).Code);

        var warning = lambda != null ? Leaks(files, lambda.PublicKey) : null;

        return McpProtocol.Say(new
        {
            ok = true,
            feature = Brief(result.Feature, origin),
            previewUrl = $"{origin}{result.Feature.Path}",
            warning,
            next = result.Feature.Mergeable
                ? $"Call previewUrl, and read_logs with feature: '{result.Feature.Key}' to see how it answered. It works on its own copy of the data. Run what .lambda/tests/README.md describes against previewUrl, and bring .lambda/docs/ and .lambda/tests/ up to date with what the feature changes. When it does what was asked: merge_feature (deploy: true to put it live)."
                : $"Call previewUrl, and read_logs with feature to see how it answered. Before merging: {Later(result.Feature)} saved after this feature's base, version {result.Feature.Base} - bring those changes in and move the base to {result.Feature.Newest} with update_feature.",
            documentation = Unwritten(files)
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
    private JsonObject Logs(JsonObject arguments)
    {
        var privateKey = Required(arguments, "privateKey");

        var feature = Text(arguments, "feature");

        var id = meta.GetId(privateKey)
              ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var lines = FeatureLines.None;

        if (feature != null)
        {
            lines = FeatureLines.Of((features.Require(privateKey, feature, false)).FeatureId);
        }

        var lambda = meta.Get(privateKey);

        if (feature != null)
        {
            logger.LogInformation("Read logs of feature '{Feature}' of lambda {Lambda}", features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Read logs of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

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

    private JsonObject Read(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        var lambda = meta.Get(privateKey)
                  ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var history = meta.GetVersions(privateKey);

        var open = features.List(privateKey);

        var wanted = Text(arguments, "feature");

        // read for a feature, the data it works on is its own copy
        var stores = data.List(privateKey, wanted);

        // by name only, and which the code waits for
        var kept = secrets.List(privateKey, wanted);

        int? version = null;

        LambdaVersionContent? content = null;

        FeatureContent? feature = null;

        IReadOnlyList<LambdaFile> files = [];

        if (wanted != null)
        {
            feature = features.Get(privateKey, wanted);

            files = LambdaSource.Parse(feature.Code);
        }
        else
        {
            version = Number(arguments, "version") ?? lambda.LatestVersion;

            if (version is { } which)
            {
                content = meta.GetVersion(privateKey, which);

                files = LambdaSource.Parse(content.Code);
            }
        }

        if (feature != null)
        {
            logger.LogInformation("Read feature '{Feature}' of lambda {Lambda}", feature.Feature.Name, lambda.PublicKey);
        }
        else
        {
            logger.LogInformation("Read lambda {Lambda} version {Version}", lambda.PublicKey, version?.ToString() ?? "(none)");
        }

        // whose code is public, which changes what may be written into it
        var published = sources.Get(privateKey) is { Published: true } source ? source : null;

        var archive = feature != null
            ? $"GET /api/v1/lambdas/{{privateKey}}/features/{feature.Feature.Key}/zip"
            : $"GET /api/v1/lambdas/{{privateKey}}/versions/{version}/zip";

        var only = Text(arguments, "file");

        // the program; what is written about it is handed over apart, and first
        var program = files.Where(f => !f.IsContext).ToList();

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
        else if (program.Sum(f => (long)f.Code.Length) <= ReadBudget)
        {
            listing = program.Select(f => new { f.Name, f.Code });
        }
        else
        {
            listing = program.Select(f => new { f.Name, length = f.Code.Length });

            note = $"The files come to {program.Sum(f => (long)f.Code.Length):N0} characters, more than one answer carries. Pass file to read one of them, up to {ReadFileLimit:N0} characters; {archive} has every file.";
        }

        // asked for one file, the answer is about that file
        var (documentation, tests) = only == null ? Context(files) : (null, null);

        var unwritten = only == null ? Unwritten(files) : null;

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
            lambda.View,
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
            data = stores.ToDictionary(s => s.Kind, s => s.Kind switch
            {
                DataKinds.SecretsId => (object)new
                {
                    s.Enabled,
                    names = kept.Secrets.Select(n => n.Name),
                    missing = kept.Missing.Count > 0 ? kept.Missing : null,
                    note = kept.Missing.Count > 0
                        ? "The code reads secrets that have no value yet: Secret.Read throws until the user sets them under Data > Secrets in the editor (or you store one they gave you with set_secret)."
                        : null
                },
                DataKinds.DatabaseId => new
                {
                    s.Enabled,
                    tables = s.Items,
                    s.UsedBytes,
                    s.QuotaBytes,
                    note = !s.Enabled && files.Any(f => f.IsCode && DatabaseService.Uses(f.Code))
                        ? "The code connects to the database, which is switched off: Database.GetConnection() throws until it is on (enable_data with kind 'database')."
                        : s.Enabled ? "read_database lists its tables and reads their rows." : null
                },
                _ => new { s.Enabled, items = s.Items, s.UsedBytes, s.QuotaBytes }
            }),
            version,
            specification = content?.Specification,
            change = content?.Change,
            openSource = published == null
                ? null
                : new
                {
                    license = published.License,
                    url = $"{origin}/source/{lambda.PublicKey}",
                    note = "The owner published this lambda's source: every version is public, and so is what you save next - code, assets, documentation, tests and each version's change line. Keep keys, passwords and personal data out of the files; they belong in secrets and the database, which are never published."
                },
            feature = working,
            // what the app is for and why it is built as it is, before anything
            // else about it: what a change has to keep
            documentation,
            tests,
            documentationNote = unwritten,
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
    /// What is written about a version, as read_lambda hands it over: its
    /// documentation and how it is tested, apart from its files.
    /// </summary>
    /// <remarks>
    /// Apart, because it is read first and for another reason: what the app
    /// is for and why it is built the way it is says what a change has to
    /// keep, which the code does not. Within a budget of its own, so a long
    /// page cannot push the program out of the answer, nor the program the
    /// pages; a page that does not fit is named with its length, like a file.
    /// The pages are the markdown, the known ones first. Anything else - a
    /// picture, a script, the data a test uses - is named with its length and
    /// read with file when it is wanted.
    /// </remarks>
    private static (object Documentation, object Tests) Context(IReadOnlyList<LambdaFile> files)
    {
        var budget = ContextBudget;

        object Part(string folder, params string[] known)
        {
            var inside = files.Where(f => f.Name.StartsWith(folder, StringComparison.Ordinal)).ToList();

            var pages = inside.Where(f => f.Encoding != "base64" && f.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                              .OrderBy(f => Array.IndexOf(known, f.Name) is var at and >= 0 ? at : known.Length)
                              .ThenBy(f => f.Name, StringComparer.Ordinal)
                              .ToList();

            var shown = new List<object>();

            foreach (var page in pages)
            {
                if (page.Code.Length <= budget)
                {
                    budget -= page.Code.Length;

                    shown.Add(new { page.Name, content = page.Code });
                }
                else
                {
                    shown.Add(new { page.Name, length = page.Code.Length, read = "Longer than this answer carries beside the rest: pass it as file." });
                }
            }

            var others = inside.Except(pages).Select(f => new { f.Name, length = f.Code.Length }).ToList();

            var missing = known.Where(k => string.IsNullOrWhiteSpace(ContextPages.Read(files, k))).ToList();

            return new
            {
                pages = shown,
                files = others.Count > 0 ? others : null,
                missing = missing.Count > 0 ? missing : null
            };
        }

        return (Part(LambdaSource.DocsFolder, LambdaSource.ProductDoc, LambdaSource.DecisionsDoc),
                Part(LambdaSource.TestsFolder, LambdaSource.TestingDoc));
    }

    /// <summary>
    /// How much of the documentation and the tests read_lambda sends in one
    /// answer, beside the files.
    /// </summary>
    /// <remarks>
    /// Enough for three pages that say what they should without saying it
    /// three times; a page past it is named with its length rather than
    /// sent, so the answer stays under what a client accepts.
    /// </remarks>
    private const int ContextBudget = 20_000;

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
            databaseBytes = options.DatabaseOf(tier),
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
    private JsonObject Showcase(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        if (Flag(arguments, "remove") == true)
        {
            showcases.Remove(privateKey);

            logger.LogInformation("Removed lambda {Lambda} from showcase", meta.PublicKeyOf(privateKey));

            return McpProtocol.Say(new { ok = true, showcased = false });
        }

        var title = Text(arguments, "title");
        var description = Text(arguments, "description");
        var encoded = Text(arguments, "image");

        ShowcaseInfo? entry;

        if (title == null && description == null && encoded == null)
        {
            entry = showcases.Get(privateKey);

            logger.LogInformation("Read showcase entry of lambda {Lambda}", meta.PublicKeyOf(privateKey));
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
            var current = showcases.Get(privateKey);

            entry = showcases.Save(privateKey, new ShowcaseDraft(title ?? current?.Title, description ?? current?.Description, image));

            logger.LogInformation("Added lambda {Lambda} to showcase as '{Title}'", entry.PublicKey, entry.Title);
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

    /// <summary>
    /// Reads, publishes or takes down the source of a lambda.
    /// </summary>
    /// <remarks>
    /// One tool, like the showcase, for the same reason: it is used rarely,
    /// and every tool listed is read by every agent in every conversation.
    /// </remarks>
    private JsonObject OpenSource(JsonObject arguments, string origin)
    {
        var privateKey = Required(arguments, "privateKey");

        SourceSettings? source;

        if (Flag(arguments, "remove") == true)
        {
            source = sources.Withdraw(privateKey);

            logger.LogInformation("Unpublished source of lambda {Lambda}", source?.PublicKey ?? meta.PublicKeyOf(privateKey));

            return McpProtocol.Say(new
            {
                ok = true,
                published = false,
                stars = source?.Stars,
                note = "Taken down: the page and the downloads are gone. Whatever somebody downloaded while it was published stays theirs under the license it came with."
            });
        }

        var license = Text(arguments, "license");
        var author = arguments.TryGetPropertyValue("author", out var named) && named != null ? named.ToString() : null;

        if (license == null && author == null)
        {
            source = sources.Get(privateKey);

            logger.LogInformation("Read source status of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }
        else
        {
            source = sources.Publish(privateKey, new SourceDraft(license, author));

            logger.LogInformation("Published source of lambda {Lambda} license {License}", source.PublicKey, source.License);
        }

        if (source is not { Published: true })
        {
            return McpProtocol.Say(new
            {
                ok = true,
                published = false,
                stars = source?.Stars,
                licenses = SourceLicenses.All.Select(l => new { l.Id, l.Name, kind = l.Kind.ToString() }),
                note = "Not published. Pass license (and author, if the user named one) to publish it - only if the user asked for that."
            });
        }

        var chosen = SourceLicenses.Find(source.License)!;

        return McpProtocol.Say(new
        {
            ok = true,
            published = true,
            license = chosen.Id,
            licenseName = chosen.Name,
            source.Author,
            holder = SourceLicenses.Holder(source.Author, source.PublicKey),
            source.Stars,
            url = $"{origin}/source/{source.PublicKey}",
            note = "Everything in every version is public now, older versions included - code, assets, documentation, tests and the one line each version says it changed. Never write keys, passwords or personal data into files: secrets and the database stay private."
        });
    }

    private JsonObject ListDemos(string origin)
    {
        logger.LogInformation("Listed demos");

        return Demos(origin);
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

    private JsonObject ReadGuide()
    {
        logger.LogInformation("Read platform guide");

        return Guide();
    }

    private JsonObject Guide() => McpProtocol.Say(new
    {
        ok = true,
        preferTheApi = "If you can make HTTP requests, the REST API at https://genhttp.dev/api/v1/openapi.json does the same as these tools and costs fewer tokens, because files are sent directly. GET /api/v1/lambdas/{privateKey}/versions/{version}/zip downloads a version; a feature is downloaded from and put back to /api/v1/lambdas/{privateKey}/features/{feature}/zip (GET, PUT) - so edit locally and push as often as it takes. POST /api/v1/lambdas/{privateKey}/versions/zip saves a zip as a new version. The zip holds the documentation and tests in .lambda/, a hidden folder: zip the contents with it ('zip -r ../feature.zip .', not '*'), or the version you save has none. Every endpoint that saves takes ?deploy=true. Many environments cannot reach it; then use these tools.",
        whatALambdaIs = "C# that returns a GenHTTP handler, served at /lambda/{publicKey}/. No Main and no project: the snippet is the program.",
        lifecycle = new
        {
            threeThings = "A lambda holds versions, features and data, and they live differently. Versions are the program as it was saved. Features are changes being worked on beside it. Data is what the program keeps. Getting this right is most of getting a lambda right.",
            versions = new
            {
                what = "A version is the program: every .cs file and every asset - index.html, scripts, styles, icons, the whole front end - and, beside it in .lambda/, what is written about it: its documentation and its tests (see documentationAndTests). They are saved, deployed and rolled back together, and nothing else is in a version.",
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
                kinds = "There are three kinds: the database, a SQLite database for records; the workspace, a private directory of files; and the secrets - API keys, passwords and tokens the code reads by name and nobody sees. read_lambda lists what a lambda has under data.",
                optIn = "Each kind is switched on before the lambda can use it. The workspace is on unless the owner switched it off; the database and the secrets are off until switched on. enable_data switches on what what you build needs - the database for records, secrets for an API key. Switching a kind off deletes what it held, so only the owner does that, in the editor; a kind the owner switched off is theirs to decide about - ask before switching it on again.",
                beReady = "Data can be empty: a new lambda has none, and an owner can clear it. Have the code create what it needs on first use, and say so plainly where something it expects is missing, rather than fail."
            },
            whereThingsGo = new
            {
                theProgram = "In the version, as assets: the app itself - C#, and the whole front end including a single page application's HTML, JavaScript, CSS, images and fonts. Ship it with write_code under a folder such as web/ and serve it with Assets.App(\"web\").",
                theRecords = "In the database: everything the lambda keeps as records while it runs - entries, accounts, sessions, orders, votes, scores. Its schema is SQL migrations shipped in the version, applied by Evolve as the lambda starts; the rows are data. Prefer it to JSON files: it takes one write at a time, so two requests cannot lose each other's change, and it finds a record without reading all of them.",
                theFiles = "In the workspace: files the lambda writes or users upload - pictures, documents - and large input files that are not program - a model, a dataset, media. Write them with Workspace from the code, or put one there with upload_file. What is known about a file - who uploaded it, when - is a record, in the database.",
                neverTheOtherWay = "Never keep user data in assets: they are read only while the lambda runs and replaced on every deploy. Never upload the front end to the workspace: it would not be versioned, a rollback would not bring the matching pages back, and a feature would work on a copy of it."
            },
            flows = new
            {
                newLambda = "Nothing is online and nobody uses it yet: create_lambda, write_code with deploy: true - the documentation and tests in .lambda/ with the code - and fix with change_code and deploy: true until it works. No feature needed.",
                changeALambda = "It exists and may be in use: read_lambda - its documentation first - then create_feature (or continue an open one), change it with feature until its preview does what was asked and its tests pass, bring .lambda/ up to date with what changed, then merge_feature with deploy: true - unless the user wants to look at the preview first, in which case leave the feature and give them its previewUrl."
            },
            theLambda = new
            {
                lifetime = $"A free lambda goes offline after {(int)options.DeploymentLifetime.TotalDays} days without visits or edits, and is removed - versions, features, data and all - about {(int)options.Retention.TotalDays} days after the last of either. A premium lambda stays online and is kept.",
                deleting = "Deleting a lambda deletes its versions, its features and its data together. Nothing else deletes versions one by one."
            }
        },
        documentationAndTests = new
        {
            what = "Beside its program, every version keeps what is written about it, in .lambda/: its documentation in .lambda/docs/ and how it is tested in .lambda/tests/. They are files of the version like any other - saved with write_code and change_code, compared in the history, rolled back, copied into a feature and merged with it, in the zip and in the export - but never compiled and never served, whatever they are called. So each version is described as it is: rolling back brings back the documentation that was true of it.",
            why = "The code says what the program does. It does not say what the user wants, what has to keep working, or why it is built this way - and the next agent to change the lambda, or you a week later, starts from nothing without that. The owner reads it too: the editor shows the documentation of every version, and product.md is the one page the owner of an app they had built reads.",
            product = ".lambda/docs/product.md - what the app is, in a sentence or two first (the editor shows that paragraph on the overview); who uses it and what for, as use cases; each feature and why it exists; and what it deliberately does not do. In the user's terms, from what they asked for and why - every request folded in, not a list of changes. No code and no file names: its reader is the owner.",
            decisions = ".lambda/docs/decisions.md - the technical decisions and why they were made: how the program is put together, how the data is stored and kept readable across versions, which secrets it reads and what for, what was considered and left out. One short entry per decision - what was decided, why, and what a change has to keep in mind. When a decision changes, change its entry: the versions keep the history.",
            testing = ".lambda/tests/README.md - how to test the app automatically: what has to keep working, one line per behaviour worth checking; how each is checked (the request, and the answer expected); what data a test needs; and how to run the scripts beside it against an address - a feature's previewUrl, and never the live lambda for a test that writes. Scripts, where the app is worth them, go beside it in .lambda/tests/ - a Node script that takes the address (node smoke.mjs <url>: Node has fetch and WebSocket built in, nothing to install), curl, or a .http file - and so does test data: files to put into a feature's copy of the workspace with upload_file, and records as a script that adds them through the app's own routes against the previewUrl (only the app writes to its database; demo-crud's seed.mjs does it).",
            more = "More pages go beside them in .lambda/docs/ (api.md, data.md) and are shown in the editor too; a picture a page links to - ![Flow](flow.svg) - is shown where it is linked.",
            howMuch = "In proportion to the lambda. A small one - a page, a form, a handful of routes - needs a short paragraph per page and one quick check (a curl line or a short script), not a test suite, extra pages or seeded test data. Write what the next change needs to know and what has to keep working, nothing it could read off the code. They grow with the app: more checks as there are more behaviours worth keeping, a page of its own when a topic needs one.",
            when = "Write all three with a new lambda, in the same write_code as the code. With every change, update what it affects in the same save - in a feature, so they are merged with the code and the version it becomes is described as it is. A lambda that has none gets them with its next change. read_lambda and every save say when one is missing.",
            use = "Read them before changing a lambda: read_lambda hands them over first. Before merging a feature, run what tests/README.md describes against its previewUrl, fix what fails, and add a check for what the change does when it is a behaviour worth keeping.",
            demos = "Every demo has all three and a test script - read_lambda demo-crud shows what they look like. A demo is there to teach; size yours to your lambda.",
            language = "Write them in the language the user writes in.",
            format = "Markdown. A relative link from one page to another works in the editor.",
            size = "They count towards what the assets of a version may come to."
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
            inCSharp = "Redirect.To(\"other\") and Location headers take relative paths too. Never build an absolute URL from the request's host and /lambda/.",
            exception = "og:image, og:url and a canonical link are full addresses: they name the page for whoever reads it elsewhere, and the page never follows them - see beingFound."
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
            files = "Assets.Files(\"media\") serves a folder as plain files, straight from the directory - the fastest way the server has to send a file. Use it for what is not the app's pages: pictures, downloads, media.",
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
            what = "The lambda's files as data: a private directory it reads and writes at runtime, for anything that must outlive a request or a deployment. The same for every version; a feature has a copy of its own.",
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
        database = new
        {
            what = "A SQLite database of the lambda's own, for its records. Data like the workspace - shared by every version, untouched by deploys, rollbacks and merges; a feature works on a copy of it, which its merge throws away.",
            switchedOn = "Off until switched on: enable_data with kind 'database', which makes it, empty. Switched on or off, the lambda starts again on its next request. While it is off, Database.GetConnection() throws.",
            surface = new[]
            {
                "Database.GetConnection() - an open Microsoft.Data.Sqlite SqliteConnection to the lambda's database; dispose of it when done, or hand it to a context that does",
                "class Records(SqliteConnection connection) : DbContext { ... OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection, contextOwnsConnection: true); } - a context of Entity Framework Core of your own, mapping the tables",
                "using var db = new Records(Database.GetConnection()); then db.Things.Where(...).ToList(), db.Things.Find(id), db.Things.Add(thing) and db.SaveChanges(), ExecuteUpdate(...) and ExecuteDelete(), db.Database.BeginTransaction() - all synchronous",
                "connection.CreateCommand(), command.Parameters.AddWithValue(\"$name\", value), ExecuteNonQuery() / ExecuteScalar() / ExecuteReader() - plain SQL, where a statement is easier written than LINQ",
                "new Evolve(connection) { Locations = [Assets.Root + \"migrations\"], IsEraseDisabled = true }.Migrate() - applies the migrations"
            },
            imported = "Microsoft.Data.Sqlite, Microsoft.EntityFrameworkCore and EvolveDb are imported in every file: SqliteConnection, SqliteException, DbContext, DbSet, ModelBuilder, EF and Evolve need no using. Of Entity Framework's namespaces below that, Microsoft.EntityFrameworkCore.Metadata.Builders, .ChangeTracking and .Storage.ValueConversion may be imported; the others - Infrastructure, Storage, Internal, Migrations and the rest - are refused.",
            entityFramework = new
            {
                context = "Write a DbContext that maps the tables the migrations make: ToTable(\"tasks\"), HasKey where the key is not called Id, HasColumnName where a column is named otherwise than its property beyond case - SQLite ignores case, so Title is the column title. The context makes nothing; the schema is Evolve's.",
                connection = "The context runs on the connection Database.GetConnection() opens, handed in from outside - inside a DbContext, Database is the context's own (the lambda's is LambdaEnvironment.Database there). UseSqlite with a connection string is refused: it would open a database of its own.",
                perRequest = "A context per request or per call, disposed of with using; contextOwnsConnection: true disposes of the connection with it. Never keep a context in a field shared between requests.",
                synchronous = "Synchronously, always: ToList, FirstOrDefault, Find, Count, Any, SaveChanges, ExecuteUpdate, ExecuteDelete. Never ToListAsync, FirstOrDefaultAsync, SaveChangesAsync or any other Async form of Entity Framework - SQLite answers synchronously whatever the method is called, so the Async ones only add cost to every request.",
                schema = "Never EnsureCreated, EnsureDeleted, Migrate or Entity Framework's migrations - they are refused. A new property is a migration adding its column first, then the mapping.",
                reading = "AsNoTracking() for what is only read; an entity loaded tracked, changed and saved for an edit; ExecuteUpdate and ExecuteDelete for a change to many rows, or to a count, in one statement without reading the rows first.",
                conflicts = "A key or unique column that is taken fails SaveChanges with a DbUpdateException whose InnerException is a SqliteException with SqliteErrorCode 19 - catch that to answer 409."
            },
            reach = "Database can be used from every file, like Workspace. A type of your own called Database hides it; the platform's is LambdaEnvironment.Database.",
            migrations = new
            {
                how = "The schema is SQL files shipped with the version as assets, in migrations/: V1__Create_tasks.sql, V2__Add_due_date.sql - a V, a number, two underscores, a name. Evolve applies each once, in order, and remembers which in a table of its own (changelog). Apply them as the lambda starts, at the top of lambda.cs, before it serves anything.",
                example = "using (var connection = Database.GetConnection())\n{\n    var evolve = new Evolve(connection, message => Console.WriteLine(message))\n    {\n        Locations = [Assets.Root + \"migrations\"],\n        IsEraseDisabled = true\n    };\n\n    evolve.Migrate();\n}",
                neverEdit = "Never change or rename a migration that was applied - Evolve refuses to start on a checksum that no longer matches, and a database that already ran it would never run the change. A change to the schema is the next file.",
                compatible = "Every version reads the same database, and a rollback runs an older version against a schema a newer one migrated. Keep changes additive where it costs little - new tables, new columns with a default, never dropping or renaming what an older version reads - rather than building machinery for compatibility nobody needs. An older version runs fine against a database with migrations it does not know.",
                features = "A feature migrates its own copy when its preview starts, so a migration is tried there first; merged and deployed, it runs against the lambda's database."
            },
            usage = new
            {
                connectionPerCall = "Open a connection - or a context on one - where it is used and dispose of it: using var db = new Records(Database.GetConnection()); inside a route, a method or a store class. Connections are pooled, so that costs nothing, and one is never shared between requests.",
                synchronous = "Use it synchronously - ToList and SaveChanges with Entity Framework, ExecuteReader and ExecuteNonQuery with plain SQL, never their Async forms. The database is a file on the same machine, and the server has an asynchronous model of its own that a blocking call here does not disturb.",
                parameters = "LINQ puts values in as parameters. Plain SQL does the same with $title, never by pasting a value into the text: what somebody typed must not change what a statement does.",
                concurrency = "SQLite takes one write at a time and lets readers read meanwhile, so a count raised in one statement - ExecuteUpdate(s => s.SetProperty(v => v.Votes, v => v.Votes + 1)) - counts every request, with no locks of your own. Where the row may not exist yet, look and add it inside db.Database.BeginTransaction(), which takes the one writer's place before it reads.",
                times = "SQLite has no type for times, and Entity Framework's format for them drops the zone: keep them as ISO 8601 text in UTC with a converter - Property(t => t.Created).HasConversion(v => v.ToUniversalTime().ToString(\"O\", CultureInfo.InvariantCulture), v => DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)), or for every DateTime in ConfigureConventions. They sort as text, compare in queries and read well.",
                topLevel = "At the top level of lambda.cs, write using (var connection = Database.GetConnection()) { ... } - a using declaration (using var) is not allowed there. Inside methods and routes, using var is fine."
            },
            notAllowed = "A lambda does not open connections of its own - new SqliteConnection(...), a connection string (UseSqlite(\"Data Source=...\") included), ATTACH and VACUUM INTO are refused - and Entity Framework does not make its schema: EnsureCreated, EnsureDeleted, Migrate and its migrations are refused. The database is the one file Database.GetConnection() opens, and its schema is Evolve's.",
            fromOutside = "read_database lists the tables and reads their rows - with feature, a feature's copy. The owner sees the same in the editor under Data. Nothing writes to it but the lambda.",
            exported = "The export carries the database as database/database.db; Database.GetConnection() opens it there with plain Microsoft.Data.Sqlite, and a project whose code has a DbContext references Entity Framework Core.",
            limits = new
            {
                bytes = options.DatabaseOf(LambdaTier.Free),
                premiumBytes = options.DatabaseOf(LambdaTier.Premium),
                full = "Past its room, a write fails with SQLite's 'database or disk is full'. read_database says how full it is."
            },
            demo = "Every demo keeps its records like this - read_lambda demo-crud: lambda.cs migrates, Store.cs maps the table with a DbContext and reads and writes it, migrations/ holds the schema."
        },
        secrets = new
        {
            what = "API keys, passwords, tokens, connection strings: whatever the code needs and must not contain. Data like the workspace - shared by every version, untouched by deploys, rollbacks and merges; a feature works on a copy.",
            surface = new[]
            {
                "Secret.Read(\"STRIPE_KEY\") - the value; throws, saying how to set it, when there is none of that name",
                "Secret.Exists(\"STRIPE_KEY\") - whether it is set; false while secrets are switched off"
            },
            reach = "Secret can be used from every file, like Workspace, and read at the top of lambda.cs or per request. A value changed by the owner is what the next call reads, without a deploy - read it where it is used rather than once into a static, if a change should take effect without one.",
            switchedOn = "Secrets are off until switched on: enable_data with kind 'secrets'. While they are off Secret.Read throws and Secret.Exists answers false.",
            whoSetsThem = "Best: the user, in the editor under Data > Secrets - the value then never passes through you. Write the code with Secret.Read(\"NAME\"), switch secrets on, and tell the user which names to set and where to get the values. read_lambda and list_secrets list the names the code reads that have no value yet (missing), and so does the editor, which offers to set them. If the user hands you a value, store it with set_secret.",
            neverShown = "A value is never shown again - not in the editor, the API or here. Only the lambda reads it. So a secret is replaced, not edited.",
            neverInTheOpen = "Never write a secret into code, an asset, the workspace, a log line, a specification or a change note, and never return one from a route or print it. Never echo a value the user gave you.",
            names = $"Letters, digits and underscores, not starting with a digit; conventionally upper case: STRIPE_KEY, SMTP_PASSWORD. Case sensitive. Up to {SecretVault.MaxSecrets} per lambda, {SecretVault.MaxValue / 1024} KB each.",
            features = "A feature gets a copy of the lambda's secrets, and its preview reads the copy; set_secret with feature changes only the copy - for a sandbox key, say. Merging discards the copy.",
            exported = "In the exported project, Secret.Read reads the environment variable of the same name. The values are not exported.",
            example = "var stripe = new System.Net.Http.HttpClient();\nstripe.DefaultRequestHeaders.Authorization = new(\"Bearer\", Secret.Read(\"STRIPE_KEY\"));",
            demo = "demo-registration peppers its password hashes with a secret when it has one - read_lambda demo-registration, Accounts.cs."
        },
        servingAFrontEnd = new
        {
            rule = "The front end is part of the program, so it is part of the version: ship index.html, scripts, styles and images as assets and serve them with Assets.App(\"web\"). It is then deployed, rolled back and merged together with the API it talks to.",
            how = "write_code with the files under a folder (web/index.html, web/app.js, web/app.css), return Layout.Create().Add(\"api\", api).Add(Assets.App(\"web\")), deploy. Change it like any other file.",
            example = "Every demo serves its front end like this, from web/ - read_lambda demo-crud.",
            notFromTheWorkspace = "Do not upload the app's own pages to the workspace with upload_file. They would not be versioned: a rollback would keep the new pages over the old API, and a feature would work on a copy of them that its merge throws away. The workspace is for data.",
            underTheHood = "App() is SinglePageApplication.From(tree).ServerSideRouting() over Assets.Tree()."
        },
        beingFound = new
        {
            when = "For a page meant to be found or shared - a website, a landing page, a shop, a portfolio, an event: anything the user wants people to come across in a search engine, in an AI agent's answer, or as a link in a chat. A tool for a few people, a page behind a login or an admin page needs a title and nothing more.",
            title = "<title>: what the page is, then whose - 'Menu and opening hours - Café Lindner' - in about 60 characters, one for each page. It is the line a search engine shows and the tab is named after.",
            description = "<meta name=\"description\" content=\"...\">: one or two sentences, up to about 155 characters, saying what a visitor finds there in the words they would search for. Not a list of keywords. Both in the language of the page, with <html lang> set to it.",
            icon = "A favicon as an asset, linked relatively: <link rel=\"icon\" href=\"icon.svg\" type=\"image/svg+xml\"> - an SVG is text, written with the code. Without the link a browser asks the root of the host, which below /lambda/{publicKey}/ is this platform's icon. It shows in the tab and in bookmarks, and in search results on a domain of its own - search engines show one icon per host.",
            socialPreview = "What a chat or a social network shows for a shared link: og:title, og:description, og:type (website) and og:image as <meta property=\"...\" content=\"...\">, and <meta name=\"twitter:card\" content=\"summary_large_image\">. The picture is a PNG or JPEG of 1200 by 630 pixels, shipped as an asset - none of them shows an SVG. Without a picture, leave og:image out and make the card summary: the title and the description still make one.",
            fullAddress = "og:image takes a full address - social networks do not resolve a relative one: the lambda's domainUrl if it has one, its publicUrl otherwise (read_lambda), followed by the asset's path. Change it when the address does. It, og:url and the canonical link are the only full addresses in a page, and a feature's deploy does not warn about them: the page never follows them.",
            canonical = "A lambda with a domain of its own also answers at /lambda/{publicKey}/. Give each page <link rel=\"canonical\"> and og:url with its address on the domain, so search engines list the domain.",
            withoutScripts = "Crawlers and AI agents mostly read the HTML as it is served and run no JavaScript. Put what the page is about - the title, the description, a heading and the opening text - in the HTML, not only in what a script renders later; a page whose content comes from an API still says in its HTML what it is.",
            howMuch = "In proportion: the title, the description, the language and the icon are a few lines on every public page; the preview picture where the app is meant to be shared, the canonical link where it has a domain. Written for people, not for a search engine - no keyword lists, no hidden text.",
            keptOut = "A feature's preview is kept out of search engines by the platform. A lambda whose owner wants it kept out says so with <meta name=\"robots\" content=\"noindex\">."
        },
        generatedContent = new
        {
            tree = "VirtualTree.Create().Add(\"app.css\", Resource.FromString(css).Type(new ContentType(\"text/css\"))) builds a tree in memory.",
            singlePage = "Content.From(Resource.FromString(html).Type(new ContentType(\"text/html; charset=utf-8\")))"
        },
        takingItAway = "GET /api/v1/lambdas/{privateKey}/export returns the newest version as a standalone zipped .NET 10 project with a Dockerfile: Program.cs hosts it, Project.cs is lambda.cs, the other files keep their code, the documentation and tests go to docs/ and tests/, and Platform/ stands in for Workspace, Assets, Secret, Database and the implicit imports. The database comes along as a SQLite file. It needs nothing from this platform. Worth telling the user.",
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
            andTheDocumentation = "The specification says what one version was asked for; .lambda/docs/product.md is the whole app as the user wants it, every specification folded in. Pass the one with every save and keep the other up to date.",
            limits = new { specification = VersionNote.MaxSpecification, change = VersionNote.MaxChange }
        },
        showcase = new
        {
            what = "The owner can list a lambda on the public showcase page with a title, a short description and a picture. The showcase tool does it.",
            when = "Only when the user asks. It is not part of building or deploying.",
            tone = ShowcaseLimits.Tone
        },
        openSource = new
        {
            what = "The owner can publish a lambda's source code at /source/{publicKey} under a license: anybody reads every version's code, front end, documentation and tests there, sees what each version changed, stars it, and downloads any version as the .NET project the export makes - with a LICENSE, and without the data. The open_source tool does it.",
            when = "Only when the user asks. Ask which license if they did not say: MIT unless they want another.",
            neverPublished = "The data - the database, the workspace and the values of the secrets - what the owner asked for in their words (the specification), and anything about who uses it: traffic, logs, visitors.",
            thenPublic = "Once published, every version is public, the ones saved before included. Keys, passwords and personal data never go into files anyway; for a published lambda it matters at once.",
            licenses = SourceLicenses.All.Select(l => new { l.Id, l.Name, kind = l.Kind.ToString() }),
            startingFromOne = "A published source downloads as a .NET project, not as a lambda. To make a lambda of it: Project.cs holds the snippet as the body of Build() with the types after the class - put the body back into lambda.cs with those types below it; the other .cs files stay as they are; assets/ is what goes at the root of the version; docs/ and tests/ go into .lambda/docs/ and .lambda/tests/; Platform/, Program.cs, the .csproj and the Dockerfile are the platform's and stay behind. Keep to the terms of its LICENSE."
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
            what = "Reflection, dynamic, processes, the environment, the file system, and anything else that reaches the host.",
            why = "All lambdas share one process."
        },
        waiting = new
        {
            rule = "Never wait for a task: .Result, .Wait(), .GetAwaiter().GetResult(), Task.WaitAll, Task.WaitAny and SemaphoreSlim.Wait() are refused.",
            why = "Requests run on one thread per core, and a task finishes on the thread that would be waiting for it - so it never would, and every request on that thread would wait with it.",
            instead = "Await it: handlers and routes may be async and return Task<T> or ValueTask<T> - Inline.Create().Get(async () => await ...). Take a semaphore with await semaphore.WaitAsync(), or use a lock statement for a short section without any await in it.",
            synchronous = "Where there is a synchronous method, call it - the database is used synchronously, Entity Framework included (ToList and SaveChanges, not ToListAsync and SaveChangesAsync). What is short and local (a query against the lambda's database, a small file in the workspace) costs less done in place than awaited."
        },
        thingsThatCatchPeopleOut = new[]
        {
            "A new version for every attempt at changing a lambda that is online. Work in a feature, try it at its preview address, and merge it once.",
            "An API key or password in the code, where every version, export and reader of the history keeps it. Read it with Secret.Read(\"NAME\") and have the user set it under Data > Secrets.",
            "A change that leaves .lambda/docs/ as it was. The next agent reads the documentation first and works from what it says - so out of date, it is worse than none.",
            "Records in a JSON file in the workspace, rewritten whole on every change. Keep them in the database: enable_data with kind 'database', a migration, a DbContext on Database.GetConnection().",
            "Entity Framework's Async methods - ToListAsync, FirstOrDefaultAsync, SaveChangesAsync. Call ToList, FirstOrDefault and SaveChanges: SQLite answers synchronously whatever the method is called.",
            "Database.GetConnection() inside a DbContext does not compile: there, Database is the context's own. Hand the connection in - new Records(Database.GetConnection()) - and configure it with options.UseSqlite(connection, contextOwnsConnection: true).",
            "A table made by Entity Framework - EnsureCreated, Migrate, its migrations. They are refused: the schema is SQL migrations applied by Evolve, and the context only maps it.",
            "using var connection = Database.GetConnection(); at the top level of lambda.cs does not compile there. Write using (var connection = ...) { ... }, or open it inside the method that needs it.",
            "Editing a migration that was applied. Evolve refuses to start on the changed checksum; add the next file instead.",
            "Request bodies bind by type: a bare string parameter is null. Take a record.",
            "Once a route has read the body, the request's headers are gone. Check a header (a token, say) in a concern in front of the route - the Authentication module does exactly that, see demo-registration - or in a route that takes no body.",
            "In other .cs files, Assets is the Files module's type of that name: use LambdaEnvironment.Assets there. Workspace works in every file.",
            "A browser cannot set headers on a websocket handshake. Pass what the socket needs in the query (connection.Request.Header.Query) or, for secrets, as the first frame.",
            "Concurrent writes to one socket corrupt it. Guard broadcasts with a semaphore - taken with await semaphore.WaitAsync(), never Wait().",
            "Waiting for a task with .Result, .Wait() or .GetAwaiter().GetResult() is refused: it would hang the thread the task has to finish on. Await it - see waiting.",
            "A relative og:image: the shared link shows no picture. It takes the full address - see beingFound.",
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
            features = $"Up to {options.MaxFeatures} features open at once; each holds a copy of the files, the workspace and the database, within the same limits.",
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
             + $"Workspace: {Size(workspace.Quota)} in all, in any number of files. "
             + $"Database: {Size(options.DatabaseOf(tier))}.";
    }

    private static string Size(long bytes) => bytes switch
    {
        >= 1L << 30 when bytes % (1L << 30) == 0 => $"{bytes >> 30} GB",
        >= 1L << 20 when bytes % (1L << 20) == 0 => $"{bytes >> 20} MB",
        _ => $"{bytes >> 10:N0} KB"
    };

    #endregion

    #region Arguments

    /// <summary>
    /// What the view of an editor is, wherever a tool takes one.
    /// </summary>
    private const string ViewField =
        "How the editor opens: 'Full' (the default) shows every section, 'Simple' only the app, how it is doing and a box to ask for a change. Simple suits an owner who is not going to read the code - use it when the user asks for a simple editor, or says they are not a developer.";

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
