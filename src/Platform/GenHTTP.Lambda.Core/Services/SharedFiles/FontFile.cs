using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// Reads what a font file is: its format, the weights and the slant it draws,
/// and the letters it has.
/// </summary>
/// <remarks>
/// Every font a browser draws is the same set of tables in one of four
/// wrappers: TrueType and OpenType as they are, WOFF with each table
/// compressed, WOFF2 with all of them compressed as one. Only the tables that
/// say what the font is are read - <c>OS/2</c>, <c>head</c>, <c>name</c>,
/// <c>cmap</c> and, in a variable font, <c>fvar</c> - never the drawings.
///
/// A page has to be told the weights of a face to choose between several, and
/// a browser that is told wrong thickens what is bold already. So they are
/// read here rather than asked of whoever uploads it, as a picture's type is.
/// </remarks>
public static class FontFile
{

    /// <summary>
    /// How much of a compressed font is unpacked at most: a font of two
    /// megabytes holds no more than this, and a file made to hold more is none.
    /// </summary>
    private const int MaxUnpacked = 24 * 1024 * 1024;

    /// <summary>
    /// The tables of WOFF2 by the number it writes in place of their names.
    /// </summary>
    private static readonly string[] Known =
    [
        "cmap", "head", "hhea", "hmtx", "maxp", "name", "OS/2", "post", "cvt ", "fpgm", "glyf", "loca", "prep", "CFF ", "VORG", "EBDT",
        "EBLC", "gasp", "hdmx", "kern", "LTSH", "PCLT", "VDMX", "vhea", "vmtx", "BASE", "GDEF", "GPOS", "GSUB", "EBSC", "JSTF", "MATH",
        "CBDT", "CBLC", "COLR", "CPAL", "SVG ", "sbix", "acnt", "avar", "bdat", "bloc", "bsln", "cvar", "fdsc", "feat", "fmtx", "fvar",
        "gvar", "hsty", "just", "lcar", "mort", "morx", "opbd", "prop", "trak", "Zapf", "Silf", "Glat", "Gloc", "Feat", "Sill"
    ];

    private static readonly HashSet<string> Wanted = ["OS/2", "head", "name", "cmap", "fvar"];

    /// <summary>
    /// The weights as fonts name them, the longer ahead of those they end in.
    /// </summary>
    private static readonly string[] Weighted =
    [
        " Thin", " ExtraLight", " Extra Light", " UltraLight", " Light", " Regular", " Medium", " SemiBold", " Semi Bold", " DemiBold",
        " ExtraBold", " Extra Bold", " Bold", " Black", " Heavy"
    ];

    #region Functionality

    /// <summary>
    /// What a font of the given content would be served as, or nothing for
    /// anything that is no font a browser draws.
    /// </summary>
    public static string? Sniff(ReadOnlySpan<byte> content) => content switch
    {
        _ when content.StartsWith("wOF2"u8) => "font/woff2",
        _ when content.StartsWith("wOFF"u8) => "font/woff",
        _ when content.StartsWith("OTTO"u8) => "font/otf",
        _ when content.StartsWith((ReadOnlySpan<byte>)[0, 1, 0, 0]) || content.StartsWith("true"u8) => "font/ttf",
        _ => null
    };

    /// <summary>
    /// What the font draws, or nothing where it is none, or is damaged.
    /// </summary>
    public static FontFace? Read(byte[] content)
    {
        try
        {
            var tables = Sniff(content) switch
            {
                "font/woff2" => Woff2(content),
                "font/woff" => Woff(content),
                null => null,
                _ => Plain(content)
            };

            return tables == null ? null : Describe(tables);
        }
        catch (Exception e) when (e is ArgumentException or IndexOutOfRangeException or InvalidDataException or InvalidOperationException or IOException or OverflowException)
        {
            // cut off, or made up: a length or an offset leads out of the file
            return null;
        }
    }

    private static FontFace? Describe(Dictionary<string, byte[]> tables)
    {
        // every font has a head, which names itself by this number - whatever
        // merely starts like one has none
        if (!tables.TryGetValue("head", out var head) || Long(head, 12) != 0x5F0F3CF5)
        {
            return null;
        }

        var weight = 400;
        var italic = (Short(head, 44) & 0b10) != 0;

        if (tables.TryGetValue("OS/2", out var traits))
        {
            weight = Math.Clamp((int)Short(traits, 4), 1, 1000);
            italic = (Short(traits, 62) & 0b1) != 0;
        }

        var (from, to) = tables.TryGetValue("fvar", out var variations) && Weights(variations) is { } range ? range : (weight, weight);

        var family = tables.TryGetValue("name", out var names) ? Family(names) : null;

        // a variable font often calls itself after the weight it starts from
        if (family != null && from != to)
        {
            family = WithoutWeight(family);
        }

        var letters = tables.TryGetValue("cmap", out var map) ? Letters(map) : null;

        return new FontFace(family ?? "Unnamed", from, to, italic, letters);
    }

