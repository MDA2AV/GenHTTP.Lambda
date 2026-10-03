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
    /// Holds the code and the assets to what the tier of the lambda allows.
    /// </summary>
    /// <remarks>
    /// Only ever when a version is written. A lambda that leaves the premium
    /// tier keeps the versions it has, and can put any of them online again;
    /// what it cannot do is save a new one until it fits. A refusal names
    /// what the premium tier allows, so whoever reads it knows there is more.
    /// A feature is held to the same, since what it holds becomes a version.
    /// </remarks>
    public static void ValidateAllowance(IReadOnlyList<LambdaFile> files, LambdaTier tier, LimitsService limits)
    {
        // the limit counts what was written rather than what it is stored as,
        // so splitting a lambda into files does not spend any of it on the
        // envelope those files are kept in
        var code = limits.MaxCodeLengthOf(tier);

        if (LambdaSource.Length(files) > code)
        {
            throw LambdaException.Invalid($"The code must not exceed {code:N0} characters.{Beyond(tier, code, limits.MaxCodeLengthOf(LambdaTier.Premium), $"{limits.MaxCodeLengthOf(LambdaTier.Premium):N0} characters")}");
        }

        // the documentation and the tests are carried the same way as the
        // assets - a copy in every version - so they share the allowance
        var assets = limits.MaxAssetBytesOf(tier);

        var context = LambdaSource.ContextBytes(files);

        if (LambdaSource.AssetBytes(files) + context > assets)
        {
            var what = context > 0 ? "The assets, the documentation and the tests" : "The assets";

            throw LambdaException.Invalid($"{what} must not exceed {Readable(assets)} in total.{Beyond(tier, assets, limits.MaxAssetBytesOf(LambdaTier.Premium), Readable(limits.MaxAssetBytesOf(LambdaTier.Premium)))} A large file that is not code - a model, a dataset, media - belongs in the workspace, which is kept apart from the versions.");
        }
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
        => bytes % (1024 * 1024) == 0 ? $"{bytes / 1024 / 1024} MB" : $"{bytes / 1024} KB";

}
