using System.Reflection;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The files of one starting point, read from the assembly the first time they are asked for.
/// </summary>
public sealed class LambdaTemplate(string id, string resource, params (string Name, string Resource)[] parts)
{
    private readonly Lazy<string> _source = new(() => Read(resource), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Lazy<IReadOnlyList<TemplatePart>> _parts = new(
        () => [.. parts.Select(p => new TemplatePart(p.Name, p.Resource))],
        LazyThreadSafetyMode.ExecutionAndPublication);

    public string Id => id;

    /// <summary>
    /// The files beside the snippet, in the order they are offered.
    /// </summary>
    public IReadOnlyList<TemplatePart> Parts => _parts.Value;

    /// <summary>
    /// The snippet, still carrying its key placeholder and demo markers.
    /// </summary>
    public string Source => _source.Value;

    /// <summary>
    /// Reads one embedded template file.
    /// </summary>
    /// <param name="resource">Its name in the assembly, without extensions</param>
    /// <param name="kind">
    /// The extension it is stored under, which is the extension of the file it
    /// becomes: a page is kept as a page, so what is read in the editor is what
    /// is served.
    /// </param>
    internal static string Read(string resource, string kind = "cs")
    {
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();

        var name = Array.Find(names, n => n.EndsWith($".{resource}.{kind}.txt", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"The template '{resource}.{kind}' is missing from the assembly ({string.Join(", ", names)}).");

        using var stream = assembly.GetManifestResourceStream(name)!;

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

}
