namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Addressing">Where the lambdas answer: <c>Paths</c> or <c>Subdomains</c></param>
/// <param name="HostingDomain">Whether the lambdas have a domain of their own (<c>LAMBDA_HOSTING_URL</c>)</param>
/// <param name="BuildAgent">Whether a build agent is configured</param>
/// <param name="BuildBox">Whether the front page offers the box that has an app built</param>
/// <param name="ChangeBox">Whether the editor offers the box that has an app changed</param>
/// <param name="AdminPanel">Whether the administration panel is on</param>
/// <param name="Terms">Whether the installation has terms of use</param>
/// <param name="Contact">Whether it names a contact</param>
/// <param name="AbuseContact">Whether it names where abuse is reported</param>
/// <param name="LegalPages">Whether it has legal pages</param>
/// <param name="McpOrigins">How many origins a browser may drive the MCP endpoint from</param>
/// <param name="ClientAddresses">Whether the log keeps the addresses requests came from</param>
/// <param name="Geo">Whether the log says which country a caller is in</param>
/// <param name="GeoPlaces">Whether it says which city and network as well</param>
/// <param name="LambdaOutput">Whether what lambdas print is kept</param>
/// <param name="CrawlerLists">Whether the lists of crawlers are fetched for the analytics</param>
/// <param name="Development">Whether the server runs in development mode</param>
/// <param name="Branded">Whether its operator gave it a name or a logo of its own - never the name or the logo</param>
/// <param name="Database">What keeps the databases: <c>sqlite</c> or <c>postgresql</c> - never where it is</param>
/// <param name="Files">What keeps the lambdas' files: <c>local</c></param>
/// <param name="Secrets">What keeps their secrets: <c>database</c>, sealed in the platform's own</param>
/// <param name="OptimizeDatabases">Whether the platform optimizes the databases itself, in their quiet hours, or its operator left them to their own tools</param>
/// <param name="Profile">Who it is for, as its operator chose it: <c>home</c> or <c>enterprise</c> - never whose it is</param>
/// <param name="SharedFiles">How many files its operator shares across it - never what they are called or hold</param>
/// <param name="Agent">Which agent integration builds and changes apps, by its name alone - <c>none</c> without one</param>
public sealed record UsageConfiguration(string Addressing, bool HostingDomain, bool BuildAgent, bool BuildBox, bool ChangeBox, bool AdminPanel,
                                        bool Terms, bool Contact, bool AbuseContact, bool LegalPages, int McpOrigins, bool ClientAddresses,
                                        bool Geo, bool GeoPlaces, bool LambdaOutput, bool CrawlerLists, bool Development, bool Branded,
                                        string Database, string Files, string Secrets, bool OptimizeDatabases, string Profile,
                                        int SharedFiles, string Agent = "none");
