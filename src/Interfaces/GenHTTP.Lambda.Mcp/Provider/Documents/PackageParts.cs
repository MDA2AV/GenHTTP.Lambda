using System.IO.Compression;
using System.Xml;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// The parts of an office document - a zip of XML files, as Word, Excel,
/// PowerPoint and the OpenDocument formats all keep theirs.
/// </summary>
/// <remarks>
/// What is read is a stranger's file, so it is read as one: no DTD, nothing
/// resolved, and all the parts read of one document unpack to a bounded
/// amount together - a few megabytes of zip can hold gigabytes of XML. Parts
/// are read as they unpack, never unpacked first.
/// </remarks>
internal sealed class PackageParts : IDisposable
{

    /// <summary>The most all the parts read of one document may unpack to.</summary>
    public const long MostUnpacked = 64L * 1024 * 1024;

    private static readonly XmlReaderSettings Settings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        CloseInput = true
    };

    private readonly ZipArchive zip;

    private long left = MostUnpacked;

    #region Initialization

    private PackageParts(ZipArchive zip)
    {
        this.zip = zip;
    }

    /// <exception cref="InvalidDataException">It is no zip</exception>
    public static PackageParts Open(byte[] content) => new(new ZipArchive(new MemoryStream(content, false), ZipArchiveMode.Read));

    #endregion

    #region Reading

    /// <summary>
    /// A part, to be read as it unpacks - or nothing, where the document has
    /// none of that name.
    /// </summary>
    public XmlReader? Read(string path)
    {
        var entry = zip.GetEntry(path);

        return entry == null ? null : XmlReader.Create(new Budgeted(entry.Open(), this), Settings);
    }

    /// <summary>
    /// What a part refers to (<c>_rels/</c> beside it), in the order it says:
    /// the id, the kind as its last word, and the part referred to.
    /// </summary>
    /// <param name="part">The part, or the empty string for the package itself</param>
    public IReadOnlyList<(string Id, string Kind, string Target)> Relationships(string part)
    {
        var folder = Folder(part);

        using var reader = Read($"{(folder.Length > 0 ? folder + "/" : "")}_rels/{part[(part.LastIndexOf('/') + 1)..]}.rels");

        if (reader == null)
        {
            return [];
        }

        var found = new List<(string, string, string)>();

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Relationship" && reader.GetAttribute("TargetMode") != "External"
                && reader.GetAttribute("Id") is { } id && reader.GetAttribute("Target") is { Length: > 0 } target)
            {
                var kind = reader.GetAttribute("Type") ?? "";

                found.Add((id, kind[(kind.LastIndexOf('/') + 1)..], Resolve(folder, target)));
            }
        }

        return found;
    }

    /// <summary>
    /// The part a reference names, from the folder of the part that names it:
    /// <c>worksheets/sheet1.xml</c> from <c>xl</c> is <c>xl/worksheets/sheet1.xml</c>.
    /// </summary>
    public static string Resolve(string folder, string target)
    {
        var path = new List<string>();

        if (!target.StartsWith('/') && folder.Length > 0)
        {
            path.AddRange(folder.Split('/'));
        }

        foreach (var segment in target.TrimStart('/').Split('/'))
        {
            if (segment == "..")
            {
                if (path.Count > 0)
                {
                    path.RemoveAt(path.Count - 1);
                }
            }
            else if (segment is not ("." or ""))
            {
                path.Add(segment);
            }
        }

        return string.Join('/', path);
    }

    private static string Folder(string part) => part.Contains('/') ? part[..part.LastIndexOf('/')] : "";

    #endregion

    #region Lifecycle

    public void Dispose() => zip.Dispose();

    #endregion

    #region Budget

    /// <summary>
    /// A part as it unpacks, counted against what the whole document may
    /// unpack to.
    /// </summary>
    private sealed class Budgeted(Stream inner, PackageParts owner) : Stream
    {

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => Count(inner.Read(buffer, offset, count));

        public override int Read(Span<byte> buffer) => Count(inner.Read(buffer));

        private int Count(int read)
        {
            owner.left -= read;

            if (owner.left < 0)
            {
                throw new InvalidDataException("The document unpacks to more than is read of one.");
            }

            return read;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

    }

    #endregion

}
