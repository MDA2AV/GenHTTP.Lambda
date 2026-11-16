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
    (Brand Brand, string? Previous) Rename(string? name);

    /// <summary>
    /// Gives the installation a logo, or another one.
    /// </summary>
    Brand ChangeLogo(byte[] content);

    /// <summary>
    /// Takes the logo away. Nothing happens where there is none.
    /// </summary>
    Brand RemoveLogo();

    /// <summary>
    /// Gives the installation back the platform's name and mark.
    /// </summary>
    Brand Reset();

}