    /// <summary>
    /// The letters the font has a drawing for, of those it is asked for - or
    /// nothing where it says so in none of the two ways every font of the web
    /// does.
    /// </summary>
    private static string? Letters(byte[] map)
    {
        var count = Short(map, 2);

        var tables = new List<(int At, int Format)>();

        for (var i = 0; i < count; i++)
        {
            var record = 4 + i * 8;

            // by Unicode, which is not what a font of symbols is by
            if (Short(map, record) is not (0 or 3) || (Short(map, record) == 3 && Short(map, record + 2) == 0))
            {
                continue;
            }

            var at = checked((int)Long(map, record + 4));

            if (Short(map, at) is 4 or 12)
            {
                tables.Add((at, Short(map, at)));
            }
        }

        if (tables.Count == 0)
        {
            return null;
        }

        return new([.. FontLetters.All.Where(letter => tables.Any(t => t.Format == 4 ? InSegments(map, t.At, letter) : InGroups(map, t.At, letter)))]);
    }

    /// <summary>
    /// Whether a character is in a map of segments (format 4).
    /// </summary>
    private static bool InSegments(byte[] map, int at, char letter)
    {
        var segments = Short(map, at + 6) / 2;

        var ends = at + 14;
        var starts = ends + segments * 2 + 2;
        var deltas = starts + segments * 2;
        var offsets = deltas + segments * 2;

        for (var i = 0; i < segments; i++)
        {
            if (Short(map, ends + i * 2) < letter)
            {
                continue;
            }

            var start = Short(map, starts + i * 2);

            if (start > letter)
            {
                return false;
            }

            var offset = Short(map, offsets + i * 2);

            if (offset == 0)
            {
                return ((letter + Short(map, deltas + i * 2)) & 0xFFFF) != 0;
            }

            // counted from where the offset itself is kept
            return Short(map, offsets + i * 2 + offset + (letter - start) * 2) != 0;
        }

        return false;
    }

    /// <summary>
    /// Whether a character is in a map of groups (format 12).
    /// </summary>
    private static bool InGroups(byte[] map, int at, char letter)
    {
        var groups = Long(map, at + 12);

        for (var i = 0; i < groups; i++)
        {
            var group = at + 16 + i * 12;

            if (Long(map, group) <= letter && letter <= Long(map, group + 4))
            {
                return Long(map, group + 8) + (letter - Long(map, group)) != 0;
            }
        }

        return false;
    }

    /// <summary>
    /// The name of a family without the weight at its end: <c>Nunito
    /// ExtraLight</c> is Nunito in a font that draws all of them.
    /// </summary>
    private static string WithoutWeight(string family)
    {
        foreach (var weight in Weighted)
        {
            if (family.EndsWith(weight, StringComparison.OrdinalIgnoreCase) && family.Length > weight.Length)
            {
                return family[..^weight.Length].TrimEnd();
            }
        }

        return family;
    }

    /// <summary>
    /// The weights a variable font draws, if its weight varies at all.
    /// </summary>
    private static (int From, int To)? Weights(byte[] variations)
    {
        var axes = Short(variations, 4);
        var count = Short(variations, 8);
        var size = Short(variations, 10);

        for (var i = 0; i < count; i++)
        {
            var axis = axes + i * size;

            if (Tag(variations, axis) == "wght")
            {
                var from = Math.Clamp((int)Math.Round(Fixed(variations, axis + 4)), 1, 1000);
                var to = Math.Clamp((int)Math.Round(Fixed(variations, axis + 12)), 1, 1000);

                return from <= to ? (from, to) : null;
            }
        }

        return null;
    }

    /// <summary>
    /// What the font calls its family: the name a designer gave all of it
    /// where it has one, and the name of this style's family otherwise.
    /// </summary>
    private static string? Family(byte[] names)
    {
        var count = Short(names, 2);
        var strings = Short(names, 4);

        string? found = null;

        var best = int.MinValue;

        for (var i = 0; i < count; i++)
        {
            var record = 6 + i * 12;

            var platform = Short(names, record);
            var language = Short(names, record + 4);
            var id = Short(names, record + 6);

            if (id is not (1 or 16) || platform is not (0 or 1 or 3))
            {
                continue;
            }

            // the family before the style's, and then English as Windows has it
            var rank = (id == 16 ? 10 : 0) + (platform == 3 && language == 0x409 ? 4 : platform == 3 ? 2 : 0);

            if (rank <= best)
            {
                continue;
            }

            var text = Slice(names, strings + Short(names, record + 10), Short(names, record + 8));

            var name = (platform == 1 ? Encoding.Latin1 : Encoding.BigEndianUnicode).GetString(text);

            // one line of it: it is shown beside the file's name
            name = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();

            if (name.Length > 0)
            {
                (found, best) = (name.Length > 80 ? name[..80] : name, rank);
            }
        }

        return found;
    }

    #endregion

    #region The wrappers

    /// <summary>
    /// TrueType and OpenType: a directory of tables, and the tables.
    /// </summary>
    private static Dictionary<string, byte[]> Plain(byte[] content)
    {
        var tables = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        var count = Short(content, 4);

        for (var i = 0; i < count; i++)
        {
            var entry = 12 + i * 16;
            var tag = Tag(content, entry);

            if (Wanted.Contains(tag))
            {
                tables[tag] = Slice(content, Long(content, entry + 8), Long(content, entry + 12)).ToArray();
            }
        }

        return tables;
    }

