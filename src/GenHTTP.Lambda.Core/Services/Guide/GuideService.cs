using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Settings;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// The guide agents read first, and the section its operator writes into it.
/// </summary>
/// <remarks>
/// The document is <see cref="GuideDocument"/>'s and what a section may be
/// <see cref="GuideChecks"/>'. The section is kept as two settings
/// (<c>guide.title</c>, <c>guide.text</c>) and held in memory once read, as
/// the branding is; the guide is written anew with every call, since it is
/// called rarely and differs by the address it was asked at.
///
/// Where the installation has no admin token, a section in the database is
/// not read: a copy of another installation's data would otherwise hand
/// agents rules nobody here can see or take away.
/// </remarks>
public sealed class GuideService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options, ILimitsService tiers, ILambdaAddresses addresses,
                                 IBrandingService branding) : IGuideService
{
    private const string TitleKey = "guide.title";

    private const string TextKey = "guide.text";

    private readonly GuideDocument _document = new(options, tiers, addresses);

    private readonly Lock _lock = new();

    private bool _read;

    private GuideSection? _section;

    #region Reading

    /// <summary>
    /// The operator's section, if there is one.
    /// </summary>
    public GuideSection? Section
    {
        get
        {
            if (!options.Administrable)
            {
                return null;
            }

            lock (_lock)
            {
                return Current();
            }
        }
    }

    /// <summary>
    /// The guide as an agent reads it now.
    /// </summary>
    public string Write(string origin) => _document.Write(origin, branding.Current.Name, Section);

    /// <remarks>Under the lock.</remarks>
    private GuideSection? Current()
    {
        if (_read)
        {
            return _section;
        }

        using var database = databases.CreateDbContext();

        var stored = database.Settings.AsNoTracking()
                             .Where(s => s.Key == TitleKey || s.Key == TextKey)
                             .ToDictionary(s => s.Key, s => s.Value);

        _section = stored.TryGetValue(TitleKey, out var title) && stored.TryGetValue(TextKey, out var text) ? new GuideSection(title, text) : null;
        _read = true;

        return _section;
    }

    #endregion

    #region Changing

    /// <summary>
    /// Writes the operator's section, or replaces it.
    /// </summary>
    public GuideSection Change(string? title, string? text)
    {
        RequireOperator();

        var section = GuideChecks.Section(title, text, _document.Headings);

        lock (_lock)
        {
            using var database = databases.CreateDbContext();

            Keep(database, TitleKey, section.Title);
            Keep(database, TextKey, section.Text);

            database.SaveChanges();

            _section = section;
            _read = true;

            return section;
        }
    }

    /// <summary>
    /// Takes the operator's section out of the guide.
    /// </summary>
    public GuideSection? Remove()
    {
        RequireOperator();

        lock (_lock)
        {
            var was = Current();

            using var database = databases.CreateDbContext();

            foreach (var setting in database.Settings.Where(s => s.Key == TitleKey || s.Key == TextKey).ToList())
            {
                database.Settings.Remove(setting);
            }

            database.SaveChanges();

            _section = null;
            _read = true;

            return was;
        }
    }

    private static void Keep(LambdaDbContext database, string key, string value)
    {
        if (database.Settings.Find(key) is { } existing)
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
