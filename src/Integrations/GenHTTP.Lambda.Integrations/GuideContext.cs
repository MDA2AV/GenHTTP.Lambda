namespace GenHTTP.Lambda.Integrations;

/// <summary>
/// What a part of the guide an integration writes may refer to.
/// </summary>
/// <param name="Entry">The file a lambda's top-level code is in, lambda.cs</param>
public sealed record GuideContext(string Entry);
