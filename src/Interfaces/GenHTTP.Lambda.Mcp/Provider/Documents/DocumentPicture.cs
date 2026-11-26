namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// A page of a document that holds no text, as the picture it is - a scan.
/// </summary>
/// <param name="Content">The picture, as it is kept in the document</param>
/// <param name="MediaType"><c>image/jpeg</c> or <c>image/png</c></param>
/// <param name="Page">The page it is, counted from one</param>
internal sealed record DocumentPicture(byte[] Content, string MediaType, int Page);
