using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Web;

namespace GenHTTP.Lambda.Tests.Showcases;

/// <summary>
/// The first page of the showcase a rendered page carries, made by the pages
/// themselves rather than taken from the API, is read in the browser the same
/// way as the API's listing - so the two have to be the same JSON.
/// </summary>
[TestClass]
public sealed class ShowcaseListingTests
{

    [TestMethod]
    public void TheRenderedListingIsTheShapeOfTheApis()
    {
        Assert.AreEqual(Shape(typeof(Page<ShowcaseResponse>)).Replace(nameof(ShowcaseResponse), "Entry", StringComparison.Ordinal),
                        Shape(typeof(ShowcaseListing)).Replace(nameof(ShowcaseListingEntry), "Entry", StringComparison.Ordinal));

        Assert.AreEqual(Shape(typeof(ShowcaseResponse)), Shape(typeof(ShowcaseListingEntry)));
    }

    /// <summary>
    /// The properties of a type as they are serialized: their names and their
    /// types, in their order.
    /// </summary>
    private static string Shape(Type type)
        => string.Join(", ", type.GetConstructors()
                                 .OrderByDescending(c => c.GetParameters().Length)
                                 .First()
                                 .GetParameters()
                                 .Select(p => $"{p.Name}: {Name(p.ParameterType)}"));

    private static string Name(Type type)
        => type.IsGenericType ? $"{type.Name}<{string.Join(", ", type.GetGenericArguments().Select(Name))}>" : type.Name;

}
