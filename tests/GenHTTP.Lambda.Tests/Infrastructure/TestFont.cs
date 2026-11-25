using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// A font as small as one may be - the tables that say what it is, and none
/// of the drawings - in each of the wrappers a browser takes one in.
/// </summary>
/// <remarks>
/// Made here rather than kept as a file: a real font is somebody's work under
/// a license of its own, and no test needs a letter of it drawn.
/// </remarks>
internal sealed record TestFont(string Family = "Test Sans", int Weight = 400, bool Italic = false, (int From, int To)? Axis = null, string Letters = TestFont.Latin)
{

    /// <summary>
    /// The letters of English, and those of western Europe beyond it - which
    /// is what a font cut down to Latin has.
    /// </summary>
    public const string Latin = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜŒàáâãäæçèéêëìíîïñòóôõöùúûüÿœß¡¿";

    /// <summary>
    /// The letters a font cut down to the rest of Latin has.
    /// </summary>
    public const string LatinExtended = "ĄĆĘŁŃŚŹŻąćęłńśźżĞİŞğış";

    #region Wrappers

    /// <summary>
    /// As TrueType: a directory of the tables, and the tables.
    /// </summary>
    public byte[] TrueType() => Plain([0, 1, 0, 0]);

    /// <summary>
    /// As OpenType, which differs in how its letters are drawn.
    /// </summary>
    public byte[] OpenType() => Plain("OTTO"u8.ToArray());

    /// <summary>
    /// As WOFF: each table compressed, where that makes it smaller.
    /// </summary>
    public byte[] Woff()
    {
        var tables = Tables();

        var content = new MemoryStream();

        content.Write("wOFF"u8);
        content.Write([0, 1, 0, 0]);
        Write(content, 0u);
        Write(content, (ushort)tables.Count);
        content.Write(new byte[44 - (int)content.Length]);

        var offset = 44 + tables.Count * 20;

        var packed = tables.Select(t => (t.Tag, Plain: t.Content, Packed: Smaller(t.Content))).ToList();

        foreach (var (tag, plain, kept) in packed)
        {
            content.Write(Encoding.ASCII.GetBytes(tag));
            Write(content, (uint)offset);
            Write(content, (uint)kept.Length);
            Write(content, (uint)plain.Length);
            Write(content, 0u);

            offset += kept.Length;
        }

        foreach (var (_, _, kept) in packed)
        {
            content.Write(kept);
        }

        return content.ToArray();
    }

    /// <summary>
    /// As WOFF2: all of the tables compressed as one - with the drawings among
    /// them in another form than they have, as every encoder keeps them.
    /// </summary>
    public byte[] Woff2()
    {
        var tables = Tables();

        var directory = new MemoryStream();
        var plain = new MemoryStream();

        // numbers stand in for the tables that are known; the drawings come
        // between, so what is behind them is only found by passing over them
        foreach (var (tag, content) in tables)
        {
            var number = Array.IndexOf(Known, tag);

            directory.WriteByte((byte)(number < 0 ? 63 : number));

            if (number < 0)
            {
                directory.Write(Encoding.ASCII.GetBytes(tag));
            }

            Base128(directory, content.Length);
            plain.Write(content);

            if (tag == "cmap")
            {
                directory.WriteByte(10);
                Base128(directory, 900);
                Base128(directory, 333);
                plain.Write(new byte[333]);

                directory.WriteByte(11);
                Base128(directory, 120);
                Base128(directory, 0);
            }
        }

        var packed = new MemoryStream();

        using (var brotli = new BrotliStream(packed, CompressionLevel.Optimal, leaveOpen: true))
        {
            brotli.Write(plain.ToArray());
        }

        var result = new MemoryStream();

        result.Write("wOF2"u8);
        result.Write([0, 1, 0, 0]);
        Write(result, 0u);
        Write(result, (ushort)(tables.Count + 2));
        Write(result, (ushort)0);
        Write(result, (uint)plain.Length);
        Write(result, (uint)packed.Length);
        result.Write(new byte[48 - (int)result.Length]);
        result.Write(directory.ToArray());
        result.Write(packed.ToArray());

        return result.ToArray();
    }

