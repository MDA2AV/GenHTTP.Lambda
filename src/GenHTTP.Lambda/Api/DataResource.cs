using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Data;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The data of a lambda: which kinds it keeps, and switching them on and off.
/// </summary>
/// <remarks>
/// A version is the program - its code and assets - and is replaced by the
/// next one; data is what the program keeps, shared by every version and left
/// alone by deploying and rolling back. There are two kinds: the workspace,
/// which is on unless its owner switched it off, with its files under
/// <c>/lambdas/{privateKey}/files</c>, and the secrets - tokens and credentials
/// - which are off until switched on, under <c>/lambdas/{privateKey}/secrets</c>.
/// Kinds that come later are listed here the same way.
/// </remarks>
public sealed class DataResource(IDataService data)
{

    /// <summary>
    /// Every kind of data there is, and how the lambda has it.
    /// </summary>
    [ResourceMethod("lambdas/:privateKey/data")]
    public async ValueTask<List<DataStoreResponse>> List(string privateKey)
        => [.. (await data.ListAsync(privateKey)).Select(Describe)];

    /// <summary>
    /// One kind of data, and how the lambda has it.
    /// </summary>
    /// <param name="kind">Which kind: <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod("lambdas/:privateKey/data/:kind")]
    public async ValueTask<DataStoreResponse> Get(string privateKey, string kind)
        => Describe(await data.GetAsync(privateKey, kind));

    /// <summary>
    /// Switches a kind of data on.
    /// </summary>
    /// <remarks>
    /// Nothing happens if it is on already. The lambda can use it from its
    /// next request on, without being deployed again.
    /// </remarks>
    /// <param name="kind">Which kind: <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod(Method.Put, "lambdas/:privateKey/data/:kind")]
    public async ValueTask<DataStoreResponse> Enable(string privateKey, string kind)
        => Describe(await data.EnableAsync(privateKey, kind));

    /// <summary>
    /// Switches a kind of data off, and deletes everything it held.
    /// </summary>
    /// <remarks>
    /// This cannot be undone. From its next request on, code that uses it is
    /// refused with an exception saying that it is off.
    /// </remarks>
    /// <param name="kind">Which kind: <c>workspace</c> or <c>secrets</c></param>
    [ResourceMethod(Method.Delete, "lambdas/:privateKey/data/:kind")]
    public async ValueTask<DataStoreResponse> Disable(string privateKey, string kind)
        => Describe(await data.DisableAsync(privateKey, kind));

    internal static DataStoreResponse Describe(DataStoreInfo store)
        => new(store.Kind, store.Enabled, store.Default, store.Changed, store.Items, store.UsedBytes, store.QuotaBytes, store.Limit);

}
