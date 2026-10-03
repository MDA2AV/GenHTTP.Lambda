using System.Reflection;

using GenHTTP.Lambda.Tests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Tests.Architecture;

/// <summary>
/// A service is reached through its interfaces: from outside its folder,
/// nothing takes one of its classes from the container.
/// </summary>
/// <remarks>
/// A service orchestrates units of its own - a client, a count, a history - and
/// those hold its rules only while everything goes through the service: a
/// version appended without the lambda's turn, or a build started without the
/// allowance, is a bug nothing else would catch. The compiler cannot keep them
/// apart, since the application is one assembly, so this test does.
///
/// What it reads is what can be handed over: the parameters of constructors,
/// which is how the container hands out a service, and of methods. A class
/// counts as a service when the container answers for it, so records and
/// other data are not caught by it. Building one with <c>new</c> or asking the
/// container for one in a method body is not seen.
/// </remarks>
[TestClass]
public sealed class ServiceBoundaryTests
{

    private const string Services = "GenHTTP.Lambda.Services.";

    /// <summary>
    /// What may take a class of another folder's service, and why. An entry
    /// here is a reason somebody has to agree with, so it stays short.
    /// </summary>
    private static readonly Dictionary<Type, string> Exempt = new()
    {
        [typeof(Application)] = "It builds the container: it registers the classes, and makes the log book and the run log before there is a container to take them from."
    };

    [TestMethod]
    public async Task AServiceIsTakenByItsInterfacesOutsideItsFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var container = fixture.Application.Services.GetRequiredService<IServiceProviderIsService>();

        var crossings = Crossings(container).ToList();

        Assert.IsEmpty(crossings,
            "Taken from outside the folder of their service:\n" + string.Join("\n", crossings) + "\n"
          + "Take an interface of that service instead, or add what is needed to one. A unit of a service is that service's own (see CLAUDE.md, Architecture).");
    }

    [TestMethod]
    public async Task TheRuleSeesAUnitTakenFromAnotherFolder()
    {
        await using var fixture = await LambdaFixture.CreateAsync();

        var container = fixture.Application.Services.GetRequiredService<IServiceProviderIsService>();

        // the units of the meta service are classes the container answers for,
        // so a resource taking one would be caught
        Assert.IsTrue(container.IsService(typeof(GenHTTP.Lambda.Services.Meta.LambdaHistory)));
        Assert.AreEqual("Meta", FolderOf(typeof(GenHTTP.Lambda.Services.Meta.LambdaHistory)));
        Assert.IsNull(FolderOf(typeof(GenHTTP.Lambda.Api.LambdaResource)));
    }

    #region Helpers

    private static IEnumerable<string> Crossings(IServiceProviderIsService container)
    {
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var consumer in typeof(Application).Assembly.GetTypes())
        {
            if (Exempt.ContainsKey(consumer))
            {
                continue;
            }

            var members = consumer.GetConstructors(declared).Cast<MethodBase>()
                                  .Concat(consumer.GetMethods(declared));

            foreach (var member in members)
            {
                foreach (var parameter in member.GetParameters())
                {
                    var taken = parameter.ParameterType;

                    if (taken.IsInterface || !container.IsService(taken))
                    {
                        continue;
                    }

                    var folder = FolderOf(taken);

                    if (folder != null && FolderOf(consumer) != folder)
                    {
                        yield return $"{consumer.FullName}.{member.Name} takes {taken.FullName}";
                    }
                }
            }
        }
    }

    /// <summary>
    /// The folder of the service a type belongs to - the namespace below
    /// <c>Services</c>, with whatever is below that - or nothing outside them.
    /// </summary>
    private static string? FolderOf(Type type)
    {
        var name = (type.DeclaringType ?? type).Namespace ?? "";

        return name.StartsWith(Services, StringComparison.Ordinal) ? name[Services.Length..].Split('.')[0] : null;
    }

    #endregion

}
