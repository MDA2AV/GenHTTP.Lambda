using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// Reads a file sent as the body of a request, as it is.
/// </summary>
internal static class Bodies
{

    /// <summary>
    /// The body, read no further than it may go - so a larger one is refused
    /// rather than held.
    /// </summary>
    /// <param name="limit">How large it may be, in bytes</param>
    /// <param name="refusal">What a larger one is refused with</param>
    public static async ValueTask<byte[]> ReadAsync(Stream body, int limit, string refusal)
    {
        using var buffer = new MemoryStream();

        var chunk = new byte[16 * 1024];

        int read;

        while ((read = await body.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > limit)
            {
                throw LambdaException.Invalid(refusal);
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

}
