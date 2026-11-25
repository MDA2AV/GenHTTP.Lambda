namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages, and what its operator
/// changes about it: a name and a logo of its own, colours and fonts.
/// </summary>
/// <remarks>
/// Where the host does not let the operator brand the installation
/// (<c>LambdaOptions.Branding</c>), it is the platform's own, and every change
/// is refused.
/// </remarks>
public interface IBrandingService
{

    /// <summary>
    /// Whether the operator may brand the installation here.
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
    /// Draws the pages in the operator's colours, or in the platform's where
    /// one is nothing.
    /// </summary>
    /// <param name="accent">What links, buttons and marks are drawn in</param>
    /// <param name="second">What is drawn in the platform's purple otherwise, and the second light of the moving background</param>
    /// <returns>The brand as it is now, and the look it had before</returns>
    ValueTask<(Brand Brand, BrandLook Previous)> ColorAsync(BrandColor? accent, BrandColor? second, CancellationToken cancellation = default);

    /// <summary>
    /// Sets the pages in the operator's fonts, or in the system's where one is
    /// made of no files.
    /// </summary>
    /// <param name="text">The names of the shared files the font of the pages is made of</param>
    /// <param name="headings">Those of the font of their headings; none for the same font</param>
    /// <returns>The brand as it is now, and the look it had before</returns>
    ValueTask<(Brand Brand, BrandLook Previous)> FontAsync(IReadOnlyList<string> text, IReadOnlyList<string> headings, CancellationToken cancellation = default);

    /// <summary>
    /// The look the given colours and fonts would make, or why they would not
    /// - for whoever looks before anything is changed.
    /// </summary>
    ValueTask<BrandLook> ComposeAsync(BrandColor? accent, BrandColor? second, IReadOnlyList<string> text, IReadOnlyList<string> headings, CancellationToken cancellation = default);

    /// <summary>
    /// Reads the fonts again, once one of their files was replaced.
    /// </summary>
    ValueTask RefreshAsync(CancellationToken cancellation = default);

    /// <summary>
    /// Gives the installation back the platform's name, mark, colours and
    /// fonts. The shared files stay.
    /// </summary>
    ValueTask<Brand> ResetAsync(CancellationToken cancellation = default);

}
