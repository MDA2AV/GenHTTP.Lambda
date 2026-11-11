using GenHTTP.Lambda.Services.Deployment.Compilation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Lambda.Tests.Compilation;

/// <summary>
/// Every member a lambda could call with the path of a file is either
/// refused by the code guard or known not to open one.
/// </summary>
/// <remarks>
/// The guard refused names, and a name nobody had listed read the platform's
/// database. This lists every public member of the assemblies a lambda is
/// compiled against that takes a string named like a path, and fails on one
/// that is neither refused nor in <see cref="Harmless" /> - so a library that
/// grows one in an update is looked at before a lambda can call it.
/// </remarks>
[TestClass]
public sealed class PathMemberTests
{

    /// <summary>
    /// Members whose path-like parameter is no path on the host, by type and
    /// member - a route, an address on the network, a name to give a download.
    /// </summary>
    private static readonly HashSet<string> Harmless = new(StringComparer.Ordinal)
    {
        // routes, and addresses a page or a document links to
        "GenHTTP.Api.Content.Documentation.Security.SecurityScheme.OpenIdConnect",
        "GenHTTP.Modules.ApiBrowsing.Common.BrowserHandlerBuilder.Url",
        "GenHTTP.Modules.ApiBrowsing.Common.BrowserMetaData..ctor",
        "GenHTTP.Modules.ApiBrowsing.Common.BrowserMetaData.Deconstruct",
        "GenHTTP.Modules.ApiBrowsing.Extensions.AddRedoc",
        "GenHTTP.Modules.ApiBrowsing.Extensions.AddScalar",
        "GenHTTP.Modules.ApiBrowsing.Extensions.AddSwaggerUi",
        "GenHTTP.Modules.Controllers.Extensions.AddController",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Any",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Delete",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Get",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Head",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.On",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Post",
        "GenHTTP.Modules.Functional.Provider.InlineBuilder.Put",
        "GenHTTP.Modules.Functional.Provider.InlineFunction..ctor",
        "GenHTTP.Modules.Functional.Provider.InlineFunction.Deconstruct",
        "GenHTTP.Modules.Mcp.ResourceLinkContent..ctor",
        "GenHTTP.Modules.Mcp.ResourceLinkContent.Deconstruct",
        "GenHTTP.Modules.Webservices.Extensions.AddService",
        "GenHTTP.Modules.Webservices.ResourceMethodAttribute..ctor",

        // the name a download is given, or a type is guessed from
        "GenHTTP.Modules.Files.Single.ResourceAssetBuilder.AsDownload",
        "GenHTTP.Modules.Files.Single.ResourceAssetHandler..ctor",
        "GenHTTP.Modules.IO.ContentTypeGuessingExtensions.GuessContentType",
        "System.Net.Http.MultipartFormDataContent.Add",
        "System.Net.Mime.MediaTypeMap.GetMediaType",

        // paths into an object or a document, not onto a disk
        "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include",
        "Microsoft.EntityFrameworkCore.RelationalDbFunctionsExtensions.JsonPathExists",
        "System.Xml.XPath.XPathExpression.Compile",
        "System.Xml.XPath.XPathNavigator.Compile",
        "System.Xml.XPath.XPathNavigator.Evaluate",
        "System.Xml.XPath.XPathNavigator.Matches",
        "System.Xml.XPath.XPathNavigator.Select",
        "System.Xml.XPath.XPathNavigator.SelectSingleNode",
        "System.Xml.XmlNode.SelectNodes",
        "System.Xml.XmlNode.SelectSingleNode",

        // where the caller's source was, written into a message
        "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.TagWithCallSite",
        "Microsoft.IdentityModel.Tokens.Experimental.ValidationError.AddCurrentStackFrame",
        "Microsoft.IdentityModel.Tokens.Experimental.ValidationError.GetCurrentStackFrame",

        // exceptions that name what was not found or what failed
        "System.BadImageFormatException..ctor",
        "System.ComponentModel.WarningException..ctor",
        "System.IO.DirectoryNotFoundException..ctor",
        "System.IO.FileNotFoundException..ctor",
        "System.Text.Json.JsonException..ctor",

        // addresses on the network, which a lambda may reach, and their parts
        "System.Net.Cookie..ctor",
        "System.Net.Http.HttpClient.DeleteAsync",
        "System.Net.Http.HttpClient.GetAsync",
        "System.Net.Http.HttpClient.GetByteArrayAsync",
        "System.Net.Http.HttpClient.GetStreamAsync",
        "System.Net.Http.HttpClient.GetStringAsync",
        "System.Net.Http.HttpClient.PatchAsync",
        "System.Net.Http.HttpClient.PostAsync",
        "System.Net.Http.HttpClient.PutAsync",
        "System.Net.Http.HttpRequestMessage..ctor",
        "System.Net.Http.Json.HttpClientJsonExtensions.DeleteFromJsonAsync",
        "System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsAsyncEnumerable",
        "System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync",
        "System.Net.Http.Json.HttpClientJsonExtensions.PatchAsJsonAsync",
        "System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync",
        "System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync",
        "System.Uri..ctor",
        "System.Uri.IsWellFormedUriString",
        "System.Uri.TryCreate",
        "System.UriBuilder..ctor",

        // namespaces and base addresses of XML, which open nothing without
        // a resolver, and the resolvers that would are refused
        "System.Xml.XmlNamespaceManager.AddNamespace",
        "System.Xml.XmlNamespaceManager.LookupPrefix",
        "System.Xml.XmlNamespaceManager.RemoveNamespace",
        "System.Xml.XmlParserContext..ctor",
        "System.Xml.XmlReader.Create",
        "System.Xml.XmlResolver.ResolveUri",

        // a profile of a target framework, a name
        "System.Runtime.Versioning.FrameworkName..ctor",

        // throws on .NET, and needs an assembly name nobody can make
        "System.Text.RegularExpressions.Regex.CompileToAssembly"
    };

