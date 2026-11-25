using System.Text.Json;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The pages a build rendered, as they are on disk and kept in memory once
/// read.
/// </summary>
/// <remarks>
/// A few megabytes of text, and the same for every platform of a process that
/// serves the bundle: what differs between them - the address, the limits, the
/// brand - is filled in as a page is sent (see <see cref="SitePrerender"/>).
/// So there is one for a file, shared by whoever serves it
/// (<see cref="LambdaRuntime.Shared"/>).
/// </remarks>
public sealed class PrerenderFile(string file)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private volatile Snapshot? _cached;

    #region Functionality

    /// <summary>
    /// What the build rendered, or nothing where there is no build or it does
    /// not read.
    /// </summary>
    public Renderings? Read()
    {
        // read again whenever it changes, because "npm run build" updates a
        // running server - and only then, because it is a few megabytes
        try
        {
            var written = File.GetLastWriteTimeUtc(file);

            if (_cached is { } cached && cached.Written == written)
            {
                return cached.Content;
            }

            var content = File.Exists(file)
                        ? JsonSerializer.Deserialize<Prerendered>(File.ReadAllText(file), Json)
                        : null;

            var renderings = content == null ? null : new Renderings(content);

            _cached = new Snapshot(written, renderings);

            return renderings;
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            // no build, or a broken one: the pages are still served, empty
            return null;
        }
    }

    private sealed record Snapshot(DateTime Written, Renderings? Content);

    /// <summary>
    /// The pages as each profile and brand shows them, made once as the
    /// build is read: a profile's own pages over the ones every profile
    /// shares - the same strings, so it costs the dictionaries alone.
    /// </summary>
    public sealed class Renderings(Prerendered content)
    {
        private readonly Dictionary<(InstallationProfile Profile, bool Branded), PrerenderedPages?> _made = Make(content);

        public Prerendered Content => content;

        /// <summary>
        /// The pages as the given profile and brand show them, or nothing
        /// where the build did not render them so.
        /// </summary>
        public PrerenderedPages? For(InstallationProfile profile, bool branded)
            => content.Profiles == null
                   ? branded ? content.Branded : content.Plain
                   : _made.GetValueOrDefault((profile, branded));

        private static Dictionary<(InstallationProfile, bool), PrerenderedPages?> Make(Prerendered content)
        {
            var made = new Dictionary<(InstallationProfile, bool), PrerenderedPages?>();

            foreach (var (name, own) in content.Profiles ?? [])
            {
                if (InstallationProfiles.TryParse(name, out var profile))
                {
                    made[(profile.Value, false)] = Over(content.Plain, own.Plain);
                    made[(profile.Value, true)] = content.Branded is { } branded && own.Branded is { } brandedOwn ? Over(branded, brandedOwn) : null;
                }
            }

            return made;
        }

        private static PrerenderedPages Over(PrerenderedPages shared, PrerenderedPages own)
        {
            var pages = new Dictionary<string, string>(shared.Pages);

            foreach (var (path, page) in own.Pages)
            {
                pages[path] = page;
            }

            var showcase = shared.Showcase.ToDictionary(l => l.Key, l => new Dictionary<string, string>(l.Value));

            foreach (var (language, shapes) in own.Showcase)
            {
                var into = showcase.TryGetValue(language, out var existing) ? existing : showcase[language] = [];

                foreach (var (shape, page) in shapes)
                {
                    into[shape] = page;
                }
            }

            return new PrerenderedPages(pages, showcase);
        }

    }

    #endregion

}
