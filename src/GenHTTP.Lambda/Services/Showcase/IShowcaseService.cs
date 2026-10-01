namespace GenHTTP.Lambda.Services.Showcase;

/// <summary>
/// The lambdas their owners chose to show to everybody else.
/// </summary>
/// <remarks>
/// Kept apart from the meta service on purpose: presenting a lambda is not
/// part of building one, and nothing about writing, saving or deploying code
/// reads or changes an entry here.
/// </remarks>
public interface IShowcaseService
{

    /// <summary>
    /// The entry of the lambda behind an editor key, if it has one.
    /// </summary>
    ShowcaseInfo? Get(string privateKey);

    /// <summary>
    /// Creates or replaces the entry of a lambda.
    /// </summary>
    ShowcaseInfo Save(string privateKey, ShowcaseDraft draft);

    /// <summary>
    /// Takes the lambda out of the showcase. Nothing happens when it was not in it.
    /// </summary>
    void Remove(string privateKey);

    /// <summary>
    /// One page of the entries of lambdas that are online, the most active first.
    /// </summary>
    ShowcasePage List(int skip, int take);

    /// <summary>
    /// The picture of the entry of the lambda at a public key.
    /// </summary>
    ShowcaseImage? GetImage(string publicKey);

}
