namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// Whose repositories a component serves.
/// </summary>
public enum GitArea
{

    /// <summary>The owner's, below the editor's key: <c>/editor/{privateKey}/{name}.git</c>.</summary>
    Owned,

    /// <summary>The published sources', for anybody: <c>/source/{publicKey}.git</c>.</summary>
    Published

}
