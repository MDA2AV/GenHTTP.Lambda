namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// The data of a lambda: which kinds it keeps, and switching them on and off.
/// </summary>
/// <remarks>
/// What is in a kind of data is reached through the service for that kind -
/// the files of the workspace through the workspace service, the tables of the
/// database through the database service. This is the part
/// every kind shares, so that the editor, the API and an agent can list the
/// data of a lambda without knowing each kind there is.
/// </remarks>
public interface IDataService
{

    /// <summary>
    /// Every kind of data there is, as the lambda has it - or, where a feature
    /// is named, as that feature's copy of it is.
    /// </summary>
    IReadOnlyList<DataStoreInfo> List(string privateKey, string? feature = null);

    /// <summary>
    /// One kind of data, as the lambda has it.
    /// </summary>
    DataStoreInfo Get(string privateKey, string kind);

    /// <summary>
    /// Switches a kind of data on. Nothing happens if it is on already.
    /// </summary>
    DataStoreInfo Enable(string privateKey, string kind);

    /// <summary>
    /// Switches a kind of data off, deleting everything it held - the copies
    /// the features of the lambda have of it included.
    /// </summary>
    /// <remarks>
    /// Deleting is the point rather than a side effect: a kind of data that is
    /// off holds nothing, so there is no data nobody can see left lying about,
    /// and switching it on again starts empty.
    /// </remarks>
    DataStoreInfo Disable(string privateKey, string kind);

}
