using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.Deployment.Model;

[Summary("A replacement of text within one file.")]
public sealed record FileEdit(
    [Summary("The name of the file to change, which has to be text")]
    string File,
    [Summary("The text to replace, exactly as it is in the file. It has to occur exactly once - include more of what surrounds it where it occurs more often.")]
    string Find,
    [Summary("What to put in its place; empty removes it")]
    string Replace);

/// <summary>
/// Applies a partial change to the files of a version.
/// </summary>
/// <remarks>
/// Lets a caller change one file, or a line in one, without sending every
/// file of the lambda again - and without reading any of them but the ones
/// it edits: what it leaves alone stays where it is kept.
/// </remarks>
public static class LambdaChanges
{

    /// <summary>
    /// Returns the files of <paramref name="current" /> with the changes applied.
    /// </summary>
    /// <param name="current">The files to start from</param>
    /// <param name="files">Files to add, or to replace where one of that name exists</param>
    /// <param name="remove">Names of files to remove</param>
    /// <param name="edits">Replacements within files, applied last</param>
    public static IReadOnlyList<VersionFile> Apply(IReadOnlyList<VersionFile> current,
                                                   IReadOnlyList<LambdaFile>? files,
                                                   IReadOnlyList<string>? remove,
                                                   IReadOnlyList<FileEdit>? edits)
    {
        if (files is not { Count: > 0 } && remove is not { Count: > 0 } && edits is not { Count: > 0 })
        {
            throw LambdaException.Invalid("Nothing to change: pass files, remove or edits.");
        }

        var result = current.ToList();

        foreach (var name in remove ?? [])
        {
            if (result.RemoveAll(f => f.Name == name) == 0)
            {
                throw LambdaException.Invalid($"There is no file called '{name}' to remove.");
            }
        }

        foreach (var sent in files ?? [])
        {
            var file = VersionFile.FromSent(sent);

            var index = result.FindIndex(f => f.Name == file.Name);

            if (index >= 0)
            {
                result[index] = file;
            }
            else
            {
                result.Add(file);
            }
        }

        foreach (var edit in edits ?? [])
        {
            var index = result.FindIndex(f => f.Name == edit.File);

            if (index < 0)
            {
                throw LambdaException.Invalid($"There is no file called '{edit.File}' to edit.");
            }

            var file = result[index];

            if (file.Binary)
            {
                throw LambdaException.Invalid($"'{edit.File}' is binary and cannot be edited as text.");
            }

            if (string.IsNullOrEmpty(edit.Find))
            {
                throw LambdaException.Invalid($"An edit of '{edit.File}' needs the text to find.");
            }

            var text = file.ReadText();

            var first = text.IndexOf(edit.Find, StringComparison.Ordinal);

            if (first < 0)
            {
                throw LambdaException.Invalid($"The text to find does not occur in '{edit.File}'.");
            }

            if (text.IndexOf(edit.Find, first + 1, StringComparison.Ordinal) >= 0)
            {
                throw LambdaException.Invalid($"The text to find occurs more than once in '{edit.File}'. Include more of the surrounding text.");
            }

            result[index] = VersionFile.FromText(file.Name, string.Concat(text.AsSpan(0, first), edit.Replace ?? string.Empty, text.AsSpan(first + edit.Find.Length)));
        }

        return result;
    }

}
