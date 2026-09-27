using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Conversion.Serializers.Json;

using JsonContent = GenHTTP.Modules.Conversion.Serializers.Json.JsonContent;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Translates the errors the service layer raises into the status codes and
/// JSON bodies the single page application expects.
/// </summary>
public sealed class ApiErrorMapper : IErrorMapper<LambdaException>
{

    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, LambdaException error, ByteString? acceptedFormat)
    {
        var status = error.Error switch
        {
            LambdaError.NotFound => ResponseStatus.NotFound,
            LambdaError.Conflict => ResponseStatus.Conflict,
            LambdaError.Forbidden => ResponseStatus.Forbidden,
            _ => ResponseStatus.BadRequest
        };

        var payload = new ErrorResponse((int)status, error.Error.ToString(), error.Message);

        var response = request.Respond()
                              .Status(status)
                              .Content(new JsonContent(payload, JsonFormat.GetDefaultOptions()))
                              .Build();

        return new ValueTask<IResponse?>(response);
    }

    // anything else is left to the error handler of the server
    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler, ByteString? acceptedFormat) => new((IResponse?)null);

}
