using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// A table or a view of a lambda's database.
/// </summary>
public sealed record DatabaseTableInfo(string Name, bool View);
