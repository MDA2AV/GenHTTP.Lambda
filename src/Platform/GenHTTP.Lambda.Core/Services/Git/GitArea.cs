using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Git;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Redirects;

using Microsoft.Extensions.Logging;

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
