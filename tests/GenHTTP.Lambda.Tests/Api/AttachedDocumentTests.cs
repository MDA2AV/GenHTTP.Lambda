using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Agents;
using GenHTTP.Lambda.Tests.Infrastructure;

using GenHTTP.Testing;

using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace GenHTTP.Lambda.Tests.Api;

/// <summary>
/// A PDF or an office document attached to a message, as the agent reads it:
/// its text in the order a person reads it, a scanned page as its picture, a
/// long one in parts - since the agent has nothing to open a file with but
/// the platform.
/// </summary>
[TestClass]
public sealed class AttachedDocumentTests
{

    #region PDF

    [TestMethod]
    public async Task APdfIsReadAsItsTextAndAScannedPageAsItsPicture()
    {
        var (fixture, agent) = await StartAsync();
        await using var _ = fixture;

        var pdf = Pdf();

        var menu = await AttachAsync(fixture, "menu.pdf", "application/pdf", pdf);

        var (read, pictures) = await ReadAsync(fixture, agent, menu.Id);

        Assert.AreEqual(2, read["pages"]!.GetValue<int>());

        var text = read["text"]!.GetValue<string>();

        Assert.Contains("## Page 1", text);
        Assert.Contains("Soup of the day", text, "the text of a page with text");
        Assert.Contains("## Page 2", text);
        Assert.Contains("picture 1 of this answer", text, "a page without text says where its picture is");

        Assert.AreEqual(1, pictures.Count, "the scan, as a picture the model sees");
        Assert.AreEqual("image/png", pictures[0]!["mimeType"]!.GetValue<string>());
        Assert.AreEqual(2, read["pictures"]![0]!["page"]!.GetValue<int>());
    }

    [TestMethod]
    public async Task AFileThatIsNoPdfIsSaidToBeUnreadableRatherThanFailing()
    {
        var (fixture, agent) = await StartAsync();
        await using var _ = fixture;

        var broken = await AttachAsync(fixture, "broken.pdf", "application/pdf", Encoding.ASCII.GetBytes("%PDF-1.7 nothing that follows makes sense"));

        var (read, _) = await ReadAsync(fixture, agent, broken.Id);

        Assert.Contains("could not be read", read["note"]!.GetValue<string>());
    }

    #endregion

    #region Office

    [TestMethod]
    public async Task AWordDocumentIsReadAsParagraphsAndItsTablesAsRows()
    {
        var (fixture, agent) = await StartAsync();
        await using var _ = fixture;

        var document = Package(
            ("_rels/.rels", Relationships(("rId1", "officeDocument", "word/document.xml"))),
            ("word/document.xml", Word(
                "<w:p><w:pPr><w:tabs><w:tab w:val=\"left\" w:pos=\"720\"/></w:tabs></w:pPr><w:r><w:t>Opening hours</w:t></w:r></w:p>",
                "<w:tbl><w:tr><w:tc><w:p><w:r><w:t>Monday</w:t></w:r></w:p></w:tc><w:tc><w:p><w:r><w:t>closed</w:t></w:r></w:p></w:tc></w:tr>"
              + "<w:tr><w:tc><w:p><w:r><w:t>Tuesday</w:t></w:r></w:p></w:tc><w:tc><w:p><w:r><w:t>9 - 18</w:t></w:r></w:p></w:tc></w:tr></w:tbl>",
                "<w:p><w:del><w:r><w:delText>struck out</w:delText></w:r></w:del><w:r><w:t xml:space=\"preserve\">Call </w:t></w:r><w:r><w:t>us</w:t></w:r></w:p>")));

        // a browser on a machine without an office suite knows no kind for it
        var hours = await AttachAsync(fixture, "hours.docx", "application/octet-stream", document);

        Assert.AreEqual("application/vnd.openxmlformats-officedocument.wordprocessingml.document", hours.Type, "the extension says what it is");

        var (read, _) = await ReadAsync(fixture, agent, hours.Id);

        Assert.AreEqual("Opening hours\nMonday\tclosed\nTuesday\t9 - 18\nCall us", read["text"]!.GetValue<string>(),
                        "paragraphs as lines, cells with a tab between them, deleted text left out");
    }

