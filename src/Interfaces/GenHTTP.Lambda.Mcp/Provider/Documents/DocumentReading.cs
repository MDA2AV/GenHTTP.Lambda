namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// What an agent is shown of a document: a part of its text, and the
/// pictures of the pages in that part that hold none.
/// </summary>
/// <param name="Text">The part of the text asked for</param>
/// <param name="Next">Where the next part starts, if the text goes on</param>
/// <param name="Pages">How many pages, slides or sheets it has, where it says</param>
/// <param name="Pictures">The scanned pages of this part</param>
/// <param name="Note">Why there is less than the document holds - protected, damaged, nothing but pictures</param>
internal sealed record DocumentReading(string Text, int? Next, int? Pages, IReadOnlyList<DocumentPicture> Pictures, string? Note = null);
