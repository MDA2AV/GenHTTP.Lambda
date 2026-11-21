using System.Buffers;
using System.Text;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The index page of a bundle as the installation presents itself right now.
/// </summary>
/// <remarks>
/// The single page application holds one handler for its index page for as
/// long as it runs, so the page is made again as it is asked for: the bundle
/// reads its index page again once a build replaced it, and the brand changes
/// whenever the operator changes it. What was made is kept until either
/// does. Its length is not said ahead of the content - a brand changed
/// between the two would make the one disagree with the other - so it is
/// sent as it is written.
/// </remarks>
internal sealed class BrandedIndex(Bundle bundle, IBrandingService branding) : IResource
{
    private static readonly ContentType Html = new("text/html; charset=utf-8");

    private volatile Snapshot? _made;

    public string Name => "index.html";

    public DateTime? Modified => null;

    public ContentType? ContentType => Html;

    public ulong? Length => null;

    public ValueTask<ulong> CalculateChecksumAsync() => new(Current().Checksum);

    public ValueTask<Stream> GetContentAsync() => new(new MemoryStream(Current().Content, false));

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(Current().Content);
        return ValueTask.CompletedTask;
    }

    private Snapshot Current()
    {
        var index = bundle.Index();
        var brand = branding.Current;

        if (_made is { } made && ReferenceEquals(made.Index, index) && ReferenceEquals(made.Brand, brand))
        {
            return made;
        }

        var markup = PageBranding.Apply(index, brand);

        return _made = new Snapshot(index, brand, Encoding.UTF8.GetBytes(markup), (ulong)markup.GetHashCode());
    }

    private sealed record Snapshot(string Index, Brand Brand, byte[] Content, ulong Checksum);

}