    [TestMethod]
    public async Task AnExcelWorkbookIsReadSheetBySheetWithItsCellsInTheirColumns()
    {
        var (fixture, agent) = await StartAsync();
        await using var _ = fixture;

        var workbook = Package(
            ("_rels/.rels", Relationships(("rId1", "officeDocument", "xl/workbook.xml"))),
            ("xl/workbook.xml", Spreadsheet("workbook", "<sheets><sheet name=\"Prices\" sheetId=\"1\" r:id=\"rId1\"/></sheets>")),
            ("xl/_rels/workbook.xml.rels", Relationships(("rId1", "worksheet", "worksheets/sheet1.xml"), ("rId2", "sharedStrings", "sharedStrings.xml"))),
            ("xl/sharedStrings.xml", Spreadsheet("sst", "<si><t>Coffee</t></si><si><r><t>Tea</t></r><r><t> (large)</t></r></si>")),
            ("xl/worksheets/sheet1.xml", Spreadsheet("worksheet",
                "<sheetData><row r=\"1\"><c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"C1\"><v>3.5</v></c></row>"
              + "<row r=\"2\"><c r=\"A2\" t=\"s\"><v>1</v></c><c r=\"B2\" t=\"inlineStr\"><is><t>hot</t></is></c><c r=\"C2\"><f>C1+1</f><v>4.5</v></c></row></sheetData>")));

        var prices = await AttachAsync(fixture, "prices.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", workbook);

        var (read, _) = await ReadAsync(fixture, agent, prices.Id);

        Assert.AreEqual(1, read["pages"]!.GetValue<int>(), "its sheets");
        Assert.AreEqual("## Sheet: Prices\nCoffee\t\t3.5\nTea (large)\thot\t4.5", read["text"]!.GetValue<string>(),
                        "an empty cell keeps the columns, a formula is read as its value");
    }

    [TestMethod]
    public async Task ALongDocumentIsReadInParts()
    {
        var (fixture, agent) = await StartAsync();
        await using var _ = fixture;

        var paragraphs = Enumerable.Range(1, 1500).Select(i => $"<w:p><w:r><w:t>Paragraph {i:D4} of a long document, which goes on for a while.</w:t></w:r></w:p>");

        var document = Package(("_rels/.rels", Relationships(("rId1", "officeDocument", "word/document.xml"))), ("word/document.xml", Word([.. paragraphs])));

        var book = await AttachAsync(fixture, "book.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", document);

        var (first, _) = await ReadAsync(fixture, agent, book.Id);

        var next = first["next"]!.GetValue<int>();

        Assert.StartsWith("Paragraph 0001", first["text"]!.GetValue<string>());

        var (second, _) = await ReadAsync(fixture, agent, book.Id, next);

        Assert.AreEqual(next, second["from"]!.GetValue<int>());
        Assert.IsNull(second["next"], "the rest fits into the second part");
        Assert.EndsWith("Paragraph 1500 of a long document, which goes on for a while.", second["text"]!.GetValue<string>());

        var whole = first["text"]!.GetValue<string>() + "\n" + second["text"]!.GetValue<string>();

        Assert.AreEqual(1500, whole.Split('\n').Length, "nothing lost or read twice between the parts");
    }

    #endregion

    #region Helpers

    private static async Task<(LambdaFixture Fixture, FakeAgentProvider Agent)> StartAsync()
    {
        var agent = new FakeAgentProvider();

        var fixture = await LambdaFixture.CreateWithAgentAsync(agent, o => o with { AdminToken = LambdaFixture.OperatorToken });

        var lambda = await fixture.CreateLambdaAsync("documents");

        using var response = await fixture.SendAsync(HttpMethod.Post, $"/api/v1/lambdas/{lambda.PrivateKey}/agent/start", new ChangeRequest("Build a page from what I sent"));

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode, await response.Content.ReadAsStringAsync());

