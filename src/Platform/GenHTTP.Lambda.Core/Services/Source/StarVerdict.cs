using System.Buffers.Text;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// What becomes of a star.
/// </summary>
public enum StarVerdict
{

    /// <summary>
    /// It is counted.
    /// </summary>
    Counted,

    /// <summary>
    /// The address already did exactly this; the count stays as it is.
    /// </summary>
    Unchanged,

    /// <summary>
    /// The address changed too many stars lately.
    /// </summary>
    TooMany

}
