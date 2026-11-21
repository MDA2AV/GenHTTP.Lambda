using System.Reflection;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// One file of a template beside its snippet, read when it is first wanted.
/// </summary>
public sealed class TemplatePart(string name, string resource)
{
    // the extension of the file this becomes is also the extension it is
    // stored under, so a part says what it is by what it is called
    private readonly Lazy<string> _source = new(
        () => LambdaTemplate.Read(resource, Path.GetExtension(name).TrimStart('.')),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public string Name => name;

    public string Source => _source.Value;
}
