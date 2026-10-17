using GenHTTP.Api.Content.Services;

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
/// file of the lambda again.
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
    public static IReadOnlyList<LambdaFile> Apply(IReadOnlyList<LambdaFile> current,
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

        foreach (var file in files ?? [])
        {
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

            if (file.Encoding == "base64")
            {
                throw LambdaException.Invalid($"'{edit.File}' is binary and cannot be edited as text.");
            }

            if (string.IsNullOrEmpty(edit.Find))
            {
                throw LambdaException.Invalid($"An edit of '{edit.File}' needs the text to find.");
            }

            var first = file.Code.IndexOf(edit.Find, StringComparison.Ordinal);

            if (first < 0)
            {
                throw LambdaException.Invalid($"The text to find does not occur in '{edit.File}'.");
            }

            if (file.Code.IndexOf(edit.Find, first + 1, StringComparison.Ordinal) >= 0)
            {
                throw LambdaException.Invalid($"The text to find occurs more than once in '{edit.File}'. Include more of the surrounding text.");
            }

            result[index] = file with { Code = string.Concat(file.Code.AsSpan(0, first), edit.Replace ?? string.Empty, file.Code.AsSpan(first + edit.Find.Length)) };
        }

        return result;
    }

}
