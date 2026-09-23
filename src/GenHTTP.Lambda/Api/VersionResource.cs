using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The saved versions of a lambda's code.
/// </summary>
/// <remarks>
/// Versions are never changed once they are stored and never removed one by
/// one, so there is nothing to put or delete here: a change to the code is a
/// new version, and the history goes with the lambda.
/// </remarks>
public sealed class VersionResource(IMetaService meta)
{

    /// <summary>
    /// Lists the stored versions, newest first.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions")]
    public async ValueTask<List<VersionResponse>> List(string privateKey)
    {
        var versions = await meta.GetVersionsAsync(privateKey);

        return versions.Select(v => new VersionResponse(v.Version, v.Created)).ToList();
    }

    /// <summary>
    /// Reads the code of a single version.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/versions/:version")]
    public async ValueTask<VersionContentResponse> Get(string privateKey, int version)
    {
        var content = await meta.GetVersionAsync(privateKey, version);

        return new VersionContentResponse(content.Version, content.Created, LambdaSource.Parse(content.Code));
    }

    /// <summary>
    /// Stores the code as a new version, without putting it online.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/versions")]
    public async ValueTask<Result<VersionResponse>> Create(string privateKey, VersionRequest request)
    {
        var version = await meta.SaveAsync(privateKey, Serialize(request.Files));

        return new Result<VersionResponse>(new VersionResponse(version.Version, version.Created)).Status(ResponseStatus.Created);
    }

    /// <summary>
    /// Turns what was submitted into the single blob a version is stored as.
    /// </summary>
    internal static string Serialize(IReadOnlyList<LambdaFile>? files)
    {
        if (LambdaSource.Validate(files) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        return LambdaSource.Serialize(files!);
    }

}
