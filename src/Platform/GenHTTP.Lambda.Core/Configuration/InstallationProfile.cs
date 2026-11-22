namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// Who an installation is for: the public, somebody and the people around
/// them, or an organization and its people.
/// </summary>
/// <remarks>
/// It changes how the installation speaks, not what it does: the examples its
/// pages offer, the words they use where an organization would say them
/// otherwise, whether they link to GenHTTP Lambda's project, and what agents
/// are told about the apps built here (<c>platform_guide</c>). Nothing the
/// platform allows, refuses, keeps or limits depends on it - those are the
/// tiers', the limits' and the operator's switches.
///
/// The host decides which there may be (<see cref="LambdaOptions.Profile"/>,
/// <see cref="LambdaOptions.Profiles"/>): genhttp.dev is the cloud, and the
/// community edition is a home installation until its operator says it is an
/// organization's.
/// </remarks>
public enum InstallationProfile
{

    /// <summary>
    /// A public platform anybody may build on, such as genhttp.dev: strangers,
    /// and apps for the public - a club, an event, a game, a shop.
    /// </summary>
    Cloud,

    /// <summary>
    /// Run by somebody for themselves: apps for them, their household and their
    /// friends. What the pages say on genhttp.dev reads as it should here too.
    /// </summary>
    Home,

    /// <summary>
    /// Run by an organization for its people: tools for work - forms, lists,
    /// bookings, dashboards - used by colleagues rather than the public.
    /// </summary>
    Enterprise

}
