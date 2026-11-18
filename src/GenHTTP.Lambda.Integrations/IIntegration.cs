namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// A piece of infrastructure the platform keeps something in: the database,
/// the files, the secrets.
/// </summary>
/// <remarks>
/// An integration does what its infrastructure does - open a connection, write
/// a file, seal a value - and decides nothing about it: whether a lambda may
/// have a database, how much room it gets, who may read a secret, is the
/// platform's, which asks the integration only once it has decided. That is
/// what keeps one exchangeable for another without the rules changing with it.
///
/// Each kind has one at a time per installation, chosen when the platform is
/// built (<c>LambdaPlatform.Create().Database(...)</c>) and the platform's own
/// where none is chosen. Moving what one holds into another is not something
/// the platform does.
/// </remarks>
public interface IIntegration
{

    /// <summary>
    /// What the operator is shown about it: which it is and where it keeps
    /// what it keeps - never a password, a key or a connection string.
    /// </summary>
    IntegrationInfo Describe();

    /// <summary>
    /// What an agent building a lambda here needs to know about it beyond
    /// what the platform's guide says, added at the end of the section of
    /// its kind (database, workspace, secrets) - or nothing.
    /// </summary>
    /// <remarks>
    /// For what differs from one installation to the next and changes what an
    /// agent writes, such as what the room of a database means here. The code
    /// of a lambda does not depend on it: the platform hands every lambda the
    /// same API whatever keeps its data.
    /// </remarks>
    string? Guide(GuideContext context);

}
