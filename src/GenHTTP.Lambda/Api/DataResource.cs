using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Data;
using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The data of a lambda: which kinds it keeps, and switching them on and off.
/// </summary>
/// <remarks>
/// A version is the program - its code and assets - and is replaced by the
/// next one; data is what the program keeps, shared by every version and left
/// alone by deploying and rolling back. The database is off until it is
/// switched on, which makes it; its tables are under
/// <c>/lambdas/{privateKey}/database</c>. The workspace is on unless its owner
/// switched it off; its files are under <c>/lambdas/{privateKey}/files</c>.
/// The secrets are off until they are switched on; their names are under
/// <c>/lambdas/{privateKey}/secrets</c>. Kinds that come later are listed here
/// the same way, each switched on before the lambda can use it.
/// </remarks>
public sealed class DataResource(IDataService data, IMetaService meta, ILogger<DataResource> logger)
{

    /// <summary>
    /// Every kind of data there is, and how the lambda has it.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/data")]
    public List<DataStoreResponse> List(string privateKey)
        => [.. (data.List(privateKey)).Select(Describe)];

    /// <summary>
    /// One kind of data, and how the lambda has it.
    /// </summary>
    /// <param name="kind">Which kind: <c>database</c>, <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod("lambdas/:privateKey/data/:kind")]
    public DataStoreResponse Get(string privateKey, string kind)
        => Describe(data.Get(privateKey, kind));

    /// <summary>
    /// Switches a kind of data on.
    /// </summary>
    /// <remarks>
    /// Nothing happens if it is on already. The lambda can use it from its
    /// next request on, without being deployed again. The database is made
    /// empty, and the lambda is started again on its next request, so what it
    /// does with its database as it starts - migrating it - is done. The
    /// secrets start empty: their values are set under
    /// <c>/lambdas/{privateKey}/secrets</c>.
    /// </remarks>
    /// <param name="kind">Which kind: <c>database</c>, <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/data/:kind")]
    public DataStoreResponse Enable(string privateKey, string kind)
    {
        var store = data.Enable(privateKey, kind);

        logger.LogInformation("Enabled {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return Describe(store);
    }

    /// <summary>
    /// Switches a kind of data off, and deletes everything it held.
    /// </summary>
    /// <remarks>
    /// This cannot be undone. From its next request on, code that uses it is
    /// refused with an exception saying that it is off.
    /// </remarks>
    /// <param name="kind">Which kind: <c>database</c>, <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/data/:kind")]
    public DataStoreResponse Disable(string privateKey, string kind)
    {
        var store = data.Disable(privateKey, kind);

        logger.LogInformation("Disabled and deleted {Kind} of lambda {Lambda}", store.Kind, meta.PublicKeyOf(privateKey));

        return Describe(store);
    }

    internal static DataStoreResponse Describe(DataStoreInfo store)
        => new(store.Kind, store.Enabled, store.Default, store.Changed, store.Items, store.UsedBytes, store.QuotaBytes, store.MaxItems);

}
