namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a view of the editor is called wherever it leaves the platform.
/// </summary>
public static class EditorViewNames
{

    public static string Name(EditorView view) => view switch
    {
        EditorView.Simple => "Simple",
        _ => "Full"
    };

    /// <summary>
    /// The view of a name, in any case.
    /// </summary>
    public static bool TryRead(string? name, out EditorView view)
    {
        foreach (var known in Enum.GetValues<EditorView>())
        {
            if (string.Equals(name?.Trim(), Name(known), StringComparison.OrdinalIgnoreCase))
            {
                view = known;
                return true;
            }
        }

        view = default;
        return false;
    }

}
