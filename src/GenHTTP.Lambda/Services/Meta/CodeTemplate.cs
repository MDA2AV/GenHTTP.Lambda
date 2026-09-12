using System.Reflection;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// The example a new lambda starts with - a small REST service with an
/// OpenAPI specification and a browser to play with it.
/// </summary>
public static class CodeTemplate
{
    private const string Placeholder = "{{KEY}}";

    private static readonly string Source = Read();

    /// <summary>
    /// The template with the given public key filled into its comments.
    /// </summary>
    public static string ForKey(string publicKey) => Source.Replace(Placeholder, publicKey, StringComparison.Ordinal);

    private static string Read()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();

        var name = Array.Find(names, n => n.EndsWith("Template.cs.txt", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"The code template is missing from the assembly ({string.Join(", ", names)}).");

        using var stream = assembly.GetManifestResourceStream(name)!;

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

}
