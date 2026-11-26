using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a run may be asked for on: which values of the agent's options are
/// offered and to whom, what it takes where nobody chose, and what it may
/// take - what the operator decided, over what the integration offers now.
/// </summary>
/// <remarks>
/// <para>
/// The integration says what there is (<see cref="AgentCapabilities.Options"/>),
/// the operator who may have it (<see cref="AgentSettingsStore"/>), the
/// installation the password (<see cref="LambdaOptions.AgentPassword"/>). A
/// value behind the password is offered only where there is one - one the
/// operator put behind it before setting a password is offered to nobody,
/// never to everybody.
/// </para>
/// <para>
/// Only a run whose password was checked here is held to the limits of runs
/// behind it, so taking a value from behind the password puts it on the
/// limits of every other: whatever the agent is told about a run, this is
/// the one place that decides it.
/// </para>
/// </remarks>
public sealed class AgentGate(AgentSettingsStore settings, LambdaOptions options)
{

    /// <summary>Whether the installation has a password for the values behind one.</summary>
    public bool PasswordSet => !string.IsNullOrWhiteSpace(options.AgentPassword);

    #region Offering

    /// <summary>
    /// Every option as the pages offer it: the values that may be chosen, in
    /// the integration's order, and the one a run takes where nobody chose.
    /// </summary>
    public IReadOnlyList<OfferedOption> Offer(AgentCapabilities capabilities)
    {
        var current = settings.Get();

        return [.. capabilities.Options.Select(option => new OfferedOption(option.Name, option.Label,
                    [.. option.Values.Where(v => Offered(current.AccessOf(option, v)))
                                     .Select(v => new OfferedValue(v.Name, v.Label, current.AccessOf(option, v) == OptionAccess.Password))],
                    DefaultOf(current, option)))];
    }

    /// <summary>
    /// The value a run takes where nobody chose: the operator's, where it is
    /// offered to everybody - otherwise the integration's, or the first value
    /// everybody may have.
    /// </summary>
    public static string DefaultOf(AgentSettings current, AgentOption option)
    {
        bool Open(string? name) => option.Value(name ?? "") is { } value && current.AccessOf(option, value) == OptionAccess.Open;

        if (current.Defaults.TryGetValue(option.Name, out var chosen) && Open(chosen))
        {
            return chosen;
        }

        if (Open(option.Default))
        {
            return option.Default;
        }

        return option.Values.FirstOrDefault(v => current.AccessOf(option, v) == OptionAccess.Open)?.Name ?? option.Default;
    }

    /// <summary>
    /// What a run may take: what the operator set, and where they set nothing,
    /// what the agent holds a run to - or, behind the password, nothing.
    /// </summary>
    public AgentLimits LimitsOf(AgentCapabilities capabilities, bool guarded)
    {
        var current = settings.Get();

        var set = guarded ? current.GuardedLimits : current.Limits;

        var fallback = guarded ? new AgentLimits(null, null) : capabilities.Limits.Default;

        TimeSpan? time = set.Seconds switch
        {
            null => fallback.Time,
            0 => null,
            var seconds => TimeSpan.FromSeconds(seconds.Value)
        };

        int? steps = set.Steps switch
        {
            null => fallback.Steps,
            0 => null,
            var counted => counted
        };

        // what the agent cannot be told it holds a run to by default - or,
        // behind the password, to nothing, which every agent can do
        return new AgentLimits(capabilities.Limits.Time ? time : guarded ? null : capabilities.Limits.Default.Time,
                               capabilities.Limits.Steps ? steps : guarded ? null : capabilities.Limits.Default.Steps);
    }

    #endregion

    #region Choosing

