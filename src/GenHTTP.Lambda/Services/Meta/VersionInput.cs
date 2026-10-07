using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What is sent to become a version - code, files and a note - checked
/// and tidied before it is kept.
/// </summary>
/// <remarks>
/// For a lambda and for a feature alike, since what a feature holds becomes
/// a version when it is merged.
/// </remarks>
internal static class VersionInput
{

    /// <summary>
    /// Checks what can be checked without knowing whose code it is.
    /// </summary>
    /// <returns>The files, for the checks that do need to know</returns>
    public static IReadOnlyList<LambdaFile> Validate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw LambdaException.Invalid("The code must not be empty.");
        }

        var files = LambdaSource.Parse(code);

        if (LambdaSource.Validate(files) is { } complaint)
        {
            throw LambdaException.Invalid(complaint);
        }

        return files;
    }

    /// <summary>
    /// Holds a version to what the tier of the lambda allows it to come to:
    /// its code and its resources together.
    /// </summary>
    /// <remarks>
    /// Only ever when a version is written. A lambda that leaves the premium
    /// tier keeps the versions it has, and can put any of them online again;
    /// what it cannot do is save a new one until it fits. A refusal names
    /// what the premium tier allows, so whoever reads it knows there is more.
    /// A feature is held to the same, since what it holds becomes a version.
    ///
    /// Counted in bytes as they are, whatever a file is - every version
    /// carries its own copy of all of it, and is read and written whole.
    /// </remarks>
    public static void ValidateAllowance(IReadOnlyList<LambdaFile> files, LambdaTier tier, ILimitsService limits)
    {
        var allowed = limits.BuildOf(tier);

        var size = LambdaSource.Size(files);

        if (size <= allowed)
        {
            return;
        }

        // a folder of the code that takes most of it is most likely what a
        // build installed or made, which a .gitignore of it keeps out
        var largest = files.Where(f => f.IsCode && f.Name.Contains('/'))
                           .GroupBy(f => f.Name[..(f.Name.IndexOf('/') + 1)])
                           .Select(g => (Folder: g.Key, Bytes: LambdaSource.Size(g)))
                           .OrderByDescending(g => g.Bytes)
                           .FirstOrDefault();

        var where = largest.Folder != null && largest.Bytes > size / 2
            ? $" {largest.Folder} comes to {Readable(largest.Bytes)}: what a build installs, caches or makes is no part of a version - list it in a .gitignore of that folder, which a clone and a zip follow."
            : string.Empty;

        throw LambdaException.Invalid($"The code and the resources of a version must not exceed {Readable(allowed)} together; these come to {Readable(size)}."
                                    + $"{Beyond(tier, allowed, limits.BuildOf(LambdaTier.Premium), Readable(limits.BuildOf(LambdaTier.Premium)))}{where}"
                                    + " A large file that is no part of the program - a model, a dataset, media - belongs in the workspace, which is kept apart from the versions.");
    }

    /// <summary>
    /// A note as it is kept: trimmed, nothing where there was only space, and
    /// cut rather than refused where it runs long.
    /// </summary>
    /// <remarks>
    /// Cut rather than refused because the note is the least important part
    /// of a save. An agent that wrote working code and a paragraph too many
    /// about it should lose the end of the paragraph, not the code.
    /// </remarks>
    /// <param name="most">How long it may be: <see cref="VersionNote.MaxSpecification"/> or <see cref="VersionNote.MaxChange"/></param>
    public static string? Tidy(string? text, int most)
    {
        var trimmed = text?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length <= most ? trimmed : string.Concat(trimmed.AsSpan(0, most - 2), " …");
    }

    private static string Beyond(LambdaTier tier, long allowed, long premium, string readable)
        => tier != LambdaTier.Premium && premium > allowed ? $" A lambda in the premium tier may have {readable}." : string.Empty;

    private static string Readable(long bytes)
        => bytes % (1024 * 1024) == 0 || bytes >= 10 * 1024 * 1024 ? $"{bytes / 1024 / 1024} MB" : $"{bytes / 1024} KB";

}
