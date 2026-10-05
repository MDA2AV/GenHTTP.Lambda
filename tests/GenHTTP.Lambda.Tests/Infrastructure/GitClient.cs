using System.Diagnostics;
using System.Text;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// What running git came to.
/// </summary>
internal sealed record GitResult(int ExitCode, string Output, string Error)
{

    public bool Success => ExitCode == 0;

    /// <summary>
    /// What git printed, both streams - a push says what the server said on the second.
    /// </summary>
    public string Said => Output + Error;

    public override string ToString() => $"exit code {ExitCode}\n--- stdout ---\n{Output}\n--- stderr ---\n{Error}";

}

/// <summary>
/// The git command line client, in a home of its own, so a test neither
/// depends on nor changes how the machine it runs on has git configured.
/// </summary>
/// <remarks>
/// It checks every object it receives (<c>transfer.fsckObjects</c>), so a
/// commit or a tree the platform serves wrongly fails the test that fetched it.
/// </remarks>
internal sealed class GitClient : IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    #region Get-/Setters

    /// <summary>
    /// Where the clones of this client are made.
    /// </summary>
    public string Root { get; }

    private string Home { get; }

    #endregion

    #region Initialization

    public GitClient()
    {
        Root = Path.Combine(Path.GetTempPath(), "genhttp-lambda-git", Guid.NewGuid().ToString("n"));
        Home = Path.Combine(Root, ".home");

        Directory.CreateDirectory(Home);

        File.WriteAllText(Path.Combine(Home, ".gitconfig"), """
            [user]
                name = Somebody
                email = somebody@example.com
            [init]
                defaultBranch = main
            [transfer]
                fsckObjects = true
            [core]
                autocrlf = false
            [advice]
                detachedHead = false
            [commit]
                gpgsign = false
            [gc]
                auto = 0
            [pull]
                rebase = true
            """);
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Where a clone of this client is.
    /// </summary>
    public string PathOf(string clone) => Path.Combine(Root, clone);

    /// <summary>
    /// Runs git, failing the test where it fails.
    /// </summary>
    public async Task<GitResult> RunAsync(string? clone, params string[] arguments)
    {
        var result = await TryAsync(clone, arguments);

        if (!result.Success)
        {
            Assert.Fail($"git {string.Join(' ', arguments)} failed with {result}");
        }

        return result;
    }

    /// <summary>
    /// Runs git, whatever it comes to.
    /// </summary>
    public async Task<GitResult> TryAsync(string? clone, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = clone != null ? PathOf(clone) : Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["HOME"] = Home;
        start.Environment["XDG_CONFIG_HOME"] = Home;
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        start.Environment["GIT_TERMINAL_PROMPT"] = "0";
        start.Environment["LANG"] = "C";
        start.Environment["LC_ALL"] = "C";

        using var process = Process.Start(start) ?? throw new InvalidOperationException("git did not start");

        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        using var cancellation = new CancellationTokenSource(Timeout);

        try
        {
            await process.WaitForExitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            Assert.Fail($"git {string.Join(' ', arguments)} did not finish within {Timeout}");
        }

        return new GitResult(process.ExitCode, await output, await error);
    }

    /// <summary>
    /// Clones a repository and checks everything it received.
    /// </summary>
    public async Task<string> CloneAsync(string url, string clone)
    {
        await RunAsync(null, "clone", url, clone);

        await RunAsync(clone, "fsck", "--strict");

        return PathOf(clone);
    }

    /// <summary>
    /// What a file of a clone holds.
    /// </summary>
    public string Read(string clone, string path) => File.ReadAllText(Path.Combine(PathOf(clone), path));

    /// <summary>
    /// Writes a file of a clone, its folders included.
    /// </summary>
    public void Write(string clone, string path, string content)
    {
        var file = Path.Combine(PathOf(clone), path);

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        File.WriteAllText(file, content);
    }

    /// <summary>
    /// Changes a file of a clone: the text found, replaced.
    /// </summary>
    public void Change(string clone, string path, string find, string replace)
    {
        var content = Read(clone, path);

        Assert.Contains(find, content, $"{path} holds what is to be changed");

        Write(clone, path, content.Replace(find, replace));
    }

    /// <summary>
    /// Commits everything that changed in a clone, new files and removed ones included.
    /// </summary>
    public async Task<GitResult> CommitAsync(string clone, string message)
    {
        await RunAsync(clone, "add", "-A");

        return await RunAsync(clone, "commit", "-q", "-m", message);
    }

    /// <summary>
    /// What a command printed, trimmed - a commit id, a list of names.
    /// </summary>
    public async Task<string> ReadAsync(string clone, params string[] arguments) => (await RunAsync(clone, arguments)).Output.Trim();

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, true);
        }
        catch (Exception)
        {
            // the temporary folder of the machine is cleaned up by the system
        }
    }

    #endregion

}
