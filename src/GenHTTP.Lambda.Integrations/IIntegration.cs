namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// A piece of infrastructure the platform keeps something in: the database,
/// the files, the secrets.
/// </summary>
/// <remarks>
/// <para>
/// An integration does what its infrastructure does - open a connection, write
/// a file, seal a value - and decides nothing about it: whether a lambda may
/// have a database, how much room it gets, who may read a secret, is the
/// platform's, which asks the integration only once it has decided. That is
/// what keeps one exchangeable for another without the rules changing with it.
/// </para>
/// <para>
/// Each kind has one at a time per installation, chosen when the platform is
/// built (<c>LambdaPlatform.Create().Database(...)</c>) and the platform's own
/// where none is chosen. Moving what one holds into another is not something
/// the platform does.
/// </para>
/// <para>
/// Whatever reaches the infrastructure returns a <see cref="ValueTask"/>, so
/// an integration that waits on the network - a database server, object
/// storage, a vault - awaits it rather than holding a thread, which Kestrel
/// and GenHTTP's internal engine make use of. One that answers at once
/// completes synchronously, and costs nothing for it: SQLite, local
/// directories and the platform's own secrets, while its records are in
/// SQLite, do. That is what an
/// installation on the Ioxide engine wants, where a request runs on the
/// reactor of its core and whatever an integration awaits there resumes on
/// that reactor afterwards: an integration meant for Ioxide answers on the
/// thread it is asked on, or awaits only what the reactor completes itself.
/// What says what it is - <see cref="Describe"/>, <see cref="Guide"/> - is
/// read from memory and stays synchronous.
/// </para>
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
