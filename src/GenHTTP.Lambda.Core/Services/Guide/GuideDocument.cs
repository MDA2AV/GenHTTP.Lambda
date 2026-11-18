using System.Net;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// What <c>platform_guide</c> answers: how this platform works, for an agent,
/// as one Markdown document - with the operator's section last, where they
/// wrote one.
/// </summary>
/// <remarks>
/// <para>
/// Written for a model, not a person: terse, a sentence only where it changes
/// what an agent writes or calls, each rule said once, where the agent decides
/// it. As a JSON object it had grown to 65 KB, which clients put into a file
/// rather than the conversation - and the MCP module sends an object twice,
/// as text and as structured content, with every quote of a code example
/// escaped. A string is sent once, as text. A test holds it to a budget.
/// </para>
/// <para>
/// The sections are headings named the way everything else points at them
/// ("platform_guide, connectivity"): the MCP instructions, the tools'
/// descriptions and answers, the code guard, the build agent's brief and a
/// repository's <c>AGENTS.md</c>. Renaming one means renaming every pointer,
/// and the operator's section may take none of their names
/// (<see cref="Headings"/>). What a tool's description already says, and what
/// a demo shows, is not repeated: an agent has the descriptions in front of it
/// and is sent to the demos first.
/// </para>
/// <para>
/// What differs per installation is filled in: its addresses, what it is
/// called, the tiers' labels and limits, whether lambdas answer by path, its
/// terms and its operator's section.
/// </para>
/// </remarks>
internal sealed class GuideDocument(LambdaOptions options, ILimitsService tiers, ILambdaAddresses addresses)
{
    private IReadOnlySet<string>? _headings;

    #region Functionality

    /// <summary>
    /// Every heading of the platform's own guide, which the operator's section
    /// may not be called: a pointer to it would find two.
    /// </summary>
    public IReadOnlySet<string> Headings => _headings ??= Read(Write("", "", null));

