using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// Sends JPEG pictures as <c>image/jpeg</c>, which is their type, rather than
/// the <c>image/jpg</c> GenHTTP names them by their extension.
/// </summary>
/// <remarks>
/// A browser shows either. The link previews in every language but English
/// are JPEG, though, and what fetches those for a chat or a feed is stricter
/// than a browser: a picture of a type it does not know is a preview without
/// one.
/// </remarks>
public sealed class JpegTypeConcern : IConcern
{
    private static readonly ContentType Jpeg = new("image/jpeg");

    #region Get-/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public JpegTypeConcern(IHandler content)
    {
        Content = content;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await Content.HandleAsync(request);

        if (response?.Content is { } content && content.Type == ContentType.ImageJpg)
        {
            response.Rebuild()
                    .Content(new Retyped(content, Jpeg));
        }

        return response;
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    /// <summary>
    /// The same content, sent as another type.
    /// </summary>
    private sealed class Retyped(IResponseContent content, ContentType type) : IResponseContent
    {
        public ulong? Length => content.Length;

        public ContentType? Type => type;

        public ReadOnlyMemory<byte>? Encoding => content.Encoding;

        public ValueTask<ulong?> CalculateChecksumAsync() => content.CalculateChecksumAsync();

        public ValueTask WriteAsync(IResponseSink sink) => content.WriteAsync(sink);
    }

    #endregion

}

/// <summary>
/// Builds a <see cref="JpegTypeConcern" /> for a handler.
/// </summary>
public sealed class JpegTypeConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new JpegTypeConcern(content);

}
