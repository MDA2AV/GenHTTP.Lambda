using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Reads the view a lambda's editor is asked to open in.
/// </summary>
/// <remarks>
/// Shared by the REST API and the MCP, so both accept the same words and
/// refuse the rest with the same sentence.
/// </remarks>
public static class EditorViews
{

    /// <summary>
    /// The view that was asked for, in any case, or nothing when none was.
    /// </summary>
    public static EditorView? Parse(string? view)
    {
        if (string.IsNullOrWhiteSpace(view))
        {
            return null;
        }

        return EditorViewNames.TryRead(view, out var parsed)
            ? parsed
            : throw LambdaException.Invalid($"There is no view called '{view.Trim()}'. It is either 'Full' or 'Simple'.");
    }

}
