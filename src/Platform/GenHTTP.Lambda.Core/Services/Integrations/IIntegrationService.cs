namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// Which integrations the installation keeps its data in, as its operator and
/// its usage report are told.
/// </summary>
public interface IIntegrationService
{

    /// <summary>
    /// Every integration in use - the database, the files, the secrets, in
    /// that order.
    /// </summary>
    IReadOnlyList<IntegrationState> List();

}
