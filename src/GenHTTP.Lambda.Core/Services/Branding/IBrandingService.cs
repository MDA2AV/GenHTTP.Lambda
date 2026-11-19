namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages, and what its operator
/// changes about it: a name and a logo of its own.
/// </summary>
/// <remarks>
/// Where the host does not let the operator brand the installation
/// (<c>LambdaOptions.Branding</c>), it is the platform's own, and every change
/// is refused.
/// </remarks>
public interface IBrandingService
{

    /// <summary>
    /// Whether the operator may give the installation a name and a logo here.
    /// </summary>
    bool Changeable { get; }

    /// <summary>
    /// How the installation presents itself right now.
    /// </summary>
    Brand Current { get; }

    /// <summary>
    /// Names the installation, or gives it back the platform's name where the
    /// name is empty.
    /// </summary>
    /// <returns>The brand as it is now, and the name it went by before</returns>
    ValueTask<(Brand Brand, string? Previous)> RenameAsync(string? name, CancellationToken cancellation = default);

    /// <summary>
    /// Gives the installation a logo, or another one.
    /// </summary>
    ValueTask<Brand> ChangeLogoAsync(byte[] content, CancellationToken cancellation = default);

    /// <summary>
    /// Takes the logo away. Nothing happens where there is none.
    /// </summary>
    ValueTask<Brand> RemoveLogoAsync(CancellationToken cancellation = default);

    /// <summary>
    /// Gives the installation back the platform's name and mark.
    /// </summary>
    ValueTask<Brand> ResetAsync(CancellationToken cancellation = default);

}
