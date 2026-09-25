using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// Finds the lambda a request is for and puts it into the request, so the
/// handler below only has to run it.
/// </summary>
/// <remarks>
/// How it is found is up to the locator - by the key in the path, or by the
/// domain the request was addressed to. Everything around this concern is the
/// same either way, which is what makes a lambda behave identically at both.
///
/// Requests for a lambda that does not exist (or is not deployed) never reach
/// the execution layer; the locator decides what they are told instead.
/// </remarks>
public sealed class LambdaResolutionConcern(IHandler content, ILambdaLocator locator) : IConcern
{

    #region Get-/Setters

    public IHandler Content => content;

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var lambda = await locator.LocateAsync(request);

        if (lambda == null)
        {
            return await locator.UnavailableAsync(request);
        }

        request.SetLambda(lambda);

        return await content.HandleAsync(request);
    }

    #endregion

}

public sealed class LambdaResolutionConcernBuilder(ILambdaLocator locator) : IConcernBuilder
{
    public IConcern Build(IHandler content) => new LambdaResolutionConcern(content, locator);
}
