using System.Reflection;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Deployment.Model;

using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Looks over a prepared handler tree for endpoints that GenHTTP could not
/// generate invocation code for.
/// </summary>
/// <remarks>
/// GenHTTP compiles a small invoker per operation while preparing and keeps a
/// failure to itself until the endpoint is first called, which would leave the
/// author of a lambda with a broken route and no idea why. Catching it here
/// means the deployment is refused with an explanation instead. The one that
/// actually happens is returning an anonymous object, whose type has no name
/// the generated code could use.
/// </remarks>
internal static class HandlerInspector
{
    private static readonly FieldInfo? CompilationError = typeof(MethodHandler)
        .GetField("_compilationError", BindingFlags.Instance | BindingFlags.NonPublic);

    #region Functionality

    /// <summary>
    /// Returns a message for every operation that will not be able to answer.
    /// </summary>
    internal static IReadOnlyList<CompilationDiagnostic> Inspect(IHandler handler)
    {
        if (CompilationError == null)
        {
            // the field was renamed in this version of GenHTTP; the endpoint
            // still reports the problem itself when it is called
            return [];
        }

        var findings = new List<CompilationDiagnostic>();

        Walk(handler, findings, 0);

        return findings;
    }

    private static void Walk(IHandler? handler, List<CompilationDiagnostic> findings, int depth)
    {
        // guards against a handler chain that refers back to itself
        if (handler == null || depth > 32)
        {
            return;
        }

        switch (handler)
        {
            case IConcern concern:
                Walk(concern.Content, findings, depth + 1);
                break;

            case LayoutHandler layout:
                foreach (var routed in layout.RoutedHandlers.Values)
                {
                    Walk(routed, findings, depth + 1);
                }

                foreach (var root in layout.RootHandlers)
                {
                    Walk(root, findings, depth + 1);
                }

                Walk(layout.Index, findings, depth + 1);
                break;

            case MethodCollection collection:
                Inspect(collection, findings);
                break;

            case IServiceMethodProvider provider:
                Inspect(provider.Methods, findings);
                break;
        }
    }

    private static void Inspect(MethodCollection collection, List<CompilationDiagnostic> findings)
    {
        foreach (var method in collection.Methods)
        {
            if (CompilationError!.GetValue(method) == null)
            {
                continue;
            }

            var route = method.Operation.Route.Name;

            findings.Add(new CompilationDiagnostic("Error", "LAMBDA0002", Explain(method, route), 0, 0));
        }
    }

    private static string Explain(MethodHandler method, string route)
    {
        var result = Unwrap(method.Operation.Result.Type);

        if (IsAnonymous(result))
        {
            return $"The route '{route}' returns an anonymous type, which cannot be served. Declare a record (for example 'record Message(string Text);') and return that instead.";
        }

        return $"The route '{route}' returns '{result.Name}', which GenHTTP cannot generate an endpoint for.";
    }

    private static Type Unwrap(Type type)
        => type.IsGenericType && type.GenericTypeArguments.Length == 1 && type.IsAsyncGeneric() ? type.GenericTypeArguments[0] : type;

    private static bool IsAnonymous(Type type)
        => type.IsDefined(typeof(CompilerGeneratedAttribute), false) && type.Name.Contains("AnonymousType", StringComparison.Ordinal);

    #endregion

}
