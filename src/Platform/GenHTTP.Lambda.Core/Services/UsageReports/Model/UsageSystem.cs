using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Os">The operating system: <c>Linux</c>, <c>Windows</c>, <c>macOS</c>, <c>FreeBSD</c> or <c>Other</c></param>
/// <param name="Architecture">The processor architecture, such as <c>x64</c> or <c>arm64</c></param>
/// <param name="Runtime">The version of .NET</param>
/// <param name="Container">Whether it runs in a container (Docker or Podman)</param>
/// <param name="Processors">The processors the process may use</param>
/// <param name="MemoryBytes">The memory of the machine, or the limit of the container or group the process runs in where that is lower</param>
/// <param name="DiskBytes">The size of the volume the data directory is on, where it can be read</param>
/// <param name="DiskFreeBytes">What is free on that volume</param>
public sealed record UsageSystem(string Os, string Architecture, string Runtime, bool Container, int Processors, long MemoryBytes,
                                 long? DiskBytes, long? DiskFreeBytes);
