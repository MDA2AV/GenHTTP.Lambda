namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Reads what the kernel knows about the process, which is more than the
/// runtime is able to say about itself.
/// </summary>
/// <remarks>
/// The managed heap is a small part of what a server of this kind occupies and
/// the figures .NET offers do not say where the rest of it went - a process can
/// sit at several hundred megabytes with twenty of them in managed objects, and
/// nothing in <see cref="GC" /> or <see cref="System.Diagnostics.Process" />
/// distinguishes the compiler's metadata from the engine's buffers. The kernel
/// does, per mapping, in /proc.
///
/// Everything here is Linux only and returns zeroes elsewhere, which is the
/// right shape for a panel: the figures are absent rather than the page.
/// </remarks>
public static class ProcessProbe
{

    private static readonly bool Available = OperatingSystem.IsLinux() && Directory.Exists("/proc/self");

    #region Types

    /// <summary>
    /// Resident memory, split by what is actually holding it.
    /// </summary>
    /// <param name="Resident">Everything the process has in physical memory</param>
    /// <param name="Anonymous">Heap, stacks and buffers - memory with no file behind it</param>
    /// <param name="Jit">Compiled code, which the runtime maps twice so it is never writable and executable at once</param>
    /// <param name="Assemblies">Managed assemblies, read straight from their files</param>
    /// <param name="OtherFiles">Native libraries, globalization tables and the rest</param>
    /// <param name="Swap">Paged out, and therefore no longer counted in the resident figure</param>
    public readonly record struct MemoryBreakdown(long Resident, long Anonymous, long Jit, long Assemblies, long OtherFiles, long Swap);

    /// <param name="Established">Connections open right now, not counting the loopback ones the health check makes</param>
    /// <param name="Accepted">Every inbound connection since the container started, health check included</param>
    public readonly record struct ConnectionCounters(int Established, long Accepted);

    /// <param name="Total">Open file descriptors</param>
    /// <param name="Sockets">How many of them are sockets</param>
    /// <param name="Rings">How many are io_uring rings, which is a fixed handful unless the engine is leaking them</param>
    public readonly record struct DescriptorCounts(int Total, int Sockets, int Rings);

    #endregion

    #region Functionality

    /// <summary>
    /// Splits the resident set by the kind of thing each mapping holds.
    /// </summary>
    public static MemoryBreakdown Memory()
    {
        if (!Available)
        {
            return default;
        }

        long anonymous = 0, jit = 0, assemblies = 0, other = 0, swap = 0;

        try
        {
            // a mapping is a header line followed by its fields, so the header
            // that was last seen is the one the fields belong to
            var kind = Kind.Anonymous;

            foreach (var line in File.ReadLines("/proc/self/smaps"))
            {
                if (Header(line, out var name))
                {
                    kind = Classify(name);
                }
                else if (line.StartsWith("Rss:", StringComparison.Ordinal))
                {
                    var value = Kilobytes(line);

                    switch (kind)
                    {
                        case Kind.Jit: jit += value; break;
                        case Kind.Assembly: assemblies += value; break;
                        case Kind.OtherFile: other += value; break;
                        default: anonymous += value; break;
                    }
                }
                else if (line.StartsWith("Swap:", StringComparison.Ordinal))
                {
                    swap += Kilobytes(line);
                }
            }
        }
        catch (Exception)
        {
            // /proc disappears under some sandboxes; an absent figure is better
            // than a failed sample, which would take the rest of the panel with it
            return default;
        }

        return new MemoryBreakdown(anonymous + jit + assemblies + other, anonymous, jit, assemblies, other, swap);
    }

    /// <summary>
    /// How many connections are open, and how many there have ever been.
    /// </summary>
    public static ConnectionCounters Connections()
    {
        if (!Available)
        {
            return default;
        }

        return new ConnectionCounters(Established(), Accepted());
    }

