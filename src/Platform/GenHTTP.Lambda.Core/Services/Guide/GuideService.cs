using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Integrations.Secrets;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.SharedFiles;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// The guide agents read first, and the section its operator writes into it.
/// </summary>
/// <remarks>
/// The document is <see cref="GuideDocument"/>'s and what a section may be
/// <see cref="GuideChecks"/>'. The section is kept as two settings
/// (<c>guide.title</c>, <c>guide.text</c>), read once as the platform starts
/// (<see cref="LoadAsync"/>) and held in memory, as the branding is; the guide is written anew with every call, since it is
/// called rarely and differs by the address it was asked at.
///
/// Where the installation has no admin token, a section in the database is
/// not read: a copy of another installation's data would otherwise hand
/// agents rules nobody here can see or take away.
/// </remarks>
public sealed class GuideService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options, ILimitsService tiers, ILambdaAddresses addresses,
                                 IBrandingService branding, ISettingsService settings, IDatabaseProvider database, IFileProvider files,
                                 ISecretProvider secrets, ISharedFiles shared) : IGuideService
{
    private const string TitleKey = "guide.title";

    private const string TextKey = "guide.text";

    private readonly GuideDocument _document = new(options, tiers, addresses, database, files, secrets);

    /// <summary>
    /// Who is changing the section: one at a time, across the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private volatile GuideSection? _section;

    private volatile IReadOnlySet<string> _linked = new HashSet<string>();

    #region Reading

    /// <summary>
    /// The operator's section, if there is one.
    /// </summary>
    public GuideSection? Section
    {
        get
        {
            return options.Administrable ? _section : null;
        }
    }

    /// <summary>
    /// The shared files the section links to - none where there is no
    /// operator, whose section is then not read either.
    /// </summary>
    public IReadOnlySet<string> SharedFiles => _linked;

    /// <summary>
    /// The guide as an agent reads it now.
    /// </summary>
    public async ValueTask<string> WriteAsync(string origin, CancellationToken cancellation = default)
        => _document.Write(origin, branding.Current.Name, Section, settings.Get().Profile, await shared.ListAsync(cancellation));

    /// <summary>
    /// Reads the operator's section into memory, before anything is served -
    /// where there is an operator to have written one.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
    {
        if (!options.Administrable)
        {
            return;
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking()
                                   .Where(s => s.Key == TitleKey || s.Key == TextKey)
                                   .ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        Hold(stored.TryGetValue(TitleKey, out var title) && stored.TryGetValue(TextKey, out var text) ? new GuideSection(title, text) : null);
    }

    private void Hold(GuideSection? section)
    {
        _section = section;
        _linked = GuideLinks.Names(section?.Text);
    }

    #endregion

    #region Changing

    /// <summary>
    /// Writes the operator's section, or replaces it.
    /// </summary>
    public async ValueTask<GuideSection> ChangeAsync(string? title, string? text, CancellationToken cancellation = default)
    {
        RequireOperator();

        var section = GuideChecks.Section(title, text, _document.Headings);

        await RequireSharedAsync(section, cancellation);

        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            await KeepAsync(database, TitleKey, section.Title, cancellation);
            await KeepAsync(database, TextKey, section.Text, cancellation);

            await database.SaveChangesAsync(cancellation);

            Hold(section);

            return section;
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Takes the operator's section out of the guide.
    /// </summary>
    public async ValueTask<GuideSection?> RemoveAsync(CancellationToken cancellation = default)
    {
        RequireOperator();

        await _turn.WaitAsync(cancellation);

        try
        {
            var was = _section;

            await using var database = await databases.CreateDbContextAsync(cancellation);

            foreach (var setting in await database.Settings.Where(s => s.Key == TitleKey || s.Key == TextKey).ToListAsync(cancellation))
            {
                database.Settings.Remove(setting);
            }

            await database.SaveChangesAsync(cancellation);

            Hold(null);

            return was;
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Refuses a section that links to a file nobody shared: agents would be
    /// sent to what is not there.
    /// </summary>
    private async ValueTask RequireSharedAsync(GuideSection section, CancellationToken cancellation)
    {
        foreach (var name in GuideLinks.Names(section.Text))
        {
            if (await shared.FindAsync(name, cancellation) == null)
            {
                throw LambdaException.Invalid($"The text links to shared/{name}, and there is no shared file called '{name}': share it first.");
            }
        }
    }

    private static async ValueTask KeepAsync(LambdaDbContext database, string key, string value, CancellationToken cancellation)
    {
        if (await database.Settings.FindAsync([key], cancellation) is { } existing)
        {
            existing.Value = value;
        }
        else
        {
            database.Settings.Add(new SettingEntity { Key = key, Value = value });
        }
    }

    private void RequireOperator()
    {
        if (!options.Administrable)
        {
            throw LambdaException.NotFound("This installation has no operator to write into its guide.");
        }
    }

    #endregion

}
