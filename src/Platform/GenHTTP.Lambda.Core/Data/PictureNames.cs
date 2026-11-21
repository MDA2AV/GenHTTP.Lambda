using System.Globalization;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// What the pictures the platform keeps are called in the stores of the files
/// integration (<see cref="GenHTTP.Lambda.Integrations.Files.FileStore"/>).
/// </summary>
/// <remarks>
/// Beside the rows that describe them, since both the services that keep them
/// and the move out of the rows (<see cref="PictureMove"/>) name them.
/// </remarks>
internal static class PictureNames
{

    /// <summary>
    /// The picture of a lambda's showcase entry: its id.
    /// </summary>
    public static string Showcase(long lambdaId) => lambdaId.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// A picture of the branding: its purpose (<c>logo</c>).
    /// </summary>
    public static string Branding(string purpose) => purpose;

}
