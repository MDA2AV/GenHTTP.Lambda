using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Which of the offered models a job runs on, and who may have the second one.
/// </summary>
/// <remarks>
/// The first model is everybody's, and is what the runner picks when it is
/// asked for none. The second is offered only where the operator gave it a
/// password (<see cref="LambdaOptions.AgentFablePassword"/>), and only to
/// whoever knows it.
/// </remarks>
public sealed class ModelGate(LambdaOptions options)
{

    /// <summary>Whether the second model is on offer at all.</summary>
    public bool HasSecondModel => !string.IsNullOrWhiteSpace(options.AgentFablePassword);

    /// <summary>
    /// The model asked for, where the caller may have it.
    /// </summary>
    /// <param name="model">Which of the offered models to use; the default one when left out</param>
    /// <param name="password">The password for the second model, where one was asked for</param>
    /// <returns>The model as the runner is told it, empty for the default one</returns>
    public string Choose(string? model, string? password)
    {
        var wantedModel = (model ?? "").Trim().ToLowerInvariant();

        if (wantedModel is not ("" or "opus" or "fable"))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "There is no such model here.");
        }

        if (wantedModel == "fable")
        {
            /*
             * Compared in full rather than short-circuiting on the first wrong
             * character. It is a soft gate rather than a secret, but a
             * comparison that returns faster for a closer guess is one anybody
             * can walk a character at a time, and constant time costs nothing
             * here.
             */
            var expected = options.AgentFablePassword;

            if (string.IsNullOrWhiteSpace(expected) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expected),
                    Encoding.UTF8.GetBytes(password ?? "")))
            {
                throw new ProviderException(ResponseStatus.Forbidden,
                                            "That password is not right.");
            }
        }

        return wantedModel;
    }

}