    /// <param name="origin">The address the agent called, used where the installation names no public one</param>
    /// <param name="made">What this platform is called - GenHTTP Lambda, or what its operator named it</param>
    /// <param name="section">What the operator wrote for the agents of this installation, if anything</param>
    public string Write(string origin, string made, GuideSection? section)
    {
        var home = Home(origin);
        var address = addresses.Of("{publicKey}");
        var entry = LambdaSource.EntryName;
        var limits = tiers.Get();

        var byPath = addresses.ByPath ? ", and the lambda itself below /lambda/ here" : "";

        var icon = addresses.ByPath ? " - search results show one icon per host, which a lambda here shares with this platform until it has a domain" : "";

        var link = addresses.ByPath
            ? $"<a href=\"{home}\">Made with {WebUtility.HtmlEncode(made)}</a> - no rel=\"nofollow\", since the lambdas here answer below this platform's address; a line that has it can stay as it is."
            : $"<a href=\"{home}\" rel=\"nofollow\">Made with {WebUtility.HtmlEncode(made)}</a>. A line that links without rel=\"nofollow\" is given it with your next change.";

        var connecting = string.Join(" and ", tiers.TiersWhere(p => p.Connectivity && !p.ReadOnly).Select(Identifier).DefaultIfEmpty("no tier"));

        var shown = string.Join(", ", tiers.TiersWhere(p => !p.ReadOnly).Select(t => $"{Identifier(t)} {tiers.PolicyOf(t).AnalyticsDays} days"));

        // last, after the platform's own: what the platform requires matters
        // most to an agent, and the operator's rules are how lambdas are built
        // here within it - followed where the guide leaves the choice open
        var rules = section is { } own
            ? $"\n## {own.Title}\n\nSet by whoever runs this installation, for every lambda built here. Follow it wherever the guide above leaves the choice to you; it changes nothing the platform requires or refuses.\n\n{own.Text}\n"
            : "";

        var terms = options.Terms is { } text
            ? $"\n## terms\n\nThe user accepts them with create_lambda's acceptTerms:\n\n{text.Trim()}\n"
            : "";

        return $$"""
            # Platform guide

            A lambda is C# that returns a GenHTTP handler, served at {{address}} (its publicUrl). {{entry}} is its top-level code - no Main, no project - and returns any IHandler or IHandlerBuilder - from the GenHTTP modules listed under compiling, or your own. GenHTTP's documentation describes them: https://genhttp.org/documentation/content/. "platform_guide, X" in a tool means the section ## X.

            ```csharp
            return Inline.Create().Get(() => new Greeting("hello"));

            record Greeting(string Text); // routes cannot return anonymous types
            ```

            ## lifecycle

            - Version: the program - its code and resources. Never changes once saved; deploy puts one online, an older one rolls back. The newest {{limits.Versions}} are kept, and the one online: one per thing the user asked for, not per attempt.
            - Feature: a change made beside the lambda, which keeps serving the version online. It starts with a copy of its files and of the lambda's data, is changed in place and answers at its previewUrl, below /features/{feature}/. Tools take its key (create_feature, read_lambda, the previewUrl), never its name. Up to {{limits.Features}} open: continue one that fits.
            - Data: the database, the workspace, the secrets - shared by every version, untouched by saves, deploys, rollbacks and merges. enable_data switches on what you build needs (the workspace is on unless the owner switched it off); switching off deletes, so only the owner does it - ask before switching on what they switched off. Data may be empty: create what the code needs on first use.

            Where things go: the app - its C# and the whole front end - in the version, the front end in resources/web/ served by Resources.App("web"). Records - entries, accounts, sessions, orders, votes, scores - in the database, never in JSON files. Files users upload, and large inputs that are not program - a model, a dataset, media - in the workspace. Never user data in resources (read only, replaced by every deploy); never the front end in the workspace (unversioned: a rollback keeps new pages over the old API).

            New lambda: create_lambda, write_code with deploy: true, docs/ and tests/ in the same save, fix with change_code and deploy: true. No feature needed.

            Existing lambda: read_lambda, documentation first; create_feature or continue one; change it with feature and deploy: true; read_logs with feature; run its tests against the previewUrl; update docs/ and tests/; merge_feature with deploy: true - or hand the user the previewUrl to look first.

            Only a feature based on the newest version merges. When read_lambda with feature lists newerVersions, make their changes in the feature yourself, then move its base with update_feature. Nothing merges or rebases for you, and nothing checks that the changes are in. A merge drops the feature's copy of the data: changed code must still read the lambda's.

            Blocked: the operator can block a lambda that breaks the rules - offline with its previews, every change refused, reading allowed. Do not retry or work around it: tell the user; their editor says why.

            ## git

            If you can run git, clone the lambda: read_lambda's gitUrl, which holds the editor key - keep it between you and the user. The clone builds and runs with dotnet run, pushes cost no tokens, and its AGENTS.md says how to work there.

            - It is the exported project: Project.cs is {{entry}}, the rest as in the lambda, around it the platform's files (Program.cs, the .csproj, Platform/, AGENTS.md ...), which a push may not change.
            - Each commit pushed to main is the next version: linear, no merge commits (git pull --rebase), the message's first line its change. A push to main that does not compile is refused; -o deploy puts it online.
            - Any other branch is a feature: a push starts or replaces it and puts its preview online. git push origin <branch>:main merges it as a version per commit, a push of the branch with -o merge as one; deleting the branch deletes the feature.
            - The data is not in the repository. A local build differs: it runs source generators (see compiling) and answers errors in GenHTTP's shape (see requests).

            ## restApi

            If you can make HTTP requests, the REST API ({{home}}api/v1/openapi.json) does what these tools do, with files sent as files: GET /api/v1/lambdas/{privateKey}/versions/{version}/zip, POST /api/v1/lambdas/{privateKey}/versions/zip, GET and PUT /api/v1/lambdas/{privateKey}/features/{feature}/zip - edit locally, put it back as often as needed. Saving takes ?deploy=true. A zip holds the files as the lambda names them ({{entry}}, resources/ ...), or as a clone with ?layout=project; zip a folder's contents with its dot files (zip -r ../f.zip .). What its .gitignore files ignore is left out.

            ## code

            The code is every file outside resources/.

            - Every .cs file, in any folder, is compiled as in a C# project: {{entry}} returns the handler, the others hold types. A test or a tool in C# is not saved as .cs.
            - Every other file - docs/, tests/, what a tool builds from, configuration - is kept, never compiled or served.
            - Workspace, Resources, Secret and Database work in every .cs file; a type of yours with one of those names hides it - then write LambdaEnvironment.Resources and so on.
            - No spaces in names; a .cs file starts with a letter. Taken at the top: Dockerfile, LICENSE, AGENTS.md, CLAUDE.md, .gitignore, .dockerignore, a .csproj, Platform/, bin/, obj/, workspace/, database/. No executables: run a script through its interpreter.
            - Every clone, export and published source carries every file: no key or password in any, .env included.

            ### builtWithATool

            The platform never builds anything and runs no tool. Where a tool makes part of the lambda - a bundled front end, compiled styles, generated C#:

            - Keep its sources in a folder of the code (frontend/) with a README.md naming the commands and where the output goes; note it in docs/decisions.md.
            - Build where you work, into resources/ (or C#) over the previous output, and save sources and output together - every file it wrote, hashed names included. Never edit the output: the next build undoes it.
            - The output is served at the root and below /features/{feature}/: relative paths (base './', never '/').
            - Keep installs and caches (node_modules/, .venv/, target/) out with a .gitignore in that folder, and out of zips, which are counted before .gitignore applies.
            - Without a shell to build with, leave sources and output alone and say why.

            ## resources

            The files below resources/: part of the version, served and read at runtime, read only, never compiled - the front end and the database's migrations. Nothing outside resources/ is served.

            - Send them with the code: { name: "resources/web/app.css", code: "..." }, binary as base64 with encoding "base64" - styles and scripts as files, not C# strings.
            - Resources.App("web") serves resources/web/ at the root as a single page application (resources/web/app.css is app.css); index.html answers every path no file matches. As every demo does: return Layout.Create().Add("api", api).Add(Resources.App("web"));
            - Resources.Files("media") serves a folder as plain files, the fastest way. Resources.ReadText("web/app.css"), ReadBytes, Exists, List, Tree() read them; Resources.Root is their path.
            - Names: letters, digits, - _ . and /, with an extension; no dot files, no "..".
            - Each version keeps a copy, read whole on every save and deploy: large data belongs in the workspace.

            ## paths

            Link relatively everywhere - links, scripts, styles, images, fetch, forms, websockets, redirects: "api/items", "app.css", "./". No leading slash, never the full address. The lambda answers at the root of {{address}} or its domain, but a preview below /features/{feature}/ on this platform{{byPath}}: a leading slash leaves it, and the full address is the live lambda with the real data.

            - One level down, "../api/items"; simpler to keep pages at the root.
            - A page answering at any depth - a 404 page, Resources.App's index.html for a route like items/42 - cannot load files relatively: route by the hash (#/items/42), and keep a 404 page self-contained.
            - A websocket: new URL("play", location.href), the scheme set to ws: or wss:.
            - In C#, Redirect.To("other") and Location headers are relative; never build an address from the request's host.
            - Register a service worker relatively ("sw.js"); Service-Worker-Allowed is refused on the platform's address.
            - Full addresses only for og:image, og:url and the canonical link (see beingFound).

            ## compiling

            - Imported in every file: {{Imports()}}. Those are the GenHTTP modules available; ReverseProxy and DependencyInjection are refused. Anything else needs a using - System.Text.RegularExpressions and System.Net.Http among them.
            - No source generators run: [GeneratedRegex] fails with CS8795, and so does whatever else a generator would write - keep a new Regex(...) in a static field. A local build of a clone runs them.
            - Refused: reflection, dynamic, processes, the environment, the file system beyond Workspace and Resources - lambdas share one process. A type of yours named like a refused one is fine.
            - Never wait for a task - .Result, .Wait(), .GetAwaiter().GetResult(), Task.WaitAll, Task.WaitAny, SemaphoreSlim.Wait() are refused: a request's thread is the one its task finishes on. Await (routes may be async), use await semaphore.WaitAsync() or a lock without an await inside, or the synchronous method.

            ## requests

            - A body binds by type: a record parameter is read as JSON; a bare string parameter is null.
            - Taking a body - a record or a Stream parameter alike - releases the headers before the method runs. Check a header in a concern in front of the route (the Authentication module, demo-registration) or in a route without a body.
            - An IRequest parameter reads the request: request.Header.Headers.GetEntry("X-Token"), request.Header.Query.GetEntry("q"), request.Client.Address.
            - request.Client.Address is the connection's address - the visitor's, unless the installation runs behind a proxy. The platform adds no forwarding headers: X-Forwarded-For is what the sender wrote - never key a limit, a block or an account to it.
            - throw new ProviderException(ResponseStatus.NotFound, "No such task") answers with that status. Unless the lambda adds an ErrorHandler, the body is {"status":404,"title":"NotFound","message":"No such task"} - HTML to a browser - and GenHTTP's default shape in an export: clients read the status.
            - A browser sets no headers on a websocket handshake: pass values in the query (connection.Request.Header.Query), a secret as the first frame.
            - REST routes write camel case JSON; match it on sockets.

            ## database

            SQLite, the lambda's own, for its records. enable_data 'database' makes it empty; switching it on or off restarts the lambda on its next request; while off, Database.GetConnection() throws. demo-crud shows it all: {{entry}} migrates, Store.cs maps and queries, resources/migrations/ holds the schema.

            The schema is Evolve migrations in resources/migrations/ (V1__Create_tasks.sql, V2__Add_due_date.sql), applied at the top of {{entry}} before anything is served:

            ```csharp
            using (var connection = Database.GetConnection())
            {
                new Evolve(connection, message => Console.WriteLine(message)) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
            }
            ```

            - At the top level, a using declaration (using var) does not compile.
            - Never change an applied migration - Evolve refuses to start; a schema change is the next file. A preview migrates its feature's copy first.
            - A rollback runs older code on a newer schema: prefer additive changes - new tables, columns with a default - over dropping or renaming what an older version reads.

            Records go through Entity Framework Core: a DbContext of yours mapping those tables on the connection handed in - class Records(SqliteConnection connection) : DbContext with options.UseSqlite(connection, contextOwnsConnection: true), used as using var db = new Records(Database.GetConnection()) per request or call, never kept in a shared field.

            - Synchronous only: ToList, Find, SaveChanges, ExecuteUpdate - never ToListAsync, SaveChangesAsync or any other Async form.
            - Inside a DbContext, Database is the context's own: hand the connection in.
            - EnsureCreated, Migrate and EF's migrations are refused: a new property needs a migration first.
            - ExecuteUpdate(s => s.SetProperty(v => v.Votes, v => v.Votes + 1)) counts every request without a lock; add a row that may not exist inside db.Database.BeginTransaction().
            - A taken unique value fails SaveChanges with a DbUpdateException over a SqliteException with SqliteErrorCode 19: answer 409. Keep times as ISO 8601 UTC text, as demo-crud does.
            - Of EF's namespaces only the root, Metadata.Builders, ChangeTracking and Storage.ValueConversion; refused too: new SqliteConnection, UseSqlite with a connection string, ATTACH, VACUUM INTO.
            - Past the data's room (see limits), writes fail with "database or disk is full".

            ## workspace

            The lambda's private files, as data: shared by every version, copied for a feature; switched off by the owner, every member throws. Workspace.ReadText / WriteText, ReadBytes / WriteBytes, Exists, Delete, List(), Folders(), CreateFolder(); Files(folder) serves it - Workspace.Files("uploads") for what users uploaded - and Workspace.Root is its path. No Append; nothing else on disk is reachable. From outside: upload_file, list_files, delete_file, or for a large file PUT {{home}}api/v1/lambdas/{privateKey}/files/{path}/content with the file as the body (curl -T), the path's slashes as %2F.

            ## secrets

            API keys, passwords, tokens. Data like the workspace; a feature has a copy, which set_secret with feature changes.

            - Off until enable_data 'secrets'. Secret.Read("STRIPE_KEY") returns the value or throws saying how to set it; Secret.Exists("STRIPE_KEY") checks. Read it where it is used, not into a static, so a change applies without a deploy.
            - Write Secret.Read("NAME"), switch secrets on, and tell the user which names to enter in the editor (Secrets; Keys and passwords in the simple view) and where to get the values: no value passes through you. list_secrets names the missing ones. A value the user hands you goes to set_secret.
            - Never put a value in code, resources, the workspace, a log line, a specification or a change, nor return, print or repeat it. Nothing shows one again.
            - Names: letters, digits and underscores, no leading digit, case sensitive. Up to {{SecretVault.MaxSecrets}}, {{SecretVault.MaxValue / 1024}} KB each. An export reads Secret.Read("X") from the environment variable X.

            ## connectivity

            Whether the code may connect to other servers: HttpClient, sockets, WebSocket and mail clients, DNS, XML from an address. Where its tier connects by default - here {{connecting}} - unless the operator declined, otherwise once the operator allowed it (read_lambda: connectivity.allowed). Without it, such code does not compile (LAMBDA0003).

            - A public API that browsers may call - open data, weather, maps - is fetched with fetch() from the page: nothing to ask. Only where the server must make the call, request_connectivity, and deploy the code that connects once connectivity.allowed is true.
            - A lambda fetching whatever address it is handed - a proxy, a scraper - is declined and may be blocked.
            - Give every outgoing call a timeout.

            ## liveUpdates

            Never poll - no fetch on a timer to see whether something changed: on a shared server each is a request for nothing. Push changes:

            - The page only listens: server-sent events - EventSource.Create().Generator(...) at a relative path (events), new EventSource("events") in the browser, a POST to say something back. demo-live shows it.
            - The browser talks back - a game, a chat: Websocket.Reactive() or Websocket.Functional(), addressed as in paths; guard writes to a socket with a semaphore (await WaitAsync()). demo-game shows it.
            - Loading when the page opens or the user acts is not polling.

            ## toolsForAgents

            A lambda can offer MCP tools: .Add("mcp", Tools.Create().Server("name", "1.0.0").Add(new MyTools(store))), as demo-crud's Tools.cs does - public methods marked [Tool], described with [Summary] on them and their parameters, checked by [Range], [MaxLength], [AllowedValues]. A problem the agent can fix is a ProviderException or ToolResult.Error("why"). Each call stands alone; where there is an API too, both call the same code.

            ## describingAnApi

            .AddOpenApi() and .AddScalar() on a layout serve openapi.json and a page to try it at scalar/, as demo-crud does; [Summary], [Remarks], [Tag] and [Response] describe it. The Authentication module describes the credentials it reads; a key read by an Extractor of yours, .Documentation(SecurityScheme.ApiKey(...)) from GenHTTP.Api.Content.Documentation.Security.

            ## documentationAndTests

            Files of every version's code, never compiled or served: the next agent starts from them, the owner reads them.

            - docs/product.md: the app in a sentence or two first (the editor's overview shows it), then who uses it and why, each feature and why, what it deliberately does not do - the user's terms, every request folded in, no code.
            - docs/decisions.md: an entry per technical decision - what, why, what a change must keep in mind: structure, how data stays readable across versions, secrets, what was left out.
            - tests/README.md: what must keep working, a line each, with the request and the answer expected, and how to run the scripts beside it - node smoke.mjs <url>, curl or a .http file - against a previewUrl, never the live lambda for a test that writes. Test records go in through the app's routes (demo-crud's seed.mjs).
            - In proportion: a small lambda gets a short paragraph a page and one quick check - no test suite, extra pages or seeded data. They grow with the app.
            - When: all three with a new lambda's first write_code; with every change, what it affects, in the same save.
            - Use: read them before changing a lambda; before merging, run tests/README.md against the previewUrl, fix what fails.
            - Markdown, in the user's language.

            ## beingFound

            For a page meant to be found or shared - a website, a shop, an event. A tool for a few people needs a title only.

            - <title>: what, then whose - "Menu and opening hours - Café Lindner" - about 60 characters, one per page; <meta name="description" content="...">: one or two sentences in the words people search for. Both in the page's language, with <html lang> set.
            - A favicon resource linked relatively: <link rel="icon" href="icon.svg" type="image/svg+xml">{{icon}}.
            - og:title, og:description, og:type, og:image, <meta name="twitter:card" content="summary_large_image">. The picture: a PNG or JPEG resource, 1200 by 630 - no SVG. og:image is a full address: the domainUrl if there is one, else the publicUrl (read_lambda), then its path below resources/.
            - With a domain: <link rel="canonical"> and og:url on the domain.
            - Crawlers mostly run no scripts: the title, the description, a heading and the opening text belong in the HTML as served.
            - In proportion, for people: no keyword lists, no hidden text. Previews are never indexed; noindex keeps a lambda out.

            ## backlink

            We ask - a request, not a rule - for a small line at the foot of the pages a lambda shows its visitors: "Made with {{made}}", in the page's language, so people find where an app was made.

            - Without a domain of its own (no domainUrl): {{link}}
            - One line, small muted type fitting the design; the name is the whole link - no keywords, badge, script or hidden text.
            - With a domain of its own the line is plain text; one that links is made plain text with your next change.
            - A line naming this platform otherwise is given this name with your next change to the page.
            - With a new lambda that has pages people visit - not an API, a widget in another site, a full-screen page. Changing a lambda, keep its line, and add none it lacks unless asked.
            - Say that you added it. If the user does not want it, leave it out or remove it, and note that in docs/decisions.md so it is not put back.

            ## limits

            A lambda's tier decides what it may use and how it is kept; the operator assigns it, the owner cannot. {{Capital(Called(LambdaTier.Sandbox))}} is for trying something, {{Called(LambdaTier.Production)}} for an app people use; a new lambda starts in {{Called(limits.DefaultTier)}}. read_lambda says a lambda's tier, its limits and the tier above; a refusal for size names both. Tell the user the tierLabel; name a tier by its identifier (Sandbox, Production) to the tools.

            - {{Capital(Called(LambdaTier.Sandbox))}}: {{Allowance(LambdaTier.Sandbox)}}
            - {{Capital(Called(LambdaTier.Production))}}: {{Allowance(LambdaTier.Production)}}
            - Moved to a tier that allows less, a lambda keeps what it has; what it saves or writes next meets the new limits.

            ## analytics

            The platform counts visits for the owner once analytics is switched on - never a preview: build no counter of your own. Of the year kept, a tier shows: {{shown}}. Visitors are people; views include programs, as botViews.

            ## showcase

            Only when the user asks. The picture shows the lambda in use, up to {{Size(limits.ShowcaseImageBytes)}}; one too large for a tool call goes by PUT {{home}}api/v1/lambdas/{privateKey}/showcase with {"title": "...", "description": "...", "image": "<base64>"} written to a file: curl -X PUT -H "Content-Type: application/json" --data-binary @entry.json <url>.

            ## openSource

            Only when the user asks; MIT unless they want another license. A published source is at /source/{publicKey} and clones read only from /source/{publicKey}.git. Never published: the data (the database, the workspace, the secrets' values), the specifications, anything about visitors.

            Starting from one: with git, create_lambda, clone its gitUrl, copy the source's Project.cs, other top-level .cs files, resources/ and the rest of its code over the clone's, commit, push. Without git, the body of Build() or BuildAsync() in Project.cs becomes {{entry}}, the types after the class below it; other files keep their paths; Program.cs, the .csproj, Platform/, the Dockerfile and ignore files stay behind. Keep to its LICENSE.

            ## export

            GET {{home}}api/v1/lambdas/{privateKey}/export: the newest version as a standalone .NET 10 project with a Dockerfile and its database, needing nothing from here. The code is the user's - worth telling them.
            {{terms}}{{rules}}
            """;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The headings of a document, by their words, whatever their case.
    /// </summary>
    private static HashSet<string> Read(string document)
        => document.Split('\n')
                   .Where(l => l.StartsWith('#'))
                   .Select(l => l.TrimStart('#').Trim())
                   .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// What the link back to this platform points at.
    /// </summary>
    /// <remarks>
    /// The address the site is meant to be found at where the installation
    /// names one: the build agent reaches this endpoint by whatever address
    /// its container was given, which is not one to send visitors to.
    /// </remarks>
    private string Home(string origin) => $"{options.PublicUrl ?? origin}/";

    /// <summary>
    /// The namespaces every file imports, each family said once - GenHTTP's
    /// modules by their names, which are what an agent looks up in its
    /// documentation.
    /// </summary>
    private static string Imports()
    {
        var others = ModuleCatalog.Imports.Where(i => i != "System" && !i.StartsWith("System.", StringComparison.Ordinal) && !i.StartsWith("GenHTTP.", StringComparison.Ordinal));

        return string.Join(", ", (string[])["System", Family("System."), .. others, Family("GenHTTP.Api."), Family("GenHTTP.Modules.")]);
    }

    /// <summary>
    /// The imports below a namespace, said once: <c>System.{IO, Linq}</c>.
    /// </summary>
    private static string Family(string prefix)
        => $"{prefix}{{{string.Join(", ", ModuleCatalog.Imports.Where(i => i.StartsWith(prefix, StringComparison.Ordinal)).Select(i => i[prefix.Length..]))}}}";

    /// <summary>
    /// What a lambda in the given tier may use, as the guide says it.
    /// </summary>
    private string Allowance(LambdaTier tier)
    {
        var policy = tiers.PolicyOf(tier);
        var limits = tiers.Get();

        return $"A version: {Size(policy.MaxVersionBytes)} - its code and its resources together. "
             + $"Its data: {Size(policy.MaxDataBytes)} - its database and its workspace together. "
             + (policy.OwnDomain ? "A domain of its own. " : "No domain of its own. ")
             + (policy.ExpiresWhenUnused
                    ? $"Taken offline after {(int)limits.OfflineAfter.TotalDays} days without visits or edits, and removed about {(int)limits.RemovedAfter.TotalDays} days after the last of either; a preview goes offline after as long without work."
                    : "Kept online whether used or not.");
    }

    /// <summary>
    /// A tier as the guide names it: by its identifier, and by what the
    /// installation calls it where that differs - "the sandbox ('Free' here)".
    /// </summary>
    private string Called(LambdaTier tier)
        => tiers.PolicyOf(tier).Label is { } label ? $"{Identifier(tier)} ('{label}' here)" : Identifier(tier);

    /// <summary>
    /// A tier by its identifier only, where the limits above have said what
    /// the installation calls it.
    /// </summary>
    private static string Identifier(LambdaTier tier) => tier switch
    {
        LambdaTier.Sandbox => "the sandbox",
        LambdaTier.Production => "production",
        LambdaTier.System => "the system tier",
        _ => $"the {tier.ToString().ToLowerInvariant()} tier"
    };

    private static string Capital(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    private static string Size(long bytes) => bytes switch
    {
        >= 1L << 30 when bytes % (1L << 30) == 0 => $"{bytes >> 30} GB",
        >= 1L << 20 when bytes % (1L << 20) == 0 => $"{bytes >> 20} MB",
        _ => $"{bytes >> 10:N0} KB"
    };

    #endregion

}
