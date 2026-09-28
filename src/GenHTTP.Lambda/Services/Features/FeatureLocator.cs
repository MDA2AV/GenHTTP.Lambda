using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Execution;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Protection;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// Finds the preview of a feature by the key in the first segment of the
/// path, the way every preview is reached below <c>/features/</c>.
/// </summary>
/// <remarks>
/// A preview behaves like the lambda in every other respect - throttled,
/// timed out and logged the same - because it is served by the same chain,
/// differing only in this lookup.
///
/// A key that is not online is answered with a plain page rather than the
/// platform's: the preview's address is handed to somebody to try a change,
/// and a page of this site there would only confuse them.
/// </remarks>
public sealed class FeatureLocator(IFeatureService features) : ILambdaLocator
{

    public async ValueTask<ResolvedLambda?> LocateAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current is not { } segment)
        {
            return null;
        }

        var lambda = await features.ResolvePreviewAsync(segment.Decode());

        if (lambda != null)
        {
            // the preview routes on what comes after its key
            target.Advance();
        }

        return lambda;
    }

    public ValueTask<IResponse> UnavailableAsync(IRequest request)
        => new(LambdaErrorMapper.Render(request, request.Header.Headers.GetEntry("Accept"), ResponseStatus.NotFound, "Not online",
                                        "This preview is not online. It may have been merged, deleted or taken offline.", null));

}
