using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Showcase;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The showcase entry of one lambda, for its owner.
/// </summary>
/// <remarks>
/// Behind the editor key like everything else about a lambda, so only
/// whoever may change the lambda may put it in front of everybody. Apart from
/// the versions and the deployment on purpose: presenting a lambda is
/// something done once it works, not a step of making it work.
/// </remarks>
public sealed class LambdaShowcaseResource(IShowcaseService showcases, LambdaOptions options)
{

    /// <summary>
    /// The entry of the lambda, if it has one, and what may go into one.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/showcase")]
    public async ValueTask<OwnShowcaseResponse> Get(string privateKey)
    {
        var showcase = await showcases.GetAsync(privateKey);

        return new OwnShowcaseResponse(showcase == null ? null : ShowcaseResponse.Of(showcase), Limits(options));
    }

    /// <summary>
    /// Puts the lambda into the showcase, or changes what its entry says.
    /// </summary>
    /// <remarks>
    /// A new entry needs a picture; a changed one keeps its picture unless
    /// another is sent. The entry is only listed while the lambda is online.
    /// </remarks>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/showcase")]
    public async ValueTask<ShowcaseResponse> Put(string privateKey, ShowcaseRequest request)
    {
        byte[]? image = null;

        if (!string.IsNullOrEmpty(request.Image))
        {
            try
            {
                image = Convert.FromBase64String(request.Image);
            }
            catch (FormatException)
            {
                throw LambdaException.Invalid("The picture has to be base64 encoded.");
            }
        }

        var saved = await showcases.SaveAsync(privateKey, new ShowcaseDraft(request.Title, request.Description, image));

        return ShowcaseResponse.Of(saved);
    }

    /// <summary>
    /// Takes the lambda out of the showcase.
    /// </summary>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/showcase")]
    public async ValueTask Delete(string privateKey) => await showcases.RemoveAsync(privateKey);

    internal static ShowcaseLimitsResponse Limits(LambdaOptions options)
        => new(ShowcaseLimits.MaxTitle, ShowcaseLimits.MaxDescription, options.MaxShowcaseImageBytes,
               ["image/png", "image/jpeg", "image/gif", "image/webp"], ShowcaseLimits.Tone);

}