    /// <summary>
    /// WOFF: the same, each table compressed where that made it smaller.
    /// </summary>
    private static Dictionary<string, byte[]> Woff(byte[] content)
    {
        var tables = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        var count = Short(content, 12);

        for (var i = 0; i < count; i++)
        {
            var entry = 44 + i * 20;
            var tag = Tag(content, entry);

            if (!Wanted.Contains(tag))
            {
                continue;
            }

            var kept = Slice(content, Long(content, entry + 4), Long(content, entry + 8));
            var length = Long(content, entry + 12);

            if (kept.Length == length)
            {
                tables[tag] = kept.ToArray();
                continue;
            }

            using var unpacking = new ZLibStream(new MemoryStream(kept.ToArray()), CompressionMode.Decompress);

            tables[tag] = Take(unpacking, length);
        }

        return tables;
    }

    /// <summary>
    /// WOFF2: a directory of the tables in the order they were packed, and
    /// all of them packed as one.
    /// </summary>
    private static Dictionary<string, byte[]>? Woff2(byte[] content)
    {
        // several fonts in one file, of which a page could not say which
        if (Tag(content, 4) == "ttcf")
        {
            return null;
        }

        var count = Short(content, 12);
        var packed = Long(content, 20);

        var at = 48;

        var directory = new List<(string Tag, uint Length)>();

        for (var i = 0; i < count; i++)
        {
            var flags = content[at++];

            var number = flags & 0x3F;

            string tag;

            if (number == 0x3F)
            {
                tag = Tag(content, at);
                at += 4;
            }
            else
            {
                tag = Known[number];
            }

            var length = Base128(content, ref at);

            // the drawings are packed as something else unless marked as they
            // are, everything else as it is unless marked otherwise - and what
            // is packed as something else says how long that is
            var version = flags >> 6;

            var changed = tag is "glyf" or "loca" ? version != 3 : version != 0;

            directory.Add((tag, changed ? Base128(content, ref at) : length));
        }

        if (directory.Sum(t => (long)t.Length) > MaxUnpacked)
        {
            return null;
        }

        var tables = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        using var unpacking = new BrotliStream(new MemoryStream(Slice(content, (uint)at, packed).ToArray()), CompressionMode.Decompress);

        foreach (var (tag, length) in directory)
        {
            if (Wanted.Contains(tag))
            {
                tables[tag] = Take(unpacking, length);
            }
            else
            {
                Skip(unpacking, length);
            }
        }

        return tables;
    }

    /// <summary>
    /// Reads so many bytes, which is how a table is told from the next.
    /// </summary>
    private static byte[] Take(Stream stream, uint length)
    {
        if (length > MaxUnpacked)
        {
            throw new InvalidDataException("A table of the font is larger than a font may be.");
        }

        var table = new byte[length];

        stream.ReadExactly(table);

        return table;
    }

    /// <summary>
    /// Passes over a table nothing is read of - the drawings, mostly, which
    /// are most of the font.
    /// </summary>
    private static void Skip(Stream stream, uint length)
    {
        Span<byte> passed = stackalloc byte[4096];

        for (var left = length; left > 0;)
        {
            var read = stream.Read(passed[..(int)Math.Min(left, (uint)passed.Length)]);

            if (read == 0)
            {
                throw new InvalidDataException("The font ends before its tables do.");
            }

            left -= (uint)read;
        }
    }

    /// <summary>
    /// A number as WOFF2 writes lengths: seven bits a byte, the last without
    /// the eighth.
    /// </summary>
    private static uint Base128(byte[] content, ref int at)
    {
        uint value = 0;

        for (var i = 0; i < 5; i++)
        {
            var part = content[at++];

            if ((value & 0xFE000000) != 0)
            {
                break;
            }

            value = (value << 7) | (uint)(part & 0x7F);

            if ((part & 0x80) == 0)
            {
                return value;
            }
        }

        throw new InvalidDataException("A length of the font never ends.");
    }

    #endregion

    #region Helpers

    private static ReadOnlySpan<byte> Slice(byte[] content, long offset, long length)
    {
        if (offset < 0 || length < 0 || offset + length > content.Length)
        {
            throw new InvalidDataException("A table of the font is outside of it.");
        }

        return content.AsSpan((int)offset, (int)length);
    }

    private static string Tag(byte[] content, int at) => Encoding.ASCII.GetString(Slice(content, at, 4));

    private static ushort Short(byte[] content, int at) => BinaryPrimitives.ReadUInt16BigEndian(Slice(content, at, 2));

    private static uint Long(byte[] content, int at) => BinaryPrimitives.ReadUInt32BigEndian(Slice(content, at, 4));

    private static double Fixed(byte[] content, int at) => BinaryPrimitives.ReadInt32BigEndian(Slice(content, at, 4)) / 65536.0;

    #endregion

}
