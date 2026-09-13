using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Compilation;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// The suggestions the code editor offers: every type a lambda can reach
/// without writing a using, plus a few snippets for the usual starting points.
/// </summary>
/// <remarks>
/// Read from the modules that are actually loaded, so the list cannot drift
/// away from what the compiler accepts.
/// </remarks>
public static class CompletionCatalog
{

    private static readonly CompletionItem[] Snippets =
    [
        new("lambda-inline", "snippet", "A functional service with a few routes", """
            var service = Inline.Create()
                                .Get(() => new Message("hello"))
                                .Get(":id", (int id) => new Message($"you asked for {id}"))
                                .Post((string body) => body.ToUpperInvariant());

            return Layout.Create().Add("api", service);

            record Message(string Text);
            """),

        new("lambda-webservice", "snippet", "A class based webservice", """
            return Layout.Create().AddService<MyService>("api");

            public class MyService
            {
                [ResourceMethod]
                public string Index() => "hello";

                [ResourceMethod(Method.Post, ":id")]
                public int Echo(int id) => id;
            }
            """),

        new("lambda-openapi", "snippet", "A service with OpenAPI and a browser", """
            var service = Inline.Create().Get(() => 42);

            return Layout.Create()
                         .Add("api", service)
                         .AddOpenApi()
                         .AddScalar();
            """),

        new("lambda-page", "snippet", "A single HTML page", """
            var page = Resource.FromString("<h1>Hello</h1>").Type(ContentType.TextHtml);

            return Content.From(page);
            """),

        new("lambda-workspace", "snippet", "Read and write files in the private workspace", """
            Workspace.WriteText("notes.txt", "written by my lambda");

            return Layout.Create()
                         .Add("files", Workspace.Files())
                         .Index(Inline.Create().Get(() => Workspace.List()));
            """)
    ];

    public static IReadOnlyList<CompletionItem> Items { get; } = Build();

    private static CompletionItem[] Build()
    {
        var namespaces = ModuleCatalog.Imports.Where(i => i.StartsWith("GenHTTP.", StringComparison.Ordinal))
                                              .ToHashSet(StringComparer.Ordinal);

        var items = new Dictionary<string, CompletionItem>(StringComparer.Ordinal);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || assembly.GetName().Name?.StartsWith("GenHTTP.", StringComparison.Ordinal) != true)
            {
                continue;
            }

            IEnumerable<Type> exported;

            try
            {
                exported = assembly.GetExportedTypes();
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var type in exported)
            {
                if (type.IsNested || type.Namespace == null || !namespaces.Contains(type.Namespace))
                {
                    continue;
                }

                var label = Simplify(type.Name);

                // never suggest something the code guard would turn down
                if (CodeGuard.IsBanned(label))
                {
                    continue;
                }

                items.TryAdd(label, new CompletionItem(label, Classify(type), type.Namespace));

                if (label.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    var shortened = label[..^"Attribute".Length];

                    items.TryAdd(shortened, new CompletionItem(shortened, "attribute", type.Namespace));
                }
            }
        }

        return [..items.Values.OrderBy(i => i.Label, StringComparer.Ordinal), ..Snippets];
    }

    private static string Simplify(string name)
    {
        var arity = name.IndexOf('`');

        return arity < 0 ? name : name[..arity];
    }

    private static string Classify(Type type)
    {
        if (type.IsInterface)
        {
            return "interface";
        }

        if (type.IsEnum)
        {
            return "enum";
        }

        if (type.IsValueType)
        {
            return "struct";
        }

        return "class";
    }


}
