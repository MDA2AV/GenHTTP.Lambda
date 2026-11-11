using System.Diagnostics;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// Builds a project with the .NET SDK, the way whoever downloads an export or
/// clones a lambda builds it - two at a time for the whole run, and without
/// build servers.
/// </summary>
/// <remarks>
/// A build is a process of its own with MSBuild and the compiler behind it, a
/// few hundred megabytes, and its output lands below the fixture's directory
/// in the temporary folder, which is memory where that is a tmpfs. The tests
/// that build every demo used to start their builds all at once, beside the
/// other tests running in parallel: well over a dozen together ran a machine
/// with 8 GB and no swap out of memory, twice. So they wait for their turn
/// here.
///
/// The build servers - MSBuild's nodes, the shared compiler - outlive the build
/// that started them by minutes, holding their memory after the test is done.
/// A build here starts none, and gives back what it took when it ends.
/// </remarks>
internal static class DotnetBuild
{
    private const int AtOnce = 2;

    private static readonly SemaphoreSlim Turns = new(AtOnce);

    #region Functionality

    /// <summary>
    /// Builds the project in a directory, once a turn is free.
    /// </summary>
    /// <returns>The exit code of the build, and what it printed on both streams</returns>
    public static async Task<(int ExitCode, string Output)> RunAsync(string directory)
    {
        await Turns.WaitAsync();

        try
        {
            var start = new ProcessStartInfo("dotnet", "build -c Release -nologo -clp:ErrorsOnly --disable-build-servers -maxcpucount:1")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(start)!;

            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return (process.ExitCode, await output + await error);
        }
        finally
        {
            Turns.Release();
        }
    }

    #endregion

}
