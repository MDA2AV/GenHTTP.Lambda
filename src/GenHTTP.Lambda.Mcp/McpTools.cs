using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Diagnostics;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;
using GenHTTP.Lambda.Services.Telemetry;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Mcp;

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
/// tested - in docs/ and tests/ of its code, and an agent writes it with a
/// new lambda and updates it with every change. read_lambda hands it over
/// before the files, and every save that leaves a page out says so.
///
/// The tools that save, deploy, read and reach into the data take an optional
/// feature, and then act on the feature rather than on the lambda - so the
/// same few tools do both, and nothing is learned twice.
///
/// Every tool answers with an object rather than prose. A model reads the text
/// and a program reads the structured copy, and both are the same thing - the
/// MCP module writes both. A refusal of the platform is said as text, marked
/// as an error, like the module's own refusal of arguments that do not fit.
/// The one exception is platform_guide, a document read from beginning to end
/// and sent once, as Markdown (see <see cref="PlatformGuide"/>).
///
/// Every tool that did something logs what it did, as the API does - reads
/// included, since what an agent looks at is how it goes about its work. See
/// <see cref="OperationLog"/> for what is left out.
/// </remarks>
public sealed class McpTools(IMetaService meta, IWorkspaceService workspace, IDataService data, IFeatureService features, ISecretService secrets,
                              IDatabaseService databases, IShowcaseService showcases, ISourceService sources, IAnalyticsService analytics,
                              IConnectivityService connectivity,
                              ITelemetryService telemetry, ILogBook book,
                              LambdaOptions options, ILimitsService tiers, ILambdaAddresses addresses, IBrandingService branding, ILogger<McpTools> logger)
{

    private readonly PlatformGuide _guide = new(options, tiers, addresses);

    #region Tools

    /*
     * The tools as an agent sees them, in the order it reads them: the name,
     * what the tool does, what it takes, and what calling it does to the
     * platform. The parameters are the arguments and their schema, and an
     * attribute on one - a range, a length, the values it takes - is
     * advertised and checked before the tool runs. The work is done below.
     *
     * Every tool states all four hints. Clients read them to decide what to
     * ask the user before a call - a read can go through, a deploy is worth a
     * question - and without them every tool looks like the worst case, which
     * is what the defaults assume. Four effects are told apart: a read looks
     * and changes nothing; creating adds something and replaces nothing;
     * saving adds a version or changes a feature, and with deploy: true
     * replaces what is online; replacing replaces or removes what was there,
     * the same way however often. None of them reaches beyond this platform.
     */

    [Tool("create_lambda", Title = "Create a lambda", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Summary("Create a lambda. Returns its public address and a private editor key, the only way back in. It starts with version 1 - an empty starter, or a copy of a demo. Nothing is online until deploy.")]
    public object CreateLambda(IRequest request,
                               [Summary("Must be true where the installation has terms - on a free shared machine, say, that deployments may be removed and nothing malicious is run (platform_guide, terms): the user accepts them. Ignored where it has none.")] bool acceptTerms,
                               [Summary("Requested address: lower case letters, digits and dashes. Generated if omitted.")] string? publicKey = null,
                               [Summary("Left out, the lambda starts empty. The id of a demo from list_demos starts it as a copy of that demo, which is yours to change.")] string? template = null,
                               [Summary(ViewField), EnumDataType(typeof(EditorView))] EditorView? view = null)
        => Refusing(() => Create(acceptTerms, publicKey, template, view, RequestOrigin.Of(request)));

    [Tool("update_lambda", Title = "Change a lambda's settings", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Change how a lambda's editor opens: view 'Simple' shows the app, how it is doing and a box to ask for a change - for an owner who does not write code - and 'Full' every section, the code, files, data, versions and logs included. Only the default: whoever opens the editor can switch for themselves, and that choice stays theirs. Only do this when the user asks for it.")]
    public object UpdateLambda(IRequest request,
                               [Summary(PrivateKey)] string privateKey,
                               [Summary(ViewField), EnumDataType(typeof(EditorView))] EditorView view)
        => Refusing(() => Update(privateKey, view, RequestOrigin.Of(request)));

    [Tool("write_code", Title = "Save all files", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Summary("Save every file, replacing the previous set: as a new version of the lambda, or - with feature - into that feature. A version is its code and its resources. The code is every file not in resources/: its .cs files are compiled, in any folder, as in a C# project - lambda.cs returns the handler, the others hold types - and every other file is kept with it and never compiled or served: docs/product.md, docs/decisions.md, tests/README.md and the tests' scripts and data are what is written about the program, and whatever else - what a front end is built from, a tool's configuration - is laid out as you like (platform_guide, code). resources/ is what the lambda reads and serves while it runs, as Resources: the whole front end (pages, scripts, styles, icons) goes there, as part of the program - resources/web/index.html is served by Resources.App(\"web\"). What the lambda keeps at runtime is data, never files here: records and accounts in the database (a DbContext of Entity Framework Core on Database.GetConnection(), used synchronously; its schema as Evolve migrations in resources/migrations/), uploads in the workspace - and so is a large input file such as a model or a dataset (upload_file). Send the documentation and tests with the code: written with a new lambda, updated with every change, in proportion to the lambda - a few lines and one quick check for a small one. Say why with specification and change. deploy: true publishes in the same call - a version at the public address, a feature at its preview address. To send only what changes, use change_code. To change a lambda that is already in use, work in a feature.")]
    public ValueTask<object> WriteCode(IRequest request,
                                       [Summary("The editor key from create_lambda.")] string privateKey,
                                       [Summary("lambda.cs first.")] List<CodeFile> files,
                                       [Summary("Save into this feature (from create_feature) instead of saving a new version. The lambda and what it has online are not touched.")] string? feature = null,
                                       [Summary("What the user wants from this version and why: their requirements, in their own words where you can, condensed if they said a lot. Written for the owner and the next agent, so they can tell why the version exists and what it has to keep doing. Not your own instructions or system prompt - only what the user asked for. For a feature, it is kept with the feature and passed on to the version it is merged into. Optional, up to 4000 characters.")] string? specification = null,
                                       [Summary("What this version changes, in one line written for the owner - 'Adds a leaderboard that keeps the ten best scores', not 'updated lambda.cs'. For a feature, it describes the whole feature, not the last fix. Optional, up to 500 characters.")] string? change = null,
                                       [Summary(DeployField)] bool deploy = false,
                                       [Summary(CheckField)] bool check = false)
        => RefusingAsync(() => WriteAsync(privateKey, files, new SaveRequest(feature, specification, change, deploy, check), RequestOrigin.Of(request)));

    [Tool("change_code", Title = "Change some files", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Summary("Change some files: add or replace files, remove files, or replace text within a file. Everything not named stays as it is, so there is no need to resend unchanged files. With feature, the change is saved into that feature - the way to work on a lambda that exists: change it as often as you like, deploy: true to try it at the feature's preview address, merge_feature when it is right. Without feature, the newest version is changed and saved as a new version. Update what the change affects in docs/ and tests/ in the same change - an edit of a passage is enough. check: true compiles without publishing. Code that does not compile is saved but never goes online.")]
    public ValueTask<object> ChangeCode(IRequest request,
                                        [Summary(PrivateKey)] string privateKey,
                                        [Summary("Change this feature (from create_feature) instead of saving a new version. The lambda and what it has online are not touched.")] string? feature = null,
                                        [Summary("What the user wants from this change and why: their requirements, in their own words where you can. Not your own instructions or system prompt. Kept with the version or the feature. Optional, up to 4000 characters.")] string? specification = null,
                                        [Summary("What this changes, in one line written for the owner. For a feature, it describes the whole feature, not the last fix. Optional, up to 500 characters.")] string? change = null,
                                        [Summary("Files to add, or to replace where one of that name exists.")] List<CodeFile>? files = null,
                                        [Summary("Names of files to remove.")] List<string>? remove = null,
                                        [Summary("Text replacements, applied after files and remove.")] List<CodeEdit>? edits = null,
                                        [Summary(DeployField)] bool deploy = false,
                                        [Summary(CheckField)] bool check = false)
        => RefusingAsync(() => ChangeAsync(privateKey, files, remove, edits, new SaveRequest(feature, specification, change, deploy, check), RequestOrigin.Of(request)));

    [Tool("create_feature", Title = "Start a feature", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Summary("Start a feature: a place to change a lambda without touching what it has online. It branches off a version - the newest unless base names another - with a copy of its files (its documentation and tests included) and a copy of the lambda's data, and gets an address of its own to try it at. Change it with change_code or write_code and feature (deploy: true puts it online at its preview address), test it there, and merge_feature once it does what was asked. The lambda goes on serving its visitors from the version online the whole time.")]
    public ValueTask<object> CreateFeature(IRequest request,
                                           [Summary(PrivateKey)] string privateKey,
                                           [Summary("What the feature is, in a few words for the owner - 'Leaderboard', 'Dark mode'."), MaxLength(FeatureService.MaxName)] string name,
                                           [Summary("What the user wants from it and why, in their words where you can. Kept with the feature and passed on to the version it is merged into. Optional, up to 4000 characters.")] string? specification = null,
                                           [Summary("The version to branch off. Defaults to the newest - leave it out unless the user asked to start from an older one.")] int? @base = null)
        => RefusingAsync(() => CreateFeatureAsync(privateKey, name, specification, @base, RequestOrigin.Of(request)));

    [Tool("update_feature", Title = "Change a feature's name, notes or base", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Rename a feature, change what it says about itself, or move its base. merge_feature refuses a feature that is not based on the newest version, since merging it would undo what was saved after it branched off: bring the newer versions' changes into the feature first, then move base to the newest version here. Nothing checks that the changes really are in - that is up to you.")]
    public object UpdateFeature(IRequest request,
                                [Summary(PrivateKey)] string privateKey,
                                [Summary("The feature, from create_feature or read_lambda.")] string feature,
                                [Summary("A new name."), MaxLength(FeatureService.MaxName)] string? name = null,
                                [Summary("What the user wants from it and why.")] string? specification = null,
                                [Summary("What it changes, in one line - what the version it is merged into will say.")] string? change = null,
                                [Summary("The version the feature is now based on, once that version's changes are in its files.")] int? @base = null)
        => Refusing(() => ChangeFeature(privateKey, feature, new FeatureUpdate(name, specification, change, @base), RequestOrigin.Of(request)));

    [Tool("merge_feature", Title = "Merge a feature into the lambda", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Summary("Make a feature's files the next version of the lambda, and delete the feature with its preview and its copy of the data - the lambda's own data is not touched. Its docs/ and tests/ become the version's documentation and tests, so bring them up to date in the feature first, and run its tests against the preview. Refused while the feature is not based on the newest version (update_feature says how to get it there), and while its code does not compile. deploy: true puts the new version online at once; otherwise deploy it when the user wants it live.")]
    public ValueTask<object> MergeFeature([Summary(PrivateKey)] string privateKey,
                                          [Summary("The feature.")] string feature,
                                          [Summary("What the new version keeps as what the user wanted. Left out, the feature's.")] string? specification = null,
                                          [Summary("What the new version says it changes. Left out, the feature's.")] string? change = null,
                                          [Summary("Also put the new version online.")] bool deploy = false)
        => RefusingAsync(() => MergeFeatureAsync(privateKey, feature, specification, change, deploy));

    [Tool("delete_feature", Title = "Delete a feature", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Throw a feature away: its files, its preview and its copy of the data. The lambda is not touched. For a feature the user does not want after all.")]
    public object DeleteFeature([Summary(PrivateKey)] string privateKey,
                                [Summary("The feature.")] string feature)
        => Refusing(() => RemoveFeature(privateKey, feature));

    [Tool("check_code", Title = "Check that code compiles", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("Compile without saving or deploying; returns diagnostics with file and line. Does not build the handler, so deploy can still refuse a route whose return type cannot be served.")]
    public ValueTask<object> CheckCode([Summary(PrivateKey)] string privateKey,
                                       [Summary("lambda.cs first. The .cs files of the code are compiled, in any folder; nothing else is - send those, the rest may be left out.")] List<CodeFile> files)
        => RefusingAsync(() => CheckAsync(privateKey, files));

    [Tool("deploy", Title = "Deploy a version or a feature", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Deploy (publish) a saved version so it goes live at its public address - the newest by default. With feature, deploy that feature's files to its own preview address instead, against its copy of the data, leaving the lambda alone. Returns diagnostics on failure, and whatever was online stays online.")]
    public ValueTask<object> Deploy(IRequest request,
                                    [Summary(PrivateKey)] string privateKey,
                                    [Summary("Defaults to the newest.")] int? version = null,
                                    [Summary("Deploy this feature to its preview address instead of a version to the lambda's.")] string? feature = null)
        => RefusingAsync(() => feature != null
                                   ? DeployFeatureAsync(privateKey, feature, RequestOrigin.Of(request))
                                   : DeployVersionAsync(privateKey, version, RequestOrigin.Of(request)));

    [Tool("read_lambda", Title = "Read a lambda", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("A lambda's status (online version, newest version, expiry, its tier and what it may use there), its open features, the recent versions with what each was asked for and changed, its data (the database, the workspace and the secrets: whether each is on, what it holds, and which secrets the code reads that have no value yet), and one version - or, with feature, that feature: its documentation (docs/: what the app is and why, the technical decisions), how it is tested (tests/), and its files. The documentation comes first and in full up to 20,000 characters; the files come in full while they fit into 30,000 characters - the C# first, then the resources, then the rest of the code - and by name and length after that, with file to read one - a test script or its data too. Read the documentation and the history before changing what you did not write. Also how a demo is read: pass its key from list_demos.")]
    public object ReadLambda(IRequest request,
                             [Summary(PrivateKeyOrDemo)] string privateKey,
                             [Summary("Defaults to the newest.")] int? version = null,
                             [Summary("Read this feature - its files, its base, and which newer versions it would have to take in before it can be merged - instead of a version.")] string? feature = null,
                             [Summary("Return only this file, in full however large the rest is - up to 1,048,576 characters, beyond which the zip has it. Any file of the version, 'tests/smoke.mjs' and 'resources/web/index.html' included.")] string? file = null)
        => Refusing(() => Read(privateKey, version, feature, file, RequestOrigin.Of(request)));

    [Tool("read_logs", Title = "Read a lambda's logs", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("What a deployed lambda has been doing: its recent requests and how they were answered, what it printed, the errors it threw with their stack traces, and how much traffic it has had in the last hour and day. With feature, what that feature's preview has been doing instead, kept apart from the lambda's visitors. Call it after deploying to see that it works, and first when something is reported broken.")]
    public object ReadLogs([Summary(PrivateKeyOrDemo)] string privateKey,
                           [Summary("Read what this feature's preview did instead.")] string? feature = null,
                           [Summary("The lowest level worth reading: 'info' for everything, 'warn' for problems, 'error' for failures."), AllowedValues("debug", "info", "warn", "warning", "error")] string level = "info",
                           [Summary("The cursor a previous call answered with, to read only what is new since then.")] long since = 0,
                           [Summary("At most this many lines, the newest."), Range(1, 1000)] int limit = 100)
        => Refusing(() => Logs(privateKey, feature, level, since, limit));

    [Tool("upload_file", Title = "Put a file into the lambda's data", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Write a file to the lambda's workspace - its data, which every version shares and no deploy, rollback or merge touches. With feature, to that feature's copy of the data instead, for trying things out without touching the real one. For files the lambda works with at runtime (pictures people will browse) and large input files that are not program: a model, a dataset, media. Records belong in the database, where the code writes them. Takes effect at once, without a deploy. Not for the front end: pages, scripts and styles are the program and belong in the version's resources (write_code).")]
    public ValueTask<object> UploadFile([Summary(PrivateKey)] string privateKey,
                                        [Summary("Relative to the workspace; slashes make folders, e.g. 'models/model.onnx'.")] string path,
                                        [Summary("Text, or base64 with encoding set.")] string content,
                                        [Summary("Write to this feature's copy of the data instead.")] string? feature = null,
                                        [Summary("'base64' for binary; omit otherwise.")] string? encoding = null)
        => RefusingAsync(() => UploadAsync(privateKey, path, content, feature, encoding));

    [Tool("list_files", Title = "List the lambda's data", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("The lambda's data: whether its workspace is switched on, and every file in it with size and last write - the same whichever version is online. With feature, that feature's copy of it. The code and resources of a version are in read_lambda.")]
    public object ListFiles([Summary(PrivateKeyOrDemo)] string privateKey,
                            [Summary("List this feature's copy of the data instead.")] string? feature = null)
        => Refusing(() => Files(privateKey, feature));

    [Tool("delete_file", Title = "Delete a file from the lambda's data", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Remove a file, or a folder with its contents, from the workspace - the lambda's data, shared by every version. No deploy or rollback brings it back. With feature, from that feature's copy instead.")]
    public object DeleteFile([Summary(PrivateKey)] string privateKey,
                             [Summary("Relative to the workspace.")] string path,
                             [Summary("Delete from this feature's copy of the data instead.")] string? feature = null)
        => Refusing(() => Remove(privateKey, path, feature));

    [Tool("enable_data", Title = "Switch on a kind of data", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Switch on a kind of data the lambda needs and does not have: 'database' (a SQLite database for its records - off until switched on, which makes it, empty), 'secrets' (off until switched on) or 'workspace' (on unless the owner switched it off). Do it when what you build needs it - records need the database, an API key for a service it calls needs secrets. Takes effect at once, without a deploy; switching the database on starts the lambda again on its next request, so its migrations run. There is no tool to switch one off: that deletes what it held, and is the owner's to do in the editor.")]
    public object EnableData([Summary(PrivateKey)] string privateKey,
                             [Summary("Which kind."), AllowedValues(DataKinds.DatabaseId, DataKinds.SecretsId, DataKinds.WorkspaceId)] string kind)
        => Refusing(() => SwitchOn(privateKey, kind));

    [Tool("request_connectivity", Title = "Ask to connect to other servers", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Ask the operator of the installation to let a lambda's code connect to other servers - call an API with HttpClient, open a socket, a WebSocket or mail client - or read where that stands. Without it, code that connects does not compile (LAMBDA0003); a lambda whose tier connects by default (production, usually) may anyway, and read_lambda says connectivity.allowed. Ask only where the server has to make the call - a key that must not reach the browser, an API browsers may not call - and fetch a public API from the lambda's page otherwise. Say what it connects to and why. The operator decides, which takes a while: tell the user it was asked, and build what does not connect meanwhile. withdraw: true takes the request back, or gives up what was allowed - only when the user asks. With only privateKey it returns where it stands.")]
    public object RequestConnectivity([Summary(PrivateKey)] string privateKey,
                                [Summary("What it connects to and why, for the operator who decides: the addresses or services and what the lambda does with them - 'api.stripe.com, to take payments for the shop', not 'HTTP calls'. Needed to ask; left out, it returns where it stands."), MaxLength(ConnectivityRule.MaxPurpose)] string? purpose = null,
                                [Summary("Take the request back, or give up what was allowed - code that connects then no longer compiles. Only when the user asks.")] bool withdraw = false)
        => Refusing(() => Connectivity(privateKey, purpose, withdraw));

    [Tool("read_database", Title = "Read the lambda's database", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("The lambda's database - its records, shared by every version: whether it is switched on, how full it is, and its tables and views with their columns and how many rows each holds. With table, a page of that table's rows, newest first, up to 100 at a time. Read only: the lambda writes it, through Database.GetConnection(). Use it to see that migrations were applied and records were written. With feature, that feature's copy.")]
    public ValueTask<object> ReadDatabase([Summary(PrivateKeyOrDemo)] string privateKey,
                                          [Summary("Read this feature's copy of the database instead.")] string? feature = null,
                                          [Summary("A table or view to read the rows of; left out, the tables are listed.")] string? table = null,
                                          [Summary("Rows to skip, for the next page."), Range(0, int.MaxValue)] int offset = 0,
                                          [Summary("Rows to read."), Range(1, MaxDatabaseRows)] int limit = 20,
                                          [Summary("A column to sort by; left out, the order the rows were written in.")] string? order = null,
                                          [Summary("Oldest or smallest first, rather than newest or largest.")] bool ascending = false)
        => RefusingAsync(() => DatabaseAsync(privateKey, feature, table, offset, limit, order, ascending));

    [Tool("set_secret", Title = "Store a secret", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Store an API key, password or token under a name the code reads with Secret.Read(\"NAME\") - never put one in the code, a resource, the workspace, a log line or a note. The value is sealed and can never be read back, by you or the owner; only the lambda reads it, from its next call on, without a deploy. Only set a value the user gave you for this: when you do not have it, write the code with Secret.Read and tell the user to set it under Secrets in the editor (Keys and passwords in its simple view) - read_lambda and list_secrets list what is still missing. Secrets must be switched on first (enable_data). With feature, into that feature's copy (a sandbox key to try it with). Up to 100 secrets of 32 KB each.")]
    public object SetSecret([Summary(PrivateKey)] string privateKey,
                            [Summary("Letters, digits and underscores, not starting with a digit - conventionally upper case: 'STRIPE_KEY'. The name of an environment variable in an exported project.")] string name,
                            [Summary("The value, as the user gave it.")] string value,
                            [Summary("Store it in this feature's copy of the secrets instead.")] string? feature = null)
        => Refusing(() => StoreSecret(privateKey, name, value, feature));

    [Tool("list_secrets", Title = "List a lambda's secrets", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("The lambda's secrets by name - never their values: whether secrets are switched on, what is stored and when, which names the code reads with Secret.Read and have no value yet (missing) - what the user still has to set - and which it only uses when they are there, checked with Secret.Exists (optional). With feature, that feature's copy.")]
    public object ListSecrets([Summary(PrivateKeyOrDemo)] string privateKey,
                              [Summary("List this feature's copy instead.")] string? feature = null)
        => Refusing(() => Secrets(privateKey, feature));

    [Tool("delete_secret", Title = "Delete a secret", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Remove a secret. Code that reads it with Secret.Read fails from its next call on. Only when the user asks for it, or a secret you stored yourself is no longer used. With feature, from that feature's copy instead.")]
    public object DeleteSecret([Summary(PrivateKey)] string privateKey,
                               [Summary("The name, as stored - names are case sensitive.")] string name,
                               [Summary("Delete from this feature's copy instead.")] string? feature = null)
        => Refusing(() => RemoveSecret(privateKey, name, feature));

    [Tool("showcase", Title = "Showcase a lambda", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("List a lambda on the public showcase page, change its entry, or take it off. Not part of building: only do this when the user asks for it. With only privateKey it returns the current entry. An entry needs a title, a description and a picture (a screenshot or short GIF of the lambda in use); it is listed while the lambda is online. " + ShowcaseLimits.Tone)]
    public object Showcase(IRequest request,
                           [Summary(PrivateKey)] string privateKey,
                           [Summary("What it is, in a few words - 'Pub quiz scoreboard', not 'The ultimate quiz experience'."), MaxLength(ShowcaseLimits.MaxTitle)] string? title = null,
                           [Summary("One to three plain sentences on what a visitor can do with it."), MaxLength(ShowcaseLimits.MaxDescription)] string? description = null,
                           [Summary("The picture, base64: PNG, JPEG, GIF or WebP, up to the size platform_guide names under showcase. Needed for a new entry; left out, the current one is kept.")] string? image = null,
                           [Summary("Take the lambda off the showcase instead.")] bool remove = false)
        => Refusing(() => ChangeShowcase(privateKey, title, description, image, remove, RequestOrigin.Of(request)));

    [Tool("open_source", Title = "Publish a lambda's source code", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("Publish a lambda's source code at /source/{publicKey} for anybody to read, star and download as a .NET project, change the license it is published under, or take it down. What is published is the program and what is written about it - every version's code, resources, documentation and tests, and the history of what each changed - never its data: not the database, not the workspace, not a secret's value. Not part of building: only do this when the user asks for it, and ask which license if they did not say (MIT unless they want another). With only privateKey it returns whether it is published. Once published, everything in every version is public, older ones included: keep keys, passwords and personal data out of the files - in secrets and the database.")]
    public object OpenSource(IRequest request,
                             [Summary(PrivateKey)] string privateKey,
                             [Summary(LicenseField)] string? license = null,
                             [Summary("Who holds the copyright, as the license names them - the user's name or organization, only if they gave it. Left out, it stays as it is; empty, the license names 'the authors of' the lambda."), MaxLength(SourceLicenses.MaxAuthor)] string? author = null,
                             [Summary("Take the source down instead. Its stars are kept for when it is published again.")] bool remove = false)
        => Refusing(() => ChangeSource(privateKey, license, author, remove, RequestOrigin.Of(request)));

    [Tool("analytics", Title = "Read who visits a lambda", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Summary("How people reach a lambda, as its owner sees it in the editor: visitors, pages opened and requests per day, and the pages, referring sites, countries, devices, browsers, operating systems and crawlers (search engines, AIs, link previews) behind them. Counted by day in UTC and written down every few minutes; a year is kept, of which the lambda's tier decides how much is shown (platform_guide, analytics). Off until switched on - nothing is counted before. enable: true switches it on, only when the user asks; switching off deletes what was counted and is the owner's to do in the editor. Use it to answer what the user asks about their visitors, or to see which pages matter before changing them.")]
    public object Analytics([Summary(PrivateKey)] string privateKey,
                            [Summary("How many days, today included. Left out, as many as the lambda's tier shows, which is also the most.")] int? days = null,
                            [Summary("Switch analytics on, counting from the next request - only when the user asked for it.")] bool enable = false)
        => Refusing(() => ReadAnalytics(privateKey, days, enable));

    [Tool("list_demos", Title = "List the demos", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("Demos this platform keeps online, each a finished lambda showing one way to build something: a REST API over records, registration and login, a websocket game, uploads, live updates. Their keys are public and read only: read the closest one with read_lambda (and list_files, read_logs) before writing similar code. create_lambda with a demo's id as template starts from a copy.")]
    public object ListDemos()
    {
        logger.LogInformation("Listed demos");

        return Demos();
    }

    [Tool("platform_guide", Title = "Read the platform guide", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Summary("How this platform works, read first: one Markdown document in sections the other tools point to as 'platform_guide, <section>' - how versions, features and data live, git and the REST API, code and resources, paths, compiling, requests, the database, the workspace, secrets, connectivity, live updates, documentation and tests, pages meant to be found, the link back, limits and terms.")]
    public string ReadGuide(IRequest request)
    {
        logger.LogInformation("Read platform guide");

        return _guide.Write(RequestOrigin.Of(request), branding.Current.Name);
    }

    #endregion

    #region Answers

    /// <summary>
    /// Runs a tool, and answers a refusal of the platform as what it is.
    /// </summary>
    /// <remarks>
    /// The platform refusing something - a key nobody has, a demo that cannot
    /// be changed, a name that is taken - is an answer, not a fault: the model
    /// should read why and try something else rather than see a tool that
    /// failed unexpectedly, which is what the MCP module makes of every
    /// exception but its own <see cref="ProviderException"/>, logged as an
    /// error. Anything else is left to it: that is a fault, and logged as one.
    /// </remarks>
    private static object Refusing(Func<object> tool)
    {
        try
        {
            return tool();
        }
        catch (LambdaException e)
        {
            return ToolResult.Error(e.Message);
        }
    }

    /// <inheritdoc cref="Refusing"/>
    private static async ValueTask<object> RefusingAsync(Func<ValueTask<object>> tool)
    {
        try
        {
            return await tool();
        }
        catch (LambdaException e)
        {
            return ToolResult.Error(e.Message);
        }
    }

    /// <summary>
    /// An answer that says the tool did not do what it was asked, with what
    /// the agent needs to know to try again: the diagnostics, what is online.
    /// </summary>
    /// <remarks>
    /// Read as text by the model and as data by anything that would rather
    /// parse it, like every other answer.
    /// </remarks>
    private static ToolResult Failed(object answer) => new(null, answer, isError: true);

    /// <summary>
    /// The same answer with one more thing said.
    /// </summary>
    /// <remarks>
    /// For what is worth saying however the call went on - a save that left
    /// something out - so the several answers it may end in need not each
    /// carry it. Written as the module writes every answer: camel case,
    /// without what is null, enumerations by their names.
    /// </remarks>
    /// <param name="note">Nothing to leave the answer as it is</param>
    private static object Adding(object said, string name, string? note)
    {
        if (note == null)
        {
            return said;
        }

        if (said is ToolResult { Structured: { } data } result)
        {
            return new ToolResult(result.Content, Adding(data, name, note), result.IsError);
        }

        if (said is ToolResult)
        {
            return said;
        }

        var answer = JsonSerializer.SerializeToNode(said, AnswerFormat)!.AsObject();

        answer[name] = note;

        return answer;
    }

    private static readonly JsonSerializerOptions AnswerFormat = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    #endregion

    #region Code

    private object Create(bool acceptTerms, string? publicKey, string? template, EditorView? view, string origin)
    {
        if (options.Terms != null && !acceptTerms)
        {
            return ToolResult.Error("acceptTerms must be true. Show the user the terms from platform_guide first.");
        }

        var lambda = meta.Create(publicKey, template, view ?? EditorView.Full);

        logger.LogInformation("Created lambda {Lambda} template {Template} view {View}", lambda.PublicKey,
                              template ?? "(none)", lambda.View);

        return new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            privateKey = lambda.PrivateKey,
            publicUrl = lambda.PublicUrl,
            editorUrl = $"{origin}{PlatformRoutes.EditorOf(lambda.PrivateKey)}",
            gitUrl = $"{origin}{PlatformRoutes.RepositoryOf(lambda.PrivateKey, lambda.PublicKey)}",
            view = lambda.View,
            next = "write_code with deploy: true, and fix what needs fixing with change_code and deploy: true - a new lambda needs no feature. Send docs/product.md, docs/decisions.md and tests/README.md with the code - short for a small app (platform_guide, documentationAndTests). Once people use it, make further changes in a feature (create_feature).",
            warning = "The editor key cannot be recovered. Give it to the user."
        };
    }

    private object Update(string privateKey, EditorView view, string origin)
    {
        var lambda = meta.ChangeView(privateKey, view);

        logger.LogInformation("Set view of lambda {Lambda} to {View}", lambda.PublicKey, lambda.View);

        return new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            editorUrl = $"{origin}{PlatformRoutes.EditorOf(lambda.PrivateKey)}",
            view = lambda.View,
            note = "The editor opens in this view for everybody who has not switched it for themselves; whoever has keeps their own."
        };
    }

    private async ValueTask<object> WriteAsync(string privateKey, List<CodeFile> sent, SaveRequest save, string origin)
    {
        var files = Files(sent);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return ToolResult.Error(invalid);
        }

        // what it held before the save replaces it
        var had = save.Feature is { } feature
            ? LambdaSource.Parse(features.Get(privateKey, feature).Code)
            : meta.Require(privateKey).LatestVersion != null ? meta.LatestFiles(privateKey) : [];

        var saved = Adding(await SaveAsync(privateKey, files, save, origin), "dropped", Dropped(had, files));

        return Adding(saved, "misplaced", Misplaced(files));
    }

    /// <summary>
    /// Said when a save of every file leaves out folders the files before it
    /// had - which an agent that did not know they were there does without
    /// noticing: what a front end is built from, a tool's files - and which
    /// no page asks for the way a missing page of the documentation is asked
    /// for. Not said of single files, which a save of every file leaves out
    /// on purpose more often than not.
    /// </summary>
    private static string? Dropped(IReadOnlyList<LambdaFile> had, IReadOnlyList<LambdaFile> after)
    {
        static string? Folder(LambdaFile file) => file.Name.Contains('/') ? file.Name[..file.Name.IndexOf('/')] + "/" : null;

        var kept = after.Select(Folder).OfType<string>().ToHashSet(StringComparer.Ordinal);

        var gone = had.Select(Folder).OfType<string>().Distinct(StringComparer.Ordinal).Where(f => !kept.Contains(f)).Order(StringComparer.Ordinal).ToList();

        if (gone.Count == 0)
        {
            return null;
        }

        var files = had.Count(f => Folder(f) is { } folder && gone.Contains(folder));

        return $"What this replaced had {string.Join(", ", gone)} - {files} files - and this save has none of {(gone.Count == 1 ? "it" : "them")}, since write_code replaces every file. "
             + "If that was not meant, save them again, or make changes with change_code, which keeps every file it is not told about.";
    }

    private async ValueTask<object> ChangeAsync(string privateKey, List<CodeFile>? sent, List<string>? remove, List<CodeEdit>? replacements, SaveRequest save,
                                                string origin)
    {
        var changed = sent != null ? Files(sent) : [];

        var edits = replacements?.Select(e => new FileEdit(e.File, e.Find, e.Replace)).ToList();

        // a feature is changed from what it holds - and only if that is still
        // what it holds when the change is saved; a new version from the newest
        IReadOnlyList<LambdaFile> current;

        int? read = null;

        if (save.Feature is { } feature)
        {
            var content = features.Get(privateKey, feature);

            current = LambdaSource.Parse(content.Code);

            read = content.Feature.Revision;
        }
        else
        {
            current = meta.LatestFiles(privateKey);
        }

        var files = LambdaChanges.Apply(current, changed, remove, edits);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return ToolResult.Error(invalid);
        }

        return Adding(await SaveAsync(privateKey, files, save, origin, read), "misplaced", Misplaced(files));
    }

    /// <summary>
    /// Said when a save keeps migrations at the top of the code, where a
    /// lambda kept them before its resources were a folder of their own - an
    /// agent used to that writes them there without noticing, and as code they
    /// are kept and never read: Evolve finds no migration, and makes no table.
    /// </summary>
    private static string? Misplaced(IReadOnlyList<LambdaFile> files)
    {
        var stray = files.Any(f => f.Name.StartsWith("migrations/", StringComparison.Ordinal) && f.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));

        return stray
            ? "migrations/ at the top is a folder of the code, kept with the version and never read. Evolve applies the migrations in resources/migrations/ (Resources.Root + \"migrations\"): move them there."
            : null;
    }

    /// <summary>
    /// Stores the files - as a new version, or into the feature named - and
    /// deploys or checks them if the arguments ask for that.
    /// </summary>
    private async ValueTask<object> SaveAsync(string privateKey, IReadOnlyList<LambdaFile> files, SaveRequest save, string origin, int? read = null)
    {
        var note = new VersionNote(save.Specification, save.Change, VersionOrigins.Agent);

        var code = LambdaSource.Serialize(files);

        // said where the next save is decided, and as a request rather than a
        // refusal - like the note below: the code matters more
        var documentation = Unwritten(files);

        if (save.Feature is { } feature)
        {
            return await SaveFeatureAsync(save, privateKey, feature, code, files.Count, note, origin, read, documentation);
        }

        var version = meta.Save(privateKey, code, note);

        logger.LogInformation("Saved lambda {Lambda} version {Version} files {Files}", meta.PublicKeyOf(privateKey), version.Version, files.Count);

        // said only when it is missing, and as a request rather than a
        // refusal: the code matters more than the note about it
        var reminder = version.Change == null
            ? "Pass change (one line on what the version does) and specification (what the user wants, and why) next time; the owner reads them in the version history."
            : null;

        if (!save.Deploy)
        {
            if (!save.Check)
            {
                return new
                {
                    ok = true,
                    version = version.Version,
                    next = "deploy",
                    note = reminder,
                    documentation
                };
            }

            /*
             * Compiled here rather than left to check_code, which wants every
             * file sent again: an agent that changed one line with change_code
             * would have to read the whole lambda back to find out whether the
             * line compiles. The version is saved either way.
             */
            var outcome = await meta.CheckAsync(privateKey, code);

            return new
            {
                ok = true,
                version = version.Version,
                compiles = outcome.Success,
                diagnostics = Diagnostics(outcome.Diagnostics),
                next = outcome.Success ? "deploy, when it should go online" : "fix the diagnostics with change_code",
                note = reminder,
                documentation
            };
        }

        return await DeployVersionAsync(privateKey, version.Version, origin, reminder, documentation);
    }

    /// <summary>
    /// Stores the files as the feature's, and puts its preview online or
    /// compiles it if the arguments ask for that.
    /// </summary>
    private async ValueTask<object> SaveFeatureAsync(SaveRequest save, string privateKey, string feature, string code, int files, VersionNote note,
                                                     string origin, int? read, string? documentation)
    {
        var saved = features.Save(privateKey, feature, code, note, read);

        logger.LogInformation("Saved feature '{Feature}' of lambda {Lambda} files {Files}", saved.Name, meta.PublicKeyOf(privateKey), files);

        if (save.Deploy)
        {
            return await DeployFeatureAsync(privateKey, feature, origin);
        }

        if (save.Check)
        {
            var outcome = await meta.CheckAsync(privateKey, code);

            return new
            {
                ok = true,
                feature = Brief(saved, origin),
                compiles = outcome.Success,
                diagnostics = Diagnostics(outcome.Diagnostics),
                next = outcome.Success
                    ? $"deploy with feature: '{saved.Key}' to try it at its preview address"
                    : $"fix the diagnostics with change_code and feature: '{saved.Key}'",
                documentation
            };
        }

        return new
        {
            ok = true,
            feature = Brief(saved, origin),
            next = $"Saved into the feature; nothing is online yet. Try it with deploy and feature: '{saved.Key}' (or deploy: true on the next save), then merge_feature when it does what was asked.",
            documentation
        };
    }

    private async ValueTask<object> CheckAsync(string privateKey, List<CodeFile> sent)
    {
        var files = Files(sent);

        if (LambdaSource.Validate(files) is { } invalid)
        {
            return ToolResult.Error(invalid);
        }

        var outcome = await meta.CheckAsync(privateKey, LambdaSource.Serialize(files));

        logger.LogInformation("Checked code of lambda {Lambda} files {Files} compiles {Compiles}", meta.PublicKeyOf(privateKey), files.Count, outcome.Success);

        return new
        {
            ok = outcome.Success,
            compiles = outcome.Success,
            diagnostics = Diagnostics(outcome.Diagnostics)
        };
    }

    #endregion

    #region Features

    private async ValueTask<object> CreateFeatureAsync(string privateKey, string name, string? specification, int? @base, string origin)
    {
        var created = await features.CreateAsync(privateKey, new FeatureDraft(name, specification, @base, VersionOrigins.Agent));

        logger.LogInformation("Created feature '{Feature}' of lambda {Lambda} base {Version}", created.Name, meta.PublicKeyOf(privateKey), created.Base);

        return new
        {
            ok = true,
            feature = Brief(created, origin),
            next = $"Change it with change_code (or write_code) and feature: '{created.Key}' - it holds the files of version {created.Base} and a copy of the lambda's data. deploy: true on a save puts it online at previewUrl, never at the lambda's address; read_logs with feature shows how it answers. merge_feature when it does what was asked."
        };
    }

    private object ChangeFeature(string privateKey, string feature, FeatureUpdate update, string origin)
    {
        var updated = features.Update(privateKey, feature, update);

        logger.LogInformation("Updated feature '{Feature}' of lambda {Lambda} base {Version}", updated.Name, meta.PublicKeyOf(privateKey), updated.Base);

        return new
        {
            ok = true,
            feature = Brief(updated, origin),
            next = updated.Mergeable
                ? $"It is based on the newest version, so merge_feature can merge it once it does what was asked."
                : $"It is based on version {updated.Base}, the newest is {updated.Newest}: bring the newer versions' changes in and move base to {updated.Newest} before merging."
        };
    }

    private async ValueTask<object> MergeFeatureAsync(string privateKey, string feature, string? specification, string? change, bool deploy)
    {
        var note = new VersionNote(specification, change, VersionOrigins.Agent);

        var merged = await features.MergeAsync(privateKey, feature, note, deploy);

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
            return Failed(new
            {
                ok = false,
                problem = "Not merged: the feature's code does not compile. Nothing changed - fix it with change_code and feature, then merge again.",
                diagnostics = Diagnostics(merged.Diagnostics)
            });
        }

        var version = merged.Version!;

        if (merged.Deployment is not { } deployment)
        {
            return new
            {
                ok = true,
                merged = true,
                version = version.Version,
                next = $"The feature is now version {version.Version}, and gone - its preview and its copy of the data with it; the lambda's own data is as it was. deploy puts version {version.Version} online when the user wants it live."
            };
        }

        if (!deployment.Success)
        {
            var current = meta.Get(privateKey);

            return Failed(new
            {
                ok = false,
                merged = true,
                problem = $"Merged as version {version.Version}, but it did not go online: building its handler failed. Whatever was online before still is.",
                version = version.Version,
                stillOnline = current?.ActiveVersion,
                diagnostics = Diagnostics(deployment.Diagnostics)
            });
        }

        var lambda = deployment.Lambda!;

        return new
        {
            ok = true,
            merged = true,
            publicKey = lambda.PublicKey,
            publicUrl = lambda.PublicUrl,
            domainUrl = DomainUrl(lambda),
            version = lambda.ActiveVersion,
            online = true,
            onlineUntil = lambda.DeployedUntil,
            next = "Merged and online. Call the public address, then read_logs to see how it answers."
        };
    }

    private object RemoveFeature(string privateKey, string feature)
    {
        var name = features.Delete(privateKey, feature);

        logger.LogInformation("Deleted feature '{Feature}' of lambda {Lambda}", name, meta.PublicKeyOf(privateKey));

        return new { ok = true, deleted = feature.Trim().ToLowerInvariant() };
    }

    /// <summary>
    /// The versions saved after a feature's base, as a sentence begins with them.
    /// </summary>
    private static string Later(FeatureInfo feature)
        => feature.Newest - feature.Base == 1 ? $"version {feature.Newest} was" : $"versions {feature.Base + 1} to {feature.Newest} were";

    /// <summary>
    /// Said when a feature's code links to the lambda by its full address -
    /// or by the path it had below <c>/lambda/</c>, which leads there - and
    /// from the preview that is the live lambda, with its real data.
    /// </summary>
    /// <remarks>
    /// The program only - the C# and the resources: documentation that names
    /// the lambda's address is saying where it is, not linking there from a
    /// page, and what a front end is built from says it again in what it
    /// builds, if anywhere. The same goes for meta tags and a canonical link -
    /// og:image needs the full address, since social networks do not resolve
    /// a relative one, and the page itself never follows it.
    /// </remarks>
    private string? Leaks(IReadOnlyList<LambdaFile> files, string publicKey)
    {
        var address = addresses.Of(publicKey);

        // the host, so http, https, ws and wss are all caught
        var host = $"//{publicKey}.{addresses.Authority}";

        return files.Any(f => (f.IsCompiled || f.IsResource) && LinksTo(f.Code, publicKey, host) && LinksTo(Named.Replace(f.Code, string.Empty), publicKey, host))
            ? $"The code links to the lambda by its full address, {address}. From the preview that is the live lambda, with its real data - not the feature's copy. Use relative paths (\"api/items\", not \"{address}api/items\")."
            : null;
    }

    private static bool LinksTo(string code, string publicKey, string host)
        => code.Contains(host, StringComparison.OrdinalIgnoreCase)
        || code.Contains($"{PlatformRoutes.LambdaBase}{publicKey}/", StringComparison.Ordinal)
        || code.Contains($"{PlatformRoutes.LambdaBase}{publicKey}\"", StringComparison.Ordinal);

    /// <summary>
    /// Where a page names an address rather than linking to it: its meta tags
    /// and its canonical link.
    /// </summary>
    private static readonly Regex Named = new(@"<meta\b[^>]*>|<link\b[^>]*\brel\s*=\s*[""']?canonical\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// What is said when a version or a feature lacks the pages of its
    /// documentation and tests, where the next save is decided - and nothing
    /// when it has them.
    /// </summary>
    private static string? Unwritten(IReadOnlyList<LambdaFile> files)
    {
        var missing = ContextPages.Missing(files);

        if (missing.Count == 0)
        {
            return null;
        }

        return missing.Count == LambdaSource.ExpectedPages.Count
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
        feature.Branch,
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
    private async ValueTask<object> UploadAsync(string privateKey, string path, string content, string? named, string? encoding)
    {
        var (id, feature) = WorkspaceOf(privateKey, named, true);

        WorkspaceEntry written;

        if (encoding == Base64)
        {
            written = await workspace.WriteEncodedAsync(id, path, content, feature);
        }
        else
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            written = await workspace.WriteAsync(id, path, stream, featureId: feature);
        }

        if (named != null)
        {
            logger.LogInformation("Wrote workspace file {Path} of feature '{Feature}' of lambda {Lambda} size {Size}", written.Path,
                                  features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey), written.Size);
        }
        else
        {
            logger.LogInformation("Wrote workspace file {Path} of lambda {Lambda} size {Size}", written.Path, meta.PublicKeyOf(privateKey), written.Size);
        }

        return new
        {
            ok = true,
            written.Path,
            written.Size,
            note = feature == null
                ? "In the lambda's data: there at once, no deploy needed, and shared by every version."
                : "In the feature's copy of the data: its preview sees it at once; the lambda's own data is not touched."
        };
    }

    private object Files(string privateKey, string? named)
    {
        var (id, feature) = WorkspaceOf(privateKey, named, false);

        var listing = workspace.List(id, feature);

        if (named != null)
        {
            logger.LogInformation("Listed workspace of feature '{Feature}' of lambda {Lambda}", features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Listed workspace of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

        return new
        {
            enabled = listing.Enabled,
            files = listing.Files.Select(f => new { f.Path, f.Size, f.Modified }),
            listing.Folders,
            listing.UsedBytes,
            listing.QuotaBytes,
            note = listing.Enabled
                ? null
                : "The workspace is switched off: it holds nothing, and code that uses Workspace fails. The owner switched it off, so ask the user before switching it on again with enable_data."
        };
    }

    private object SwitchOn(string privateKey, string kind)
    {
        var store = data.Enable(privateKey, kind);

        logger.LogInformation("Enabled {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return new
        {
            ok = true,
            kind = store.Kind,
            enabled = store.Enabled,
            next = store.Kind switch
            {
                DataKinds.SecretsId => "Secrets are on, and empty. Store a value the user gave you with set_secret, or tell the user to set it under Secrets in the editor (Keys and passwords in its simple view). The code reads it with Secret.Read(\"NAME\").",
                DataKinds.DatabaseId => "The database is on, and empty. Ship its schema as SQL migrations in resources/migrations/ (V1__Create_items.sql) and apply them with Evolve at the top of lambda.cs - see platform_guide under database, or demo-crud. The code keeps its records with a DbContext of Entity Framework Core on the connection Database.GetConnection() opens, synchronously; read_database shows what it holds.",
                _ => "On from the lambda's next request, without a deploy."
            }
        };
    }

    /// <summary>
    /// Asks for connectivity, takes the request back, or says where it stands.
    /// </summary>
    /// <remarks>
    /// The next step is said for each answer, since the decision is somebody
    /// else's and takes a while: the agent is to tell its user and carry on,
    /// not to wait or ask again.
    /// </remarks>
    private object Connectivity(string privateKey, string? purpose, bool withdraw)
    {
        ConnectivityInfo info;

        if (withdraw)
        {
            info = connectivity.Withdraw(privateKey);

            logger.LogInformation("Withdrew connectivity of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }
        else if (purpose != null)
        {
            info = connectivity.Request(privateKey, purpose);

            logger.LogInformation("Requested connectivity for lambda {Lambda} status {Status}", meta.PublicKeyOf(privateKey),
                                  info.Status?.ToString() ?? (info.Included ? "Included" : "None"));
        }
        else
        {
            info = connectivity.Get(privateKey);

            logger.LogInformation("Read connectivity of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

        return new
        {
            ok = true,
            connectivity = Describe(info),
            next = info switch
            {
                { Included: true } => "Its tier includes it: the code may connect to other servers. Give every request a timeout - the lambda runs in the shared server process.",
                { Allowed: true } => "The operator allowed it: code that connects compiles from now on - deploy what was refused before. Give every request a timeout.",
                { Status: ConnectivityStatus.Requested } => "Asked. The operator of the installation decides, which may take a while, and nothing tells you when: tell the user it waits for the operator, who reads what you said it is for. Meanwhile build what does not connect, or make the calls a browser may make from the page; once read_lambda says connectivity.allowed, deploy the code that connects.",
                { Status: ConnectivityStatus.Declined } => "The operator declined it" + (info.Note != null ? $": \"{info.Note}\"" : ".") + " Tell the user. Make the calls a browser may make from the page instead; ask again only if the user wants to, with what the operator would need to know.",
                _ => "Nothing is asked for: code that connects to other servers does not compile. request_connectivity with purpose asks for it."
            }
        };
    }

    /// <summary>
    /// Whether a lambda may connect, as the tools say it.
    /// </summary>
    private static object Describe(ConnectivityInfo? info)
    {
        var known = info ?? ConnectivityInfo.None;

        return new { known.Allowed, known.Included, status = known.Status?.ToString(), known.Changed, known.Purpose, known.Note };
    }

    /// <summary>
    /// Stores a secret the user handed the agent.
    /// </summary>
    /// <remarks>
    /// The answer repeats the name and never the value, so the value is in the
    /// conversation once - where the user put it - rather than twice.
    /// </remarks>
    private object StoreSecret(string privateKey, string name, string value, string? feature)
    {
        var stored = secrets.Set(privateKey, name, value, feature);

        // the name only - the value is not repeated anywhere, here least of all
        if (feature != null)
        {
            logger.LogInformation("Set secret {Name} of feature '{Feature}' of lambda {Lambda}", stored.Name, features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Set secret {Name} of lambda {Lambda}", stored.Name, meta.PublicKeyOf(privateKey));
        }

        return new
        {
            ok = true,
            name = stored.Name,
            usedByCode = stored.Used,
            note = (feature == null
                       ? "Stored in the lambda's secrets: its code reads it from its next call on, without a deploy. "
                       : "Stored in the feature's copy: its preview reads it; the lambda's own secrets are not touched. ")
                 + (stored.Used ? "" : $"No code reads it yet - read it with Secret.Read(\"{stored.Name}\"). ")
                 + "Do not repeat the value anywhere - not in code, notes, logs or your answer."
        };
    }

    private object Secrets(string privateKey, string? named)
    {
        var listing = secrets.List(privateKey, named);

        if (named != null)
        {
            logger.LogInformation("Listed secrets of feature '{Feature}' of lambda {Lambda}", features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Listed secrets of lambda {Lambda}", meta.PublicKeyOf(privateKey));
        }

        return new
        {
            enabled = listing.Enabled,
            secrets = listing.Secrets.Select(s => new { s.Name, s.Changed, usedByCode = s.Used }),
            listing.Missing,
            optional = listing.Optional.Count > 0 ? listing.Optional : null,
            limit = listing.Limit,
            note = !listing.Enabled
                ? "Secrets are switched off. enable_data with kind 'secrets' switches them on when what you build needs one."
                : listing.Missing.Count > 0
                    ? $"The code reads {string.Join(", ", listing.Missing)}, which have no value: Secret.Read throws until they are set. Ask the user to set them under Secrets in the editor (Keys and passwords in its simple view), or store one they gave you with set_secret."
                    : null
        };
    }

    /// <summary>
    /// The tables of a lambda's database, or a page of the rows of one.
    /// </summary>
    /// <remarks>
    /// Rows go out as objects keyed by column, which is what a model reads
    /// best; the editor gets arrays, which is what a grid draws best.
    /// </remarks>
    private async ValueTask<object> DatabaseAsync(string privateKey, string? feature, string? table, int offset, int limit, string? order, bool ascending)
    {
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

            return new
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
                        ? "The database is empty: no migration has made a table yet. Ship them in resources/migrations/ and apply them with Evolve as the lambda starts."
                        : null
            };
        }

        var rows = await databases.ReadAsync(privateKey, table, offset, limit, order, !ascending, feature);

        return new
        {
            rows.Table,
            rows.Total,
            rows.Offset,
            rows = rows.Rows.Select(r => rows.Columns.Select((c, i) => (c.Name, Value: r[i])).ToDictionary(p => p.Name, p => p.Value)),
            next = rows.Offset + rows.Rows.Count < rows.Total ? rows.Offset + rows.Rows.Count : (int?)null
        };
    }

    /// <summary>
    /// The most rows read_database answers with at once.
    /// </summary>
    private const int MaxDatabaseRows = 100;

    private object RemoveSecret(string privateKey, string name, string? feature)
    {
        secrets.Delete(privateKey, name, feature);

        if (feature != null)
        {
            logger.LogInformation("Deleted secret {Name} of feature '{Feature}' of lambda {Lambda}", name.Trim(), features.NameOf(privateKey, feature), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Deleted secret {Name} of lambda {Lambda}", name.Trim(), meta.PublicKeyOf(privateKey));
        }

        return new { ok = true, deleted = name.Trim() };
    }

    private object Remove(string privateKey, string path, string? named)
    {
        var (id, feature) = WorkspaceOf(privateKey, named, true);

        workspace.Delete(id, path, feature);

        if (named != null)
        {
            logger.LogInformation("Deleted workspace path {Path} of feature '{Feature}' of lambda {Lambda}", path, features.NameOf(privateKey, named), meta.PublicKeyOf(privateKey));
        }
        else
        {
            logger.LogInformation("Deleted workspace path {Path} of lambda {Lambda}", path, meta.PublicKeyOf(privateKey));
        }

        return new { ok = true, path };
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

    private async ValueTask<object> DeployVersionAsync(string privateKey, int? version, string origin, string? reminder = null, string? documentation = null)
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

            return Failed(new
            {
                ok = false,
                problem = "Not deployed. The diagnostics say whether compiling or building the handler failed. Whatever was online before still is.",
                version = version ?? current?.LatestVersion,
                stillOnline = current?.ActiveVersion,
                diagnostics = Diagnostics(result.Diagnostics),
                next = "Fix the diagnostics with change_code and deploy: true. If people already use the lambda, fix it in a feature (create_feature) instead, so every attempt is tried at its own address rather than saved as a version."
            });
        }

        var lambda = result.Lambda!;

        /*
         * online says it went online, for anything that reads the answer:
         * onlineUntil is no such signal, since a tier that keeps its lambdas
         * online has no deadline, and the field is then left out. The build
         * agent's runner read it as one, and told the owner of a lambda in
         * production that a change it had just put online had not gone online.
         */
        return new
        {
            ok = true,
            publicKey = lambda.PublicKey,
            publicUrl = lambda.PublicUrl,
            domainUrl = DomainUrl(lambda),
            version = lambda.ActiveVersion,
            online = true,
            onlineUntil = lambda.DeployedUntil,
            next = "Call the public address, then read_logs to see how it answered. While you are still building it, fix with change_code and deploy: true; once people use it, make changes in a feature (create_feature), which is tried at an address of its own and merged into the next version once it is right.",
            note = reminder ?? (lambda.DeployedUntil != null ? "Deploying again extends onlineUntil." : null),
            documentation
        };
    }

    private async ValueTask<object> DeployFeatureAsync(string privateKey, string feature, string origin)
    {
        var result = await features.DeployAsync(privateKey, feature);

        logger.Previewed(result, meta.PublicKeyOf(privateKey));

        if (!result.Success)
        {
            return Failed(new
            {
                ok = false,
                problem = "The preview was not deployed. The diagnostics say whether compiling or building the handler failed. Whatever the preview served before, it still does; the lambda was not touched.",
                feature = Brief(result.Feature, origin),
                diagnostics = Diagnostics(result.Diagnostics),
                next = $"Fix the diagnostics with change_code and feature: '{result.Feature.Key}', deploy: true."
            });
        }

        // links that leave the preview for the live lambda, said while there is still time
        var lambda = meta.Get(privateKey);

        var files = LambdaSource.Parse((features.Get(privateKey, feature)).Code);

        var warning = lambda != null ? Leaks(files, lambda.PublicKey) : null;

        return new
        {
            ok = true,
            feature = Brief(result.Feature, origin),
            previewUrl = $"{origin}{result.Feature.Path}",
            warning,
            next = result.Feature.Mergeable
                ? $"Call previewUrl, and read_logs with feature: '{result.Feature.Key}' to see how it answered. It works on its own copy of the data. Run what tests/README.md describes against previewUrl, and bring docs/ and tests/ up to date with what the feature changes. When it does what was asked: merge_feature (deploy: true to put it live)."
                : $"Call previewUrl, and read_logs with feature to see how it answered. Before merging: {Later(result.Feature)} saved after this feature's base, version {result.Feature.Base} - bring those changes in and move the base to {result.Feature.Newest} with update_feature.",
            documentation = Unwritten(files)
        };
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
    private object Logs(string privateKey, string? feature, string level, long since, int limit)
    {
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

        var (said, next, missed) = book.Read(since, null, Level(level), limit, lambdaId: id, feature: lines);

        var shown = said.Select(l => new { l.At, l.Level, l.Source, l.Text, detail = l.Detail, repeats = l.Repeats > 1 ? l.Repeats : (int?)null });

        var note = said.Count == 0
            ? feature == null
                ? "Nothing yet. Requests appear here once somebody calls the lambda - call its public address and read again."
                : "Nothing yet. Requests appear here once somebody calls the feature's preview - call its previewUrl and read again."
            : "Requests are the source 'Requests'; what the code printed is 'stdout' and 'stderr'; a handler that threw appears with its stack trace in detail. Pass cursor as since to read only what is new.";

        if (feature != null)
        {
            // a preview is not counted, so there is no traffic to report
            return new
            {
                ok = true,
                feature,
                lines = shown,
                cursor = next,
                missed,
                capturingOutput = options.CaptureLambdaOutput,
                note
            };
        }

        var traffic = telemetry.DescribeLambda(id);

        return new
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
        };
    }

    /// <summary>
    /// The lowest level read_logs reads, by one of the names its schema allows -
    /// warning and debug among them, since agents send those as well.
    /// </summary>
    private static LogLevel Level(string level) => level switch
    {
        "debug" => LogLevel.Debug,
        "warn" or "warning" => LogLevel.Warning,
        "error" => LogLevel.Error,
        _ => LogLevel.Information
    };

    private object Read(string privateKey, int? asked, string? wanted, string? only, string origin)
    {
        var lambda = meta.Get(privateKey)
                  ?? throw LambdaException.NotFound("There is no lambda with that editor key.");

        var history = meta.GetVersions(privateKey);

        var open = features.List(privateKey);

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
            version = asked ?? lambda.LatestVersion;

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

        IEnumerable<object> listing;

        string? note = null;

        if (only != null)
        {
            var one = files.FirstOrDefault(f => f.Name == only)
                   ?? throw LambdaException.NotFound($"There is no file '{only}' here. read_lambda without file lists what there is.");

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
        else
        {
            (listing, note) = Listing(files, archive);
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

        return new
        {
            ok = true,
            lambda.PublicKey,
            publicUrl = lambda.PublicUrl,
            domainUrl = DomainUrl(lambda),
            // where it is cloned with git, if git is at hand: AGENTS.md in it says how to work there
            gitUrl = $"{origin}{PlatformRoutes.RepositoryOf(lambda.PrivateKey, lambda.PublicKey)}",
            lambda.Tier,
            // what the installation calls its tier, for whoever the agent tells
            tierLabel = lambda.Policy.Label,
            lambda.View,
            limits = Limits(lambda),
            // whether its code may connect to other servers, which it does not compile without
            connectivity = Describe(lambda.Connectivity),
            connectivityNote = lambda.Connectivity?.Allowed != true && CodeGuard.Connects(files)
                ? "The code connects to other servers, which this lambda may not do: it does not compile. Fetch what browsers may fetch from the page, or ask with request_connectivity - see platform_guide, connectivity."
                : null,
            lambda.ActiveVersion,
            lambda.LatestVersion,
            online = lambda.ActiveVersion != null,
            lambda.DeployedUntil,
            lambda.KeptUntil,
            // the work under way beside the lambda, which a change should
            // continue rather than start over where one fits
            features = open.Select(f => new { feature = f.Key, f.Name, f.Branch, @base = f.Base, mergeable = f.Mergeable, previewOnline = f.Online, f.Modified }),
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
                        ? "The code reads secrets that have no value yet: Secret.Read throws until the user sets them under Secrets in the editor (Keys and passwords in its simple view), or you store one they gave you with set_secret."
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
                    url = origin + PlatformRoutes.SourceOf(lambda.PublicKey),
                    note = "The owner published this lambda's source: every version is public, and so is what you save next - code, resources, documentation, tests and each version's change line. Keep keys, passwords and personal data out of the files; they belong in secrets and the database, which are never published."
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
        };
    }

    /// <summary>
    /// What is written about a version, as read_lambda hands it over: its
    /// documentation and how it is tested - docs/ and tests/ of its code -
    /// apart from its other files.
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
    /// The files of a version as read_lambda hands them over, beside its
    /// documentation and its tests: in full while they fit, by name and
    /// length after that.
    /// </summary>
    /// <remarks>
    /// The program first - the C#, then the resources, the front end among
    /// them - so a folder of the code that holds a large file, the lock file
    /// of what a front end is built from, does not push the program out of
    /// the answer. What does not fit is named with its length and read with
    /// file. A folder of the code may hold hundreds of files - the sources of
    /// a front end - so only so many are named before the rest is counted.
    /// </remarks>
    private static (IEnumerable<object> Listing, string? Note) Listing(IReadOnlyList<LambdaFile> files, string archive)
    {
        var program = files.Where(f => !f.Name.StartsWith(LambdaSource.DocsFolder, StringComparison.Ordinal)
                                    && !f.Name.StartsWith(LambdaSource.TestsFolder, StringComparison.Ordinal))
                           .OrderBy(f => f.IsCompiled ? 0 : f.IsResource ? 1 : 2)
                           .ToList();

        var budget = (long)ReadBudget;

        var listing = new List<object>();

        var omitted = 0;

        foreach (var file in program)
        {
            if (file.Code.Length <= budget)
            {
                budget -= file.Code.Length;

                listing.Add(new { file.Name, file.Code });
            }
            else if (omitted++ < Listed)
            {
                listing.Add(new { file.Name, length = file.Code.Length });
            }
        }

        if (omitted == 0)
        {
            return (listing, null);
        }

        var more = omitted > Listed ? $" {omitted - Listed} more are not named." : string.Empty;

        return (listing, $"The files come to more than one answer carries: those without code are named with their length.{more} Pass file to read one of them, up to {ReadFileLimit:N0} characters; {archive} has every file.");
    }

    /// <summary>
    /// How many files read_lambda names without their contents, before it
    /// only counts the rest.
    /// </summary>
    private const int Listed = 200;

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
    /// the status. Under the budget every file comes back whole; past it the
    /// answer names the rest with their lengths, and file fetches one in full.
    /// </remarks>
    private const int ReadBudget = 30_000;

    /// <summary>
    /// The largest file read_lambda sends when it is asked for one by name.
    /// </summary>
    /// <remarks>
    /// A lambda in production may ship a hundred megabytes. An answer is built
    /// whole before any of it is sent, and past a megabyte it is more than an
    /// agent reads in one piece anyway - the zip of the version has every file,
    /// and streams it.
    /// </remarks>
    private const int ReadFileLimit = 1024 * 1024;

    /// <summary>
    /// What a lambda may use in its tier, and how it is kept, as read_lambda
    /// reports it - with the tier above, so an agent that runs into a limit
    /// can tell the user what would allow more.
    /// </summary>
    private object Limits(LambdaInfo lambda)
    {
        var policy = lambda.Policy;
        var product = tiers.Get();

        return new
        {
            // a version: its code and its resources together
            buildBytes = policy.MaxVersionBytes,
            // its data: the database and the workspace together
            dataBytes = policy.MaxDataBytes,
            product.Versions,
            product.Features,
            analyticsDays = policy.AnalyticsDays,
            ownDomain = policy.OwnDomain,
            offlineWhenUnused = policy.ExpiresWhenUnused,
            above = lambda.Above is { } above
                ? new { tier = above.Tier.ToString(), label = above.Label, buildBytes = above.MaxVersionBytes, dataBytes = above.MaxDataBytes, ownDomain = above.OwnDomain, offlineWhenUnused = above.ExpiresWhenUnused }
                : null
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
    private object ChangeShowcase(string privateKey, string? title, string? description, string? encoded, bool remove, string origin)
    {
        if (remove)
        {
            showcases.Remove(privateKey);

            logger.LogInformation("Removed lambda {Lambda} from showcase", meta.PublicKeyOf(privateKey));

            return new { ok = true, showcased = false };
        }

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
                    return ToolResult.Error("The image is not valid base64.");
                }
            }

            // a change of one field keeps the others, as the tool promises
            var current = showcases.Get(privateKey);

            entry = showcases.Save(privateKey, new ShowcaseDraft(title ?? current?.Title, description ?? current?.Description, image));

            logger.LogInformation("Added lambda {Lambda} to showcase as '{Title}'", entry.PublicKey, entry.Title);
        }

        if (entry == null)
        {
            return new
            {
                ok = true,
                showcased = false,
                note = "Not in the showcase. Pass title, description and image to add it - only if the user asked for that."
            };
        }

        return new
        {
            ok = true,
            showcased = true,
            entry.Title,
            entry.Description,
            entry.Online,
            page = $"{origin}/showcase",
            note = entry.Online ? null : "Listed once the lambda is online again - deploy it."
        };
    }

    /// <summary>
    /// Reads what was counted of a lambda's visitors, and switches counting on.
    /// </summary>
    private object ReadAnalytics(string privateKey, int? days, bool enable)
    {
        LambdaAnalytics found;

        if (enable)
        {
            found = analytics.Enable(privateKey);

            logger.LogInformation("Switched on analytics of lambda {Lambda}", found.PublicKey);

            if (days != null)
            {
                found = analytics.Get(privateKey, days);
            }
        }
        else
        {
            found = analytics.Get(privateKey, days);

            logger.LogInformation("Read analytics of lambda {Lambda} days {Days}", found.PublicKey, found.Shown);
        }

        if (found.Since == null)
        {
            return new
            {
                ok = true,
                enabled = false,
                note = "Analytics are off for this lambda, so nothing about its visitors was counted. The owner switches them on in the editor (Analytics); pass enable: true only when the user asks for it."
            };
        }

        var minutes = Math.Max(1, (int)found.Interval.TotalMinutes);

        return new
        {
            ok = true,
            analytics = AnalyticsResponse.Of(found),
            note = found.Shown < found.Kept && found.Earliest is { } earliest && earliest < found.From
                ? $"Counted by day in UTC, written down every {minutes} minutes. More is kept than this tier shows: " + (meta.Get(privateKey)?.Above is { } above && above.AnalyticsDays > found.Shown
                    ? $"in the tier '{above.Name}', which whoever runs the installation assigns, the owner sees {above.AnalyticsDays} days of it."
                    : "a tier that shows more shows what was kept as well.")
                : $"Counted by day in UTC, written down every {minutes} minutes - the visits of the last minutes are not in it yet. Views are pages opened, by people and programs alike - botViews are those of programs, so people opened views - botViews; hits are every request; visitors are people who opened a page, counted once a day."
        };
    }

    /// <summary>
    /// Reads, publishes or takes down the source of a lambda.
    /// </summary>
    /// <remarks>
    /// One tool, like the showcase, for the same reason: it is used rarely,
    /// and every tool listed is read by every agent in every conversation.
    /// </remarks>
    private object ChangeSource(string privateKey, string? license, string? author, bool remove, string origin)
    {
        SourceSettings? source;

        if (remove)
        {
            source = sources.Withdraw(privateKey);

            logger.LogInformation("Unpublished source of lambda {Lambda}", source?.PublicKey ?? meta.PublicKeyOf(privateKey));

            return new
            {
                ok = true,
                published = false,
                stars = source?.Stars,
                note = "Taken down: the page and the downloads are gone. Whatever somebody downloaded while it was published stays theirs under the license it came with."
            };
        }

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
            return new
            {
                ok = true,
                published = false,
                stars = source?.Stars,
                licenses = SourceLicenses.All.Select(l => new { l.Id, l.Name, kind = l.Kind.ToString() }),
                note = "Not published. Pass license (and author, if the user named one) to publish it - only if the user asked for that."
            };
        }

        var chosen = SourceLicenses.Find(source.License)!;

        return new
        {
            ok = true,
            published = true,
            license = chosen.Id,
            licenseName = chosen.Name,
            source.Author,
            holder = SourceLicenses.Holder(source.Author, source.PublicKey),
            source.Stars,
            url = origin + PlatformRoutes.SourceOf(source.PublicKey),
            note = "Everything in every version is public now, older versions included - code, resources, documentation, tests and the one line each version says it changed. Never write keys, passwords or personal data into files: secrets and the database stay private."
        };
    }

    private object Demos() => new
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
            url = addresses.Of(d.Key),
            files = DemoCatalog.FilesFor(d).Select(f => f.Name)
        }),
        howToRead = "read_lambda with the demo's privateKey returns every file and the version history; list_files shows what it keeps as data; read_logs shows how it answers real traffic. Open the url to use it.",
        readOnly = "Anything that would change a demo is refused. To build on one, create_lambda with its id as template - that gives a lambda of your own with the same files."
    };

    /// <summary>
    /// The domain of its own the lambda answers at instead of its publicUrl,
    /// which then sends visitors there - for an agent to call and to tell the
    /// user about. Always HTTPS, which plain requests are redirected to - the
    /// operator installs a certificate for the domain.
    /// </summary>
    private static string? DomainUrl(LambdaInfo lambda)
        => lambda.Domain != null && lambda.Policy.OwnDomain ? lambda.Address : null;

    private static IEnumerable<object> Diagnostics(IReadOnlyList<CompilationDiagnostic> diagnostics)
        => diagnostics.Select(d => new { file = d.File ?? LambdaSource.EntryName, d.Line, d.Column, d.Severity, d.Message });

    #endregion

    #region Arguments

    /// <summary>
    /// What a file's name says, wherever a tool takes files.
    /// </summary>
    private const string FileNameField =
        "lambda.cs and every other .cs file, in any folder, are compiled; 'resources/...' is read and served ('resources/web/app.js'); anything else is code kept with the version, never compiled or served - 'docs/...' and 'tests/...' are the documentation and the tests.";

    /// <summary>
    /// What the view of an editor is, wherever a tool takes one.
    /// </summary>
    private const string ViewField =
        "How the editor opens: 'Full' (the default) shows every section, 'Simple' only the app, how it is doing and a box to ask for a change. Simple suits an owner who is not going to read the code - use it when the user asks for a simple editor, or says they are not a developer.";

    private const string PrivateKey = "The editor key.";

    private const string PrivateKeyOrDemo = "The editor key, or the key of a demo.";

    private const string DeployField = "Also deploy what was saved: a version at the public address, a feature at its preview address.";

    private const string CheckField = "Without deploy: compile what was saved and answer with its diagnostics, putting nothing online.";

    /// <summary>
    /// The licenses a source may be published under, as open_source names them.
    /// </summary>
    /// <remarks>
    /// Written out, since a description is fixed when the tools are described;
    /// a test holds it to <see cref="SourceLicenses.All"/>.
    /// </remarks>
    private const string LicenseField =
        "The SPDX identifier of the license: MIT, Apache-2.0, BSD-3-Clause, MPL-2.0, GPL-3.0-or-later, AGPL-3.0-or-later or Unlicense. Left out, it stays as it is - " + SourceLicenses.Default + " when first published.";

    /// <summary>
    /// The encoding of a file that is not text.
    /// </summary>
    private const string Base64 = "base64";

    /// <summary>
    /// A file as write_code, change_code and check_code take it.
    /// </summary>
    /// <remarks>
    /// The encoding is checked with the rest of the files, by the rule the
    /// API holds them to as well (<see cref="LambdaSource.Validate"/>), which
    /// takes what agents send for text besides leaving it out.
    /// </remarks>
    public sealed record CodeFile([Summary(FileNameField)] string Name,
                                  [Summary("Contents, base64 if encoding says so.")] string Code,
                                  [Summary("'base64' for binary files; omit otherwise.")] string? Encoding = null);

    /// <summary>
    /// A replacement of text within a file, as change_code takes it.
    /// </summary>
    public sealed record CodeEdit([Summary("The file to change.")] string File,
                                  [Summary("Text that occurs exactly once in the file.")] string Find,
                                  [Summary("What to put in its place.")] string Replace);

    /// <summary>
    /// What a save is asked to do besides storing the files.
    /// </summary>
    /// <param name="Feature">The feature to save into, or nothing for a new version</param>
    private sealed record SaveRequest(string? Feature, string? Specification, string? Change, bool Deploy, bool Check);

    private static List<LambdaFile> Files(IEnumerable<CodeFile> files)
        => files.Select(f => new LambdaFile(f.Name, f.Code, f.Encoding)).ToList();

    #endregion

}
