using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

public enum ProjectPathKind
{

    /// <summary>A file of the lambda: its code, or one of its resources.</summary>
    Lambda,

    /// <summary>What the platform puts around a lambda to make it a project.</summary>
    Generated

}