        return (fixture, agent);
    }

    private static async Task<RunFile> AttachAsync(LambdaFixture fixture, string name, string type, byte[] content)
    {
        using var request = fixture.Host.GetRequest($"/api/v1/attachments?name={Uri.EscapeDataString(name)}&type={Uri.EscapeDataString(type)}", HttpMethod.Post);

        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await fixture.Host.GetResponseAsync(request);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await response.GetContentAsync<RunFile>();
    }

    /// <summary>
    /// What the agent of the run is answered: the answer as the model reads
    /// it (the first block), and the pictures after it.
    /// </summary>
    private static async Task<(JsonNode Answer, JsonArray Pictures)> ReadAsync(LambdaFixture fixture, FakeAgentProvider agent, string id, int? from = null)
    {
        using var request = fixture.Host.GetRequest("/mcp", HttpMethod.Post);

        request.Headers.Add("Accept", "application/json, text/event-stream");
        request.Headers.Add(IAgentService.CredentialHeader, agent.Last.Credential);

        var arguments = new JsonObject { ["attachment"] = id };

        if (from != null)
        {
            arguments["from"] = from;
        }

        request.Content = new StringContent(new JsonObject
        {
            ["jsonrpc"] = "2.0", ["id"] = 1, ["method"] = "tools/call",
            ["params"] = new JsonObject { ["name"] = "read_attachment", ["arguments"] = arguments }
        }.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await fixture.Host.GetResponseAsync(request);

        var result = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["result"]!;

        Assert.IsFalse(result["isError"]?.GetValue<bool>() ?? false, result.ToJsonString());

        var content = result["content"]!.AsArray();

        var answer = JsonNode.Parse(content[0]!["text"]!.GetValue<string>())!;

        return (answer, [.. content.Skip(1).Where(c => c!["type"]!.GetValue<string>() == "image").Select(c => c!.DeepClone())]);
    }

    /// <summary>A PDF of two pages: one with text, one that is nothing but a picture of a page.</summary>
    private static byte[] Pdf()
    {
        var builder = new PdfDocumentBuilder();

        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        var text = builder.AddPage(PageSize.A4);

        text.AddText("Soup of the day", 14, new PdfPoint(50, 750), font);

        var scan = builder.AddPage(PageSize.A4);

        scan.AddPng(Png(300, 400), new PdfRectangle(0, 0, 595, 842));

        return builder.Build();
    }

    /// <summary>A grey PNG of the given size - as large as a scan needs to be to count as one.</summary>
    private static byte[] Png(int width, int height)
    {
        using var output = new MemoryStream();

        output.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);

        var header = new byte[13];

        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);

        header[8] = 8; // bits per sample
        header[9] = 0; // grey

        Chunk(output, "IHDR", header);

        using var pixels = new MemoryStream();

        using (var deflate = new ZLibStream(pixels, CompressionLevel.Fastest, true))
        {
            var row = new byte[width + 1];

            Array.Fill(row, (byte)0xC0, 1, width);

            for (var y = 0; y < height; y++)
            {
                deflate.Write(row);
            }
        }

        Chunk(output, "IDAT", pixels.ToArray());
        Chunk(output, "IEND", []);

        return output.ToArray();
    }

    private static void Chunk(Stream output, string kind, byte[] data)
    {
        var length = new byte[4];

        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);

        output.Write(length);

        var typed = Encoding.ASCII.GetBytes(kind).Concat(data).ToArray();

        output.Write(typed);

        var crc = new byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(typed));

        output.Write(crc);
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;

        foreach (var b in data)
        {
            crc ^= b;

            for (var k = 0; k < 8; k++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        return ~crc;
    }

    private static byte[] Package(params (string Path, string Content)[] parts)
    {
        using var output = new MemoryStream();

        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            foreach (var (path, content) in parts)
            {
                using var entry = zip.CreateEntry(path).Open();

                entry.Write(Encoding.UTF8.GetBytes(content));
            }
        }

        return output.ToArray();
    }

    private static string Relationships(params (string Id, string Kind, string Target)[] relationships)
        => "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
         + string.Concat(relationships.Select(r => $"<Relationship Id=\"{r.Id}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/{r.Kind}\" Target=\"{r.Target}\"/>"))
         + "</Relationships>";

    private static string Word(params string[] body)
        => "<?xml version=\"1.0\" encoding=\"UTF-8\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>"
         + string.Concat(body) + "<w:sectPr/></w:body></w:document>";

    private static string Spreadsheet(string root, string content)
        => $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><{root} xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
         + $"xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">{content}</{root}>";

    #endregion

}