    [TestMethod]
    public void EveryMemberThatTakesAPathIsRefusedOrKnownToBeHarmless()
    {
        var compilation = CSharpCompilation.Create("paths", references: ReferenceProvider.Resolve());

        var unknown = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
            {
                continue;
            }

            foreach (var type in PublicTypes(assembly.GlobalNamespace))
            {
                foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                {
                    if (method.DeclaredAccessibility != Accessibility.Public || !method.Parameters.Any(p => IsText(p.Type) && IsPathLike(p.Name)))
                    {
                        continue;
                    }

                    var key = $"{Name(type)}.{method.Name}";

                    if (!Harmless.Contains(key) && SymbolPolicy.Refuse(method, null) == null)
                    {
                        unknown.Add($"{key}({string.Join(", ", method.Parameters.Select(p => p.Name))}) in {assembly.Name}");
                    }
                }
            }
        }

        Assert.IsEmpty(unknown, "Refuse these in SymbolPolicy, or list them as harmless here:\n" + string.Join("\n", unknown));
    }

    private static IEnumerable<INamedTypeSymbol> PublicTypes(INamespaceSymbol space)
    {
        foreach (var member in space.GetMembers())
        {
            if (member is INamespaceSymbol inner)
            {
                foreach (var type in PublicTypes(inner))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol { DeclaredAccessibility: Accessibility.Public } type)
            {
                foreach (var nested in WithNested(type))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;

        foreach (var nested in type.GetTypeMembers().Where(t => t.DeclaredAccessibility == Accessibility.Public))
        {
            foreach (var inner in WithNested(nested))
            {
                yield return inner;
            }
        }
    }

    private static bool IsPathLike(string name)
    {
        var lower = name.ToLowerInvariant();

        if (lower.Contains("namespace", StringComparison.Ordinal))
        {
            return false;
        }

        return lower.Contains("path", StringComparison.Ordinal) || lower.Contains("file", StringComparison.Ordinal)
            || lower.Contains("dir", StringComparison.Ordinal) || lower.Contains("folder", StringComparison.Ordinal)
            || lower.EndsWith("uri", StringComparison.Ordinal) || lower.EndsWith("url", StringComparison.Ordinal)
            || lower.Contains("uristring", StringComparison.Ordinal);
    }

    private static bool IsText(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String
        || type is INamedTypeSymbol { Name: "ReadOnlySpan", TypeArguments: [{ SpecialType: SpecialType.System_Char }] };

    private static string Name(INamedTypeSymbol type)
    {
        var name = type.MetadataName;

        var tick = name.IndexOf('`');

        if (tick >= 0)
        {
            name = name[..tick];
        }

        if (type.ContainingType is { } outer)
        {
            return $"{Name(outer)}.{name}";
        }

        return type.ContainingNamespace.IsGlobalNamespace ? name : $"{type.ContainingNamespace.ToDisplayString()}.{name}";
    }

}
