using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Ivy.Tendril.Services;

public class PlanPdfService
{
    public byte[] GeneratePdf(string title, int planId, string markdownContent)
    {
        using var ms = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page);
                AddHeader(page, planId);
                AddContent(page, title, markdownContent);
                AddFooter(page);
            });
        }).GeneratePdf(ms);

        return ms.ToArray();
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(QuestPDF.Helpers.PageSizes.A4);
        page.Margin(30);
        page.DefaultTextStyle(t => t.FontSize(12));
    }

    private void AddHeader(PageDescriptor page, int planId)
    {
        page.Header()
            .Text($"Ivy Tendril — Plan #{planId}")
            .SemiBold()
            .FontSize(14);
    }

    private void AddContent(PageDescriptor page, string title, string markdown)
    {
        page.Content().Column(col =>
        {
            col.Spacing(10);

            // Title
            col.Item().Text(title).FontSize(20).SemiBold();

            // Render Markdown content directly
            col.Item().Column(md =>
            {
                var text = markdown ?? string.Empty;
                var lines = text.Replace("\r\n", "\n").Split('\n');

                bool IsTableRow(string s) => s.TrimStart().StartsWith("|") && s.Contains('|');

                bool IsSepCell(string cell)
                {
                    cell = cell.Trim();
                    if (cell.Length == 0) return false;
                    foreach (var ch in cell)
                        if (ch != '-' && ch != ':' && ch != ' ') return false;
                    return true;
                }

                void RenderTable(int startIndex, out int consumed)
                {
                    var rows = new List<string[]>();
                    var k = startIndex;
                    while (k < lines.Length && IsTableRow(lines[k]))
                    {
                        var cells = lines[k].Trim();
                        if (cells.StartsWith("|")) cells = cells[1..];
                        if (cells.EndsWith("|")) cells = cells[..^1];
                        var parts = cells.Split('|').Select(x => x.Trim()).ToArray();
                        rows.Add(parts);
                        k++;
                    }
                    consumed = k - startIndex;
                    if (rows.Count == 0) return;
                    var header = rows[0];
                    int dataStart = 1;
                    if (rows.Count > 1 && rows[1].All(IsSepCell)) dataStart = 2;

                    md.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { for (int ci = 0; ci < header.Length; ci++) c.RelativeColumn(); });
                        t.Header(h => { foreach (var cell in header) h.Cell().Text(cell).SemiBold(); });
                        for (int ri = dataStart; ri < rows.Count; ri++)
                        {
                            var row = rows[ri];
                            for (int ci = 0; ci < header.Length; ci++)
                            {
                                var val = ci < row.Length ? row[ci] : string.Empty;
                                t.Cell().Text(val);
                            }
                        }
                    });
                }

                // helper: compute indentation level (2 spaces or 1 tab per level)
                static int GetIndentLevel(string s)
                {
                    int total = 0;
                    for (int i = 0; i < s.Length; i++)
                    {
                        var c = s[i];
                        if (c == ' ') total += 1;
                        else if (c == '\t') total += 2;
                        else break;
                    }
                    return total / 2;
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var trimmed = line.TrimStart();
                    var indentLevel = GetIndentLevel(line);
                    var basePadding = 8 + indentLevel * 12; // 12 px per level for visibility

                    // preserve blank lines between elements
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        md.Item().Text("\u00A0");
                        continue;
                    }

                    if (IsTableRow(line))
                    {
                        RenderTable(i, out int consumed);
                        i += consumed - 1;
                        continue;
                    }

                    if (trimmed.StartsWith("# "))
                    {
                        md.Item().Text(t => t.Span(trimmed[2..]).FontSize(18).SemiBold());
                    }
                    else if (trimmed.StartsWith("## "))
                    {
                        md.Item().Text(t => t.Span(trimmed[3..]).FontSize(16).SemiBold());
                    }
                    else if (trimmed.StartsWith("### "))
                    {
                        md.Item().Text(t => t.Span(trimmed[4..]).FontSize(14).SemiBold());
                    }
                    else if (trimmed.StartsWith("- [x]") || trimmed.StartsWith("- [X]"))
                    {
                        var checkboxText = trimmed.Substring(5).Trim();
                        md.Item().PaddingVertical(2).PaddingLeft(basePadding).Row(row =>
                        {
                            row.ConstantItem(14).Element(c =>
                                c.Border(1).Width(14).Height(14).MinWidth(14).MaxWidth(14).AlignCenter().Text(t => t.Span("✓").FontSize(10))
                            );
                            row.RelativeItem().PaddingLeft(4).Text(t => RenderInline(t, checkboxText));
                        });
                    }
                    else if (trimmed.StartsWith("- [ ]"))
                    {
                        var checkboxText = trimmed.Substring(5).Trim();
                        md.Item().PaddingVertical(2).PaddingLeft(basePadding).Row(row =>
                        {
                            row.ConstantItem(14).Element(c =>
                                c.Border(1).Width(14).Height(14).MinWidth(14).MaxWidth(14)
                            );
                            row.RelativeItem().PaddingLeft(4).Text(t => RenderInline(t, checkboxText));
                        });
                    }
                    else if (trimmed.StartsWith("[x]") || trimmed.StartsWith("[X]"))
                    {
                        var checkboxText = trimmed.Substring(3).Trim();
                        md.Item().PaddingVertical(2).Row(row =>
                        {
                            row.ConstantItem(14).Element(c =>
                                c.Border(1).Width(14).Height(14).MinWidth(14).MaxWidth(14).AlignCenter().Text(t => t.Span("✓").FontSize(10))
                            );
                            row.RelativeItem().PaddingLeft(4).Text(t => RenderInline(t, checkboxText));
                        });
                    }
                    else if (trimmed.StartsWith("[ ]"))
                    {
                        var checkboxText = trimmed.Substring(3).Trim();
                        md.Item().PaddingVertical(2).Row(row =>
                        {
                            row.ConstantItem(14).Element(c =>
                                c.Border(1).Width(14).Height(14).MinWidth(14).MaxWidth(14)
                            );
                            row.RelativeItem().PaddingLeft(4).Text(t => RenderInline(t, checkboxText));
                        });
                    }
                    else if (trimmed.StartsWith("- "))
                    {
                        var bulletText = trimmed.Substring(2).Trim();
                        md.Item().PaddingLeft(basePadding).Text(t =>
                        {
                            t.Span("• ").FontSize(12);
                            RenderInline(t, bulletText);
                        });
                    }
                    else if (trimmed.StartsWith(">"))
                    {
                        var quoteText = trimmed.Substring(1).Trim();
                        if (trimmed.StartsWith(">>"))
                        {
                            // Nested quote
                            var nestedText = trimmed.Substring(2).Trim();
                            md.Item().PaddingLeft(32).Text(t => t.Span(nestedText).Italic());
                        }
                        else
                        {
                            // Regular quote
                            md.Item().PaddingLeft(16).Text(t => t.Span(quoteText).Italic());
                        }
                    }
                    else if (IsNumberedList(trimmed))
                    {
                        var listText = ExtractNumberedListText(trimmed);
                        var number = ExtractNumberedListNumber(trimmed);
                        md.Item().PaddingLeft(basePadding).Text(t =>
                        {
                            t.Span($"{number}. ").SemiBold();
                            RenderInline(t, listText);
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        md.Item().Text(t => RenderInline(t, line));
                    }
                }

                // Helper methods for numbered lists
                static bool IsNumberedList(string line)
                {
                    var trimmed = line.TrimStart();
                    if (trimmed.Length < 3) return false;

                    // Must start with digit/letter followed by dot and space
                    if (char.IsDigit(trimmed[0]) && trimmed[1] == '.' && trimmed[2] == ' ')
                        return true;

                    // Roman numerals (I., II., III., etc.)
                    if (IsRomanNumeral(trimmed) && trimmed.Contains(". ") && trimmed.IndexOf(". ") == 1)
                        return true;

                    // Letter sequences (a., b., c., etc.)
                    if (char.IsLetter(trimmed[0]) && trimmed[1] == '.' && trimmed[2] == ' ')
                        return true;

                    return false;
                }

                static bool IsRomanNumeral(string line)
                {
                    if (line.Length < 3) return false;
                    var dotIndex = line.IndexOf('.');
                    if (dotIndex < 1) return false;
                    var roman = line.Substring(0, dotIndex);
                    return roman.All(c => "IVXLCDM".Contains(c));
                }

                static bool IsLetterSequence(string line)
                {
                    if (line.Length < 3) return false;
                    var letter = line[0];
                    return char.IsLetter(letter) && line[1] == '.';
                }

                static string ExtractNumberedListText(string line)
                {
                    var dotIndex = line.IndexOf('.');
                    return dotIndex >= 0 ? line.Substring(dotIndex + 1).Trim() : line;
                }

                static string ExtractNumberedListNumber(string line)
                {
                    var dotIndex = line.IndexOf('.');
                    if (dotIndex < 0) return "1";

                    var numberPart = line.Substring(0, dotIndex).Trim();

                    // Arabic numerals (1, 2, 3, ...)
                    if (int.TryParse(numberPart, out int arabic))
                    {
                        return arabic.ToString();
                    }

                    // Roman numerals (I, II, III, IV, V, ...)
                    if (IsRomanNumeral(line))
                    {
                        return numberPart;
                    }

                    // Letter sequences (a, b, c, ... or A, B, C, ...)
                    if (IsLetterSequence(line))
                    {
                        return numberPart;
                    }

                    return "1";
                }

                void RenderInline(TextDescriptor t, string text)
                {
                    var sb = new StringBuilder();
                    bool isBold = false, isItalic = false;
                    int i = 0;

                    void Flush()
                    {
                        if (sb.Length == 0) return;
                        var span = t.Span(sb.ToString());
                        if (isBold) span.SemiBold();
                        if (isItalic) span.Italic();
                        sb.Clear();
                    }

                    while (i < text.Length)
                    {
                        // bold **
                        if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
                        {
                            Flush();
                            isBold = !isBold;
                            i += 2;
                            continue;
                        }
                        // italic *
                        if (text[i] == '*')
                        {
                            Flush();
                            isItalic = !isItalic;
                            i += 1;
                            continue;
                        }
                        // link [text](url)
                        if (text[i] == '[')
                        {
                            // find closing
                            int closeBracket = text.IndexOf(']', i + 1);
                            int openParen = closeBracket >= 0 ? text.IndexOf('(', closeBracket + 1) : -1;
                            int closeParen = openParen >= 0 ? text.IndexOf(')', openParen + 1) : -1;
                            if (closeBracket > i && openParen == closeBracket + 1 && closeParen > openParen)
                            {
                                Flush();
                                var linkText = text.Substring(i + 1, closeBracket - (i + 1));
                                var linkSpan = t.Span(linkText).Underline();
                                if (isBold) linkSpan.SemiBold();
                                if (isItalic) linkSpan.Italic();
                                i = closeParen + 1;
                                continue;
                            }
                        }
                        // default character
                        sb.Append(text[i]);
                        i++;
                    }
                    Flush();
                }
            });
        });
    }

    private void AddFooter(PageDescriptor page)
    {
        page.Footer().AlignRight().Text(t =>
        {
            t.CurrentPageNumber();
            t.Span(" / ");
            t.TotalPages();
        });
    }
}
