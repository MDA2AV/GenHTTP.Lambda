namespace GenHTTP.Lambda.Infrastructure;

/// <summary>
/// Hands what is awaited to code that calls it with a plain call: the
/// synchronous members a compiled lambda keeps beside the asynchronous ones
/// - <c>Database.GetConnection()</c>, <c>Workspace.ReadText</c>,
/// <c>Secret.Read</c> - so code written before they had an asynchronous form
/// keeps working.
/// </summary>
/// <remarks>
/// <para>
/// What a lambda reads and writes goes through the integrations, whose
/// contracts are asynchronous. Where the integration answers at once - SQLite,
/// local directories, the platform's own secrets while its records are in
/// SQLite - the task it returns has
/// completed, and this hands over its result on the thread that asked,
/// waiting for nothing. Where it does not - a database server, object
/// storage - this blocks the calling thread until it has, which costs a thread
/// of the pool on Kestrel and the internal engine, and holds every connection
/// of its core on Ioxide: the reason lambdas are taught the asynchronous form.
/// </para>
/// <para>
/// The synchronization context is taken off the thread before the work
/// starts, so whatever it awaits resumes on the pool rather than being posted
/// back to the thread that is blocked waiting for it - on Ioxide the reactor,
/// which would never run it, and its core would be deadlocked for good. That
/// is why it takes the work rather than a task: a task started on the
/// reactor has captured it already.
/// </para>
/// <para>
/// For the functions handed to a compiled lambda only. The platform's own
/// code awaits, and the code guard refuses a lambda's own waits.
/// </para>
/// </remarks>
public static class Synchronous
{

    public static T Run<T>(Func<ValueTask<T>> work)
    {
        var context = SynchronizationContext.Current;

        SynchronizationContext.SetSynchronizationContext(null);

        try
        {
            var pending = work();

#pragma warning disable RS0030 // the one place a lambda's plain call waits, with the context taken off (see above)
            return pending.IsCompleted ? pending.Result : pending.AsTask().GetAwaiter().GetResult();
#pragma warning restore RS0030
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }

    public static void Run(Func<ValueTask> work)
    {
        var context = SynchronizationContext.Current;

        SynchronizationContext.SetSynchronizationContext(null);

        try
        {
            var pending = work();

#pragma warning disable RS0030 // the one place a lambda's plain call waits, with the context taken off (see above)
            if (pending.IsCompleted)
            {
                pending.GetAwaiter().GetResult();
            }
            else
            {
                pending.AsTask().GetAwaiter().GetResult();
            }
#pragma warning restore RS0030
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }

}