    /// <summary>
    /// The values asked for, where the caller may have them, with what the
    /// run may take.
    /// </summary>
    /// <param name="asked">A value for each option asked for by name; an option left out takes its default</param>
    /// <param name="password">The password, where a value behind it was asked for</param>
    /// <exception cref="ProviderException">
    /// There is no such option or value here, or it is not offered (400); the
    /// password is not right (403)
    /// </exception>
    public AgentChoice Choose(AgentCapabilities capabilities, IReadOnlyDictionary<string, string?> asked, string? password)
    {
        var current = settings.Get();

        foreach (var name in asked.Keys)
        {
            if (capabilities.Option(name) == null)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"There is no option '{name}' here.");
            }
        }

        var chosen = new SortedDictionary<string, string>(StringComparer.Ordinal);

        var guarded = false;

        foreach (var option in capabilities.Options)
        {
            var name = asked.TryGetValue(option.Name, out var wanted) && !string.IsNullOrWhiteSpace(wanted) ? wanted.Trim().ToLowerInvariant() : null;

            // nothing asked: the default, which is everybody's - checked like
            // any other value, since the operator may have taken every value
            // of an option from everybody
            name ??= DefaultOf(current, option);

            var value = option.Value(name) ?? throw new ProviderException(ResponseStatus.BadRequest, $"There is no such {option.Label.ToLowerInvariant()} here.");

            var access = current.AccessOf(option, value);

            if (!Offered(access))
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"{value.Label} is not offered here.");
            }

            guarded |= access == OptionAccess.Password;

            chosen[option.Name] = value.Name;
        }

        if (guarded && !Matches(password))
        {
            throw new ProviderException(ResponseStatus.Forbidden, "That password is not right.");
        }

        return new AgentChoice(chosen, guarded, LimitsOf(capabilities, guarded));
    }

    /// <summary>Whether a value with the given access is on offer at all.</summary>
    private bool Offered(OptionAccess access)
        => access == OptionAccess.Open || (access == OptionAccess.Password && PasswordSet);

    /// <remarks>
    /// Compared in full rather than short-circuiting on the first wrong
    /// character: a comparison that returns faster for a closer guess is one
    /// anybody can walk a character at a time, and constant time costs nothing.
    /// </remarks>
    private bool Matches(string? password)
    {
        var expected = options.AgentPassword;

        return !string.IsNullOrWhiteSpace(expected)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(password ?? ""));
    }

    #endregion

    #region Deciding

    /// <summary>
    /// These settings with what a caller changed, checked against what the
    /// integration offers - or a refusal that says what cannot be kept.
    /// </summary>
    /// <remarks>
    /// The default of an option is what a run takes where nobody chose, so it
    /// is the one everybody gets: offered, and not behind the password.
    /// Whoever means to offer the agent to nobody switches building and
    /// changing off. What is decided about an option the integration does
    /// not offer now is kept as it was.
    /// </remarks>
    /// <exception cref="LambdaException">A name there is none of, or settings that would leave an option with nothing to start on</exception>
    public static AgentSettings Change(AgentSettings current, AgentCapabilities? capabilities, AgentSettingsChange change)
    {
        var access = current.Access.ToDictionary(a => a.Key, a => a.Value.ToDictionary(v => v.Key, v => v.Value, StringComparer.Ordinal), StringComparer.Ordinal);
        var defaults = new Dictionary<string, string>(current.Defaults, StringComparer.Ordinal);

        foreach (var (name, values) in change.Access ?? new Dictionary<string, IReadOnlyDictionary<string, string>>())
        {
            var option = capabilities?.Option(name) ?? throw LambdaException.Invalid($"There is no option '{name}' here.");

            if (!access.TryGetValue(name, out var decided))
            {
                access[name] = decided = new Dictionary<string, OptionAccess>(StringComparer.Ordinal);
            }

            foreach (var (value, text) in values)
            {
                if (option.Value(value) == null)
                {
                    throw LambdaException.Invalid($"There is no {option.Label.ToLowerInvariant()} '{value}' here.");
                }

                if (!OptionAccessNames.TryParse(text, out var parsed))
                {
                    throw LambdaException.Invalid($"Who may choose a value is 'open', 'password' or 'off', not '{text}'.");
                }

                decided[value] = parsed;
            }
        }

        foreach (var (name, value) in change.Defaults ?? new Dictionary<string, string>())
        {
            var option = capabilities?.Option(name) ?? throw LambdaException.Invalid($"There is no option '{name}' here.");

            if (option.Value(value) == null)
            {
                throw LambdaException.Invalid($"There is no {option.Label.ToLowerInvariant()} '{value}' here.");
            }

            defaults[name] = value;
        }

        var changed = new AgentSettings(change.Builds ?? current.Builds, change.Changes ?? current.Changes,
                                        access.ToDictionary(a => a.Key, a => (IReadOnlyDictionary<string, OptionAccess>)a.Value, StringComparer.Ordinal),
                                        defaults, Limit(change.Limits, current.Limits), Limit(change.GuardedLimits, current.GuardedLimits));

        foreach (var option in capabilities?.Options ?? [])
        {
            if (!change.Touches(option.Name))
            {
                continue;
            }

            var start = changed.Defaults.TryGetValue(option.Name, out var picked) ? option.Value(picked) : option.Value(option.Default);

            if (start == null || changed.AccessOf(option, start) != OptionAccess.Open)
            {
                throw LambdaException.Invalid($"The default {option.Label.ToLowerInvariant()} is what everybody gets, so {start?.Label ?? option.Default} has to be offered to everybody, without the password.");
            }
        }

        return changed;
    }

    private static LimitSetting Limit(LimitSetting? change, LimitSetting current)
    {
        if (change == null)
        {
            return current;
        }

        if (change.Seconds is < 0 or > 7 * 24 * 3600 || change.Steps is < 0 or > 100_000)
        {
            throw LambdaException.Invalid("A limit is a number of seconds or steps, zero for none.");
        }

        return change;
    }

    #endregion

}
