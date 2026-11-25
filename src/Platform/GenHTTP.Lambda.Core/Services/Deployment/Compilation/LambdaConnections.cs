using System.Reflection;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// What the compiled lambdas of a platform were handed: the functions their
/// generated classes read secrets, open the database and reach the workspace
/// through.
/// </summary>
/// <remarks>
/// Those are static fields of an assembly that stays in the process for good,
/// and each function leads to the services of the platform - so a platform
/// that is gone would be held by every lambda it ever ran. Cutting the
/// connections lets go of it. A lambda that still runs something then is told
/// that its secrets, database or workspace are not available, as one that
/// was never connected is.
///
/// Kept by field, since a lambda deployed again from the same code is the same
/// assembly and is handed new functions in the same fields.
/// </remarks>
internal sealed class LambdaConnections
{
    private readonly Dictionary<FieldInfo, object> _connected = [];

    private readonly Lock _lock = new();

    #region Functionality

    /// <summary>
    /// Hands a generated class the function it reads through.
    /// </summary>
    /// <param name="type">The full name of the class, which a lambda may not have</param>
    internal void Connect(Assembly assembly, string type, string field, object source)
    {
        if (assembly.GetType(type)?.GetField(field, BindingFlags.Static | BindingFlags.NonPublic) is not { } target)
        {
            return;
        }

        lock (_lock)
        {
            target.SetValue(null, source);

            _connected[target] = source;
        }
    }

    /// <summary>
    /// Takes back everything that was handed out - but for what another
    /// platform over the same data has replaced since, which is its own.
    /// </summary>
    internal void Cut()
    {
        lock (_lock)
        {
            foreach (var (field, source) in _connected)
            {
                if (ReferenceEquals(field.GetValue(null), source))
                {
                    field.SetValue(null, null);
                }
            }

            _connected.Clear();
        }
    }

    #endregion

}
