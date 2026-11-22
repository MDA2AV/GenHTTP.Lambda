using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Mcp;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// Runs a tool and turns a refusal of the platform's into an answer that says
/// why, rather than a failed call.
/// </summary>
/// <remarks>
/// A <see cref="LambdaException"/> is what every service throws where it will
/// not do what it was asked, with a message written for whoever asked. An
/// agent that is told why can do better on its next call; one whose call
/// failed with an internal error cannot. Anything else is a fault of the
/// platform's, and stays one.
/// </remarks>
internal static class Refusing
{

    public static async ValueTask<object> Async(Func<ValueTask<object>> tool)
    {
        try
        {
            return await tool();
        }
        catch (LambdaException e)
        {
            return ToolResult.Error(e.Message);
        }
    }

}
