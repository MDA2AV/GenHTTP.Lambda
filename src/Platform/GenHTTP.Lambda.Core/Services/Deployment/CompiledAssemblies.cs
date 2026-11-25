using System.Runtime.Loader;

using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Where the assemblies of the lambdas are written to, which is the
/// process's: they are loaded into it and stay for as long as it lives.
/// </summary>
/// <remarks>
/// GenHTTP builds its invocation code from the files behind the loaded
/// assemblies, so one of them removed breaks the compilation of every handler
/// that follows - of every platform. So they are kept apart from what belongs
/// to a platform: one that is stopped, started again or removed with its data
/// takes none of them along, nor does a lambda that is deleted.
///
/// What a run before this one left is removed as the process starts, which is
/// the one moment nothing is loaded - except where this process has something
/// loaded from there all the same, a platform of its own having been stopped
/// and started within it.
/// </remarks>
public sealed class CompiledAssemblies
{

    #region Get-/Setters

    private string Root { get; }

    #endregion

    #region Initialization

    public CompiledAssemblies(RuntimeOptions options)
    {
        Root = Path.Combine(options.DataDirectory, "assemblies");

        Clean();

        Directory.CreateDirectory(Root);
    }

    private void Clean()
    {
        if (!Directory.Exists(Root))
        {
            return;
        }

        var loaded = AssemblyLoadContext.Default.Assemblies.Where(a => !a.IsDynamic)
                                        .Select(a => a.Location)
                                        .ToHashSet(StringComparer.Ordinal);

        foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
        {
            if (loaded.Contains(Path.GetFullPath(file)))
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                // in use by somebody else: the next start cleans it up
            }
        }

        foreach (var folder in Directory.EnumerateDirectories(Root))
        {
            try
            {
                Directory.Delete(folder, false);
            }
            catch (IOException)
            {
                // something in it is still loaded
            }
        }
    }

    #endregion

    #region Functionality

    /// <summary>
    /// The directory the assemblies of a lambda are written to.
    /// </summary>
    /// <remarks>
    /// Lambdas of several platforms with the same id share one. An assembly
    /// is named after a random scope, so nothing in there is ever the same file.
    /// </remarks>
    public string DirectoryOf(long lambdaId) => Path.Combine(Root, lambdaId.ToString());

    #endregion

}