    private byte[] Plain(byte[] version)
    {
        var tables = Tables();

        var content = new MemoryStream();

        content.Write(version);
        Write(content, (ushort)tables.Count);
        content.Write(new byte[6]);

        var offset = 12 + tables.Count * 16;

        foreach (var (tag, table) in tables)
        {
            content.Write(Encoding.ASCII.GetBytes(tag));
            Write(content, 0u);
            Write(content, (uint)offset);
            Write(content, (uint)table.Length);

            offset += table.Length;
        }

        foreach (var (_, table) in tables)
        {
            content.Write(table);
        }

        return content.ToArray();
    }

    #endregion

    #region Tables

    private static readonly string[] Known = ["cmap", "head", "hhea", "hmtx", "maxp", "name", "OS/2", "post"];

    private List<(string Tag, byte[] Content)> Tables()
    {
        var tables = new List<(string, byte[])> { ("OS/2", Traits()), ("cmap", Map()), ("head", Head()), ("name", Names()) };

        if (Axis is { } axis)
        {
            tables.Add(("fvar", Variations(axis.From, axis.To)));
        }

        return tables;
    }

    private static byte[] Head()
    {
        var head = new byte[54];

        BinaryPrimitives.WriteUInt32BigEndian(head.AsSpan(12), 0x5F0F3CF5);

        return head;
    }

    private byte[] Traits()
    {
        var traits = new byte[96];

        BinaryPrimitives.WriteUInt16BigEndian(traits.AsSpan(4), (ushort)Weight);
        BinaryPrimitives.WriteUInt16BigEndian(traits.AsSpan(62), (ushort)(Italic ? 1 : 64));

        return traits;
    }

    /// <summary>
    /// The family's name, as Windows keeps it in English.
    /// </summary>
    private byte[] Names()
    {
        var text = Encoding.BigEndianUnicode.GetBytes(Family);

        var names = new MemoryStream();

        Write(names, (ushort)0);
        Write(names, (ushort)1);
        Write(names, (ushort)18);

        foreach (var value in (ushort[])[3, 1, 0x409, 1, (ushort)text.Length, 0])
        {
            Write(names, value);
        }

        names.Write(text);

        return names.ToArray();
    }

    /// <summary>
    /// The letters, a segment each, and the one that ends the map.
    /// </summary>
    private byte[] Map()
    {
        var letters = Letters.Distinct().Order().Select(c => (ushort)c).Append((ushort)0xFFFF).ToList();

        var map = new MemoryStream();

        Write(map, (ushort)0);
        Write(map, (ushort)1);
        Write(map, (ushort)3);
        Write(map, (ushort)1);
        Write(map, 12u);

        Write(map, (ushort)4);
        Write(map, (ushort)(16 + letters.Count * 8));
        Write(map, (ushort)0);
        Write(map, (ushort)(letters.Count * 2));
        map.Write(new byte[6]);

        foreach (var letter in letters)
        {
            Write(map, letter);
        }

        Write(map, (ushort)0);

        foreach (var letter in letters)
        {
            Write(map, letter);
        }

        // what turns a letter into the number of its drawing: any but none
        foreach (var letter in letters)
        {
            Write(map, (ushort)(letter == 0xFFFF ? 1 : (0x10000 - letter + 5) & 0xFFFF));
        }

        map.Write(new byte[letters.Count * 2]);

        return map.ToArray();
    }

    private static byte[] Variations(int from, int to)
    {
        var axes = new MemoryStream();

        foreach (var value in (ushort[])[1, 0, 16, 2, 1, 20, 0, 4])
        {
            Write(axes, value);
        }

        axes.Write("wght"u8);

        foreach (var value in (int[])[from, from, to])
        {
            Write(axes, (uint)value << 16);
        }

        Write(axes, 0u);

        return axes.ToArray();
    }

    #endregion

    #region Helpers

    private static byte[] Smaller(byte[] content)
    {
        var packed = new MemoryStream();

        using (var zlib = new ZLibStream(packed, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(content);
        }

        return packed.Length < content.Length ? packed.ToArray() : content;
    }

    private static void Base128(Stream target, int value)
    {
        if (value >= 128)
        {
            target.WriteByte((byte)(0x80 | (value >> 7)));
        }

        target.WriteByte((byte)(value & 0x7F));
    }

    private static void Write(Stream target, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);

        target.Write(bytes);
    }

    private static void Write(Stream target, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);

        target.Write(bytes);
    }

    #endregion

}
