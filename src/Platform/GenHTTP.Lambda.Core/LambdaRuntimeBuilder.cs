using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenHTTP.Lambda;

/// <summary>
/// Configures and builds a <see cref="LambdaRuntime"/>.
/// </summary>
public sealed class LambdaRuntimeBuilder
{
    private RuntimeOptions _options = new();

    private ILoggerFactory? _loggers;

    #region Functionality

    /// <summary>
    /// How the runtime is configured.
    /// </summary>
    public LambdaRuntimeBuilder Options(RuntimeOptions options)
    {
        _options = options;
        return this;
    }

    /// <summary>
    /// Where the runtime logs to - which is the host's, not a platform's.
    /// Without, it says nothing.
    /// </summary>
    public LambdaRuntimeBuilder Logging(ILoggerFactory loggers)
    {
        _loggers = loggers;
        return this;
    }

    /// <summary>
    /// Builds the runtime, for the platforms to be built on.
    /// </summary>
    public LambdaRuntime Build()
    {
        Directory.CreateDirectory(_options.DataDirectory);

        return new LambdaRuntime(_options, _loggers ?? NullLoggerFactory.Instance);
    }

    #endregion

}
