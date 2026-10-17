using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Kinds that come later are listed here the same way, each switched on
/// before the lambda can use it.
/// </remarks>
[Tag(ApiDocumentation.Data)]
[Summary("What a lambda keeps: which kinds of data it has, and switching them on and off.")]
[Remarks("""
         A version is the program and is replaced by the next one. Data is what the program keeps, shared by every version
         and left alone by deploying, rolling back and merging. There are three kinds:

         - `database`: a SQLite database, which the code reaches with `Database.GetConnection()`. Off until it is switched
           on. Its tables are under `…/database`.
         - `workspace`: files the code reads and writes. On unless its owner switched it off. They are under `…/files`.
         - `secrets`: API keys and passwords the code reads with `Secret.Read("NAME")`, which nobody else can read. Off
           until they are switched on. Their names are under `…/secrets`.

         A kind is switched on before the lambda can use it, and the lambda can use it from its next request on, without
         being deployed again. **Switching a kind off deletes everything it held**, the copies of the lambda's features
         included, and cannot be undone. Code that uses a kind while it is off fails with an exception that says so.

         The database and the workspace share one allowance, the data allowance of the lambda's tier.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class DataResource(IDataService data, IMetaService meta, ILogger<DataResource> logger)
{

    [ResourceMethod("lambdas/:privateKey/data")]
    [Summary("Lists the kinds of data")]
    [return: Summary("Every kind of data")]
    public List<DataStoreResponse> List([Summary(ApiDocumentation.PrivateKey)] string privateKey)
        => [.. (data.List(privateKey)).Select(Describe)];

    [ResourceMethod("lambdas/:privateKey/data/:kind")]
    [Summary("Reads one kind of data")]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or there is no such kind of data.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The kind of data")]
    public DataStoreResponse Get([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                 [Summary("Which kind: `database`, `workspace` or `secrets`")] string kind)
        => Describe(data.Get(privateKey, kind));

    [ResourceMethod(Method.Put, "lambdas/:privateKey/data/:kind")]
    [Summary("Switches a kind of data on")]
    [Remarks("""
             Nothing happens if it is on already. The lambda can use it from its next request on, without being deployed again.

             The database is made empty, and the lambda is started again on its next request, so what it does as it starts -
             applying its migrations - runs against it. The secrets start empty: their values are entered with
             `PUT …/secrets/{name}`.
             """)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or there is no such kind of data.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The kind of data, switched on")]
    public DataStoreResponse Enable([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                    [Summary("Which kind: `database`, `workspace` or `secrets`")] string kind)
    {
        var store = data.Enable(privateKey, kind);

        logger.LogInformation("Enabled {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return Describe(store);
    }

    [ResourceMethod(Method.Delete, "lambdas/:privateKey/data/:kind")]
    [Summary("Switches a kind of data off and deletes it")]
    [Remarks("""
             **This cannot be undone.** Every table, file or secret of that kind is deleted, the copies of the lambda's features
             included. From its next request on, code that uses it fails with an exception that says it is off. `GET …/summary`
             says whether the code of the lambda uses each kind (`usesDatabase`, `usesWorkspace`, `missingSecrets`), before
             switching one off. Nothing happens if it is off already.
             """)]
    [Response(ResponseStatus.NotFound, "No lambda has this editor key, or there is no such kind of data.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [Response(ResponseStatus.Forbidden, ApiDocumentation.Demo, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("The kind of data, switched off and empty")]
    public DataStoreResponse Disable([Summary(ApiDocumentation.PrivateKey)] string privateKey,
                                     [Summary("Which kind: `database`, `workspace` or `secrets`")] string kind)
    {
        var store = data.Disable(privateKey, kind);

        logger.LogInformation("Disabled and deleted {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return Describe(store);
    }

    internal static DataStoreResponse Describe(DataStoreInfo store)
        => new(store.Kind, store.Enabled, store.Default, store.Changed, store.Items, store.UsedBytes, store.QuotaBytes, store.MaxItems);

}
