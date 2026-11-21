using GenHTTP.Api.Content.IO;

using GenHTTP.Lambda.Integrations.Files;

using GenHTTP.Modules.IO;

namespace GenHTTP.Lambda.Integrations.Local;

/// <summary>
/// Configures the local files of an installation.
/// </summary>
/// <remarks>
/// Nothing to configure: where they are follows from the data directory.
/// </remarks>
public sealed class LocalFilesBuilder : IIntegrationBuilder<IFileProvider>
{

    public IFileProvider Build(IntegrationContext context) => new LocalFileProvider(context.DataDirectory);

}
