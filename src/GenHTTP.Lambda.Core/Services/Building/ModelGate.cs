using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Which model and effort a job runs on, and who may have the models behind
/// the password.
/// </summary>
/// <remarks>
/// What is offered is the operator's (<see cref="AgentSettingsStore"/>); the
/// password is the installation's (<see cref="LambdaOptions.AgentPassword"/>).
/// A model behind the password is offered only where there is one - one the
/// operator put behind it before setting a password is offered to nobody,
/// never to everybody.
///
/// Only a job whose password was checked here runs without a clock and a
/// limit on its steps, so taking a model from behind the password puts it on
/// the clock like every other: whatever the runner is told about a job, this
/// is the one place that decides it.
/// </remarks>
public sealed class ModelGate(AgentSettingsStore settings, LambdaOptions options)
{

    /// <summary>Whether the installation has a password for the models behind one.</summary>
    public bool PasswordSet => !string.IsNullOrWhiteSpace(options.AgentPassword);

    /// <summary>
    /// The models and efforts a page may offer, with what it chooses where
    /// nobody chooses.
    /// </summary>
    public AgentOffer Offer()
    {
        var current = settings.Get();

        var models = AgentCatalog.Models
                                 .Where(m => Offered(current.AccessOf(m.Model)))
                                 .Select(m => new OfferedModel(m.Name, m.Label, current.AccessOf(m.Model) == ModelAccess.Password))
                                 .ToList();

        return new AgentOffer(models, AgentCatalog.NameOf(current.DefaultModel),
                              [.. current.Efforts.Select(AgentCatalog.NameOf)], AgentCatalog.NameOf(current.DefaultEffort));
    }

    /// <summary>
    /// The model and the effort asked for, where the caller may have them.
    /// </summary>
    /// <param name="model">Which of the offered models to use; the default one when left out</param>
    /// <param name="effort">Which of the offered efforts to run at; the default one when left out</param>
    /// <param name="password">The password, where a model behind it was asked for</param>
    /// <exception cref="ProviderException">
    /// There is no such model or effort here, or it is not offered (400); the
    /// password is not right (403)
    /// </exception>
    public AgentChoice Choose(string? model, string? effort, string? password)
    {
        var current = settings.Get();

        var chosenModel = current.DefaultModel;

        if (!string.IsNullOrWhiteSpace(model))
        {
            if (!AgentCatalog.TryModel(model, out chosenModel))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "There is no such model here.");
            }

            if (!Offered(current.AccessOf(chosenModel)))
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"{AgentCatalog.LabelOf(chosenModel)} is not offered here.");
            }
        }

        var chosenEffort = current.DefaultEffort;

        if (!string.IsNullOrWhiteSpace(effort))
        {
            if (!AgentCatalog.TryEffort(effort, out chosenEffort))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "There is no such effort here.");
            }

            if (!current.Efforts.Contains(chosenEffort))
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"The effort '{AgentCatalog.NameOf(chosenEffort)}' is not offered here.");
            }
        }

        var guarded = current.AccessOf(chosenModel) == ModelAccess.Password;

        if (guarded && !Matches(password))
        {
            throw new ProviderException(ResponseStatus.Forbidden, "That password is not right.");
        }

        return new AgentChoice(chosenModel, chosenEffort, Unbounded: guarded);
    }

    /// <summary>Whether a model with the given access is on offer at all.</summary>
    private bool Offered(ModelAccess access)
        => access == ModelAccess.Open || (access == ModelAccess.Password && PasswordSet);

    /// <remarks>
    /// Compared in full rather than short-circuiting on the first wrong
    /// character. It is a soft gate rather than a secret, but a comparison
    /// that returns faster for a closer guess is one anybody can walk a
    /// character at a time, and constant time costs nothing here.
    /// </remarks>
    private bool Matches(string? password)
    {
        var expected = options.AgentPassword;

        return !string.IsNullOrWhiteSpace(expected)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(password ?? ""));
    }

}