    /// <summary>
    /// Counts established connections from the socket tables rather than the
    /// summary counters, so the loopback ones can be left out.
    /// </summary>
    /// <remarks>
    /// The health check connects to the server every few seconds from inside
    /// the container. Those are real connections and the kernel counts them,
    /// but reporting them as traffic would put a floor under the graph that
    /// has nothing to do with anybody visiting.
    /// </remarks>
    private static int Established()
    {
        var count = 0;

        foreach (var table in (string[])["/proc/net/tcp", "/proc/net/tcp6"])
        {
            try
            {
                if (!File.Exists(table))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(table).Skip(1))
                {
                    var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    // sl, local_address, rem_address, st
                    if (fields.Length < 4 || fields[3] != "01")
                    {
                        continue;
                    }

                    if (!Loopback(fields[1]) && !Loopback(fields[2]))
                    {
                        count++;
                    }
                }
            }
            catch (Exception)
            {
                // as above: a missing table costs one figure, not the sample
            }
        }

        return count;
    }

    /// <summary>
    /// Whether an address in the socket table is a loopback one.
    /// </summary>
    private static bool Loopback(string address)
    {
        var host = address.AsSpan(0, Math.Max(0, address.IndexOf(':')));

        // little endian, so 127.0.0.1 is written back to front, and ::1 is the
        // v6 form of the same thing
        return host is "0100007F" or "00000000000000000000000001000000";
    }

    /// <summary>
    /// Inbound connections the kernel has accepted since the namespace existed.
    /// </summary>
    private static long Accepted()
    {
        try
        {
            string? names = null;

            foreach (var line in File.ReadLines("/proc/net/snmp"))
            {
                if (!line.StartsWith("Tcp:", StringComparison.Ordinal))
                {
                    continue;
                }

                // the table comes as a line of names followed by a line of
                // values, so the column has to be found before it can be read
                if (names == null)
                {
                    names = line;
                    continue;
                }

                var columns = names.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var at = Array.IndexOf(columns, "PassiveOpens");

                return at > 0 && at < values.Length && long.TryParse(values[at], out var accepted) ? accepted : 0;
            }
        }
        catch (Exception)
        {
            // as above
        }

        return 0;
    }

    /// <summary>
    /// What the process is holding open.
    /// </summary>
    public static DescriptorCounts Descriptors()
    {
        if (!Available)
        {
            return default;
        }

        try
        {
            int total = 0, sockets = 0, rings = 0;

            foreach (var descriptor in Directory.EnumerateFiles("/proc/self/fd"))
            {
                total++;

                var target = File.ResolveLinkTarget(descriptor, false)?.Name;

                if (target == null)
                {
                    continue;
                }

                if (target.StartsWith("socket:", StringComparison.Ordinal))
                {
                    sockets++;
                }
                else if (target.Contains("io_uring", StringComparison.Ordinal))
                {
                    rings++;
                }
            }

            return new DescriptorCounts(total, sockets, rings);
        }
        catch (Exception)
        {
            return default;
        }
    }

    #endregion

    #region Helpers

    private enum Kind { Anonymous, Jit, Assembly, OtherFile }

    /// <summary>
    /// Whether a line opens a mapping, and what that mapping is called.
    /// </summary>
    /// <remarks>
    /// A header is "start-end perms offset dev inode path", and the path is
    /// optional. The fields after it are "Name: value kB", so a line whose
    /// first character is a hex digit and which has no colon before its first
    /// space is the header - cheaper than a regular expression on a file this
    /// long, and the format has not changed in twenty years.
    /// </remarks>
    private static bool Header(string line, out string name)
    {
        name = string.Empty;

        var dash = line.IndexOf('-');

        if (dash <= 0 || dash > 16 || !char.IsAsciiHexDigit(line[0]))
        {
            return false;
        }

        var fields = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);

        name = fields.Length >= 6 ? fields[5].Trim() : string.Empty;

        return true;
    }

    private static Kind Classify(string name)
    {
        if (name.Length == 0 || name[0] == '[')
        {
            return Kind.Anonymous;
        }

        if (name.Contains("doublemapper", StringComparison.Ordinal))
        {
            return Kind.Jit;
        }

        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return Kind.Assembly;
        }

        return Kind.OtherFile;
    }

    /// <summary>
    /// The value of a "Name: 1234 kB" line, in bytes.
    /// </summary>
    private static long Kilobytes(string line)
    {
        var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return fields.Length >= 2 && long.TryParse(fields[1], out var value) ? value * 1024 : 0;
    }

    #endregion

}
