using System.Text;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Incrementally parses markdown text and emits TextRun objects.
/// Designed for streaming: call <see cref="Append"/> with each token as it arrives.
/// The parser maintains internal state to handle multi-character markdown syntax
/// (e.g., ** for bold, ``` for code blocks) across token boundaries.
/// </summary>
public class RichTextMarkdownParser
{
    private readonly StringBuilder _lineBuffer = new();
    private bool _inCodeBlock;
    private string _codeBlockLang = "";
    private readonly StringBuilder _codeBlockContent = new();
    private bool _inTable;
    private readonly StringBuilder _tableBuffer = new();
    private bool _isFirstBlock = true;
    private bool _inMathBlock;
    private string _mathBlockType = ""; // "display" for display math
    private readonly StringBuilder _mathBlockContent = new();

    /// <summary>
    /// Append a new text token (from a streaming LLM response) and return
    /// any TextRun objects that can be emitted so far.
    /// </summary>
    public IEnumerable<TextRun> Append(string token)
    {
        var runs = new List<TextRun>();

        foreach (var ch in token)
        {
            if (_inCodeBlock)
            {
                _lineBuffer.Append(ch);
                if (ch == '\n')
                {
                    var line = _lineBuffer.ToString();
                    _lineBuffer.Clear();
                    if (line.TrimEnd() == "```")
                    {
                        runs.Add(new TextRun(_codeBlockContent.ToString()) { CodeBlock = _codeBlockLang });
                        _codeBlockContent.Clear();
                        _inCodeBlock = false;
                        _codeBlockLang = "";
                    }
                    else
                    {
                        _codeBlockContent.Append(line);
                    }
                }
                continue;
            }

            if (_inMathBlock)
            {
                _lineBuffer.Append(ch);
                if (ch == '\n')
                {
                    var line = _lineBuffer.ToString();
                    _lineBuffer.Clear();
                    var trimmed = line.TrimEnd();
                    if (trimmed == "$$" || trimmed == "\\]")
                    {
                        runs.Add(new TextRun(_mathBlockContent.ToString()) { Math = _mathBlockType });
                        _mathBlockContent.Clear();
                        _inMathBlock = false;
                        _mathBlockType = "";
                    }
                    else
                    {
                        _mathBlockContent.Append(line);
                    }
                }
                continue;
            }

            if (ch == '\n')
            {
                var line = _lineBuffer.ToString();
                _lineBuffer.Clear();
                runs.AddRange(ProcessLine(line));
            }
            else
            {
                _lineBuffer.Append(ch);
            }
        }

        return runs;
    }

    /// <summary>
    /// Signal end of stream. Flushes any remaining buffered content as final runs.
    /// </summary>
    public IEnumerable<TextRun> Flush()
    {
        var runs = new List<TextRun>();

        if (_inCodeBlock)
        {
            _codeBlockContent.Append(_lineBuffer);
            _lineBuffer.Clear();
            runs.Add(new TextRun(_codeBlockContent.ToString()) { CodeBlock = _codeBlockLang });
            _codeBlockContent.Clear();
            _inCodeBlock = false;
            return runs;
        }

        if (_inMathBlock)
        {
            _mathBlockContent.Append(_lineBuffer);
            _lineBuffer.Clear();
            runs.Add(new TextRun(_mathBlockContent.ToString()) { Math = _mathBlockType });
            _mathBlockContent.Clear();
            _inMathBlock = false;
            return runs;
        }

        if (_inTable)
        {
            _tableBuffer.Append(_lineBuffer);
            _lineBuffer.Clear();
            runs.Add(new TextRun { Table = _tableBuffer.ToString().TrimEnd() });
            _tableBuffer.Clear();
            _inTable = false;
            return runs;
        }

        var remaining = _lineBuffer.ToString();
        _lineBuffer.Clear();
        if (remaining.Length > 0)
        {
            runs.AddRange(ProcessLine(remaining));
        }

        return runs;
    }

    private List<TextRun> ProcessLine(string line)
    {
        var runs = new List<TextRun>();

        // Check for empty line (paragraph break)
        if (line.Trim().Length == 0)
        {
            if (_inTable)
            {
                runs.Add(new TextRun { Table = _tableBuffer.ToString().TrimEnd() });
                _tableBuffer.Clear();
                _inTable = false;
            }
            if (!_isFirstBlock)
            {
                runs.Add(new TextRun { LineBreak = true });
            }
            return runs;
        }

        // Table detection: line starts with |
        if (line.TrimStart().StartsWith('|'))
        {
            if (_inTable)
            {
                _tableBuffer.Append(line).Append('\n');
                return runs;
            }

            _inTable = true;
            _tableBuffer.Clear();
            _tableBuffer.Append(line).Append('\n');
            return runs;
        }

        // If we were in a table and this line doesn't start with |, flush the table
        if (_inTable)
        {
            runs.Add(new TextRun { Table = _tableBuffer.ToString().TrimEnd() });
            _tableBuffer.Clear();
            _inTable = false;
        }

        // Display math detection ($$...$$)
        var trimmedLine = line.TrimStart();
        if (trimmedLine.StartsWith("$$"))
        {
            _inMathBlock = true;
            _mathBlockType = "display";
            _mathBlockContent.Clear();
            return runs;
        }

        // Display math detection (\[...\])
        if (trimmedLine.StartsWith("\\["))
        {
            _inMathBlock = true;
            _mathBlockType = "display";
            _mathBlockContent.Clear();
            return runs;
        }

        // Code fence detection
        if (line.TrimStart().StartsWith("```"))
        {
            var lang = line.TrimStart().Substring(3).Trim();
            _inCodeBlock = true;
            _codeBlockLang = lang;
            _codeBlockContent.Clear();
            return runs;
        }

        // Horizontal rule detection (---, ***, ___)
        var hrTrimmed = line.Trim();
        if (hrTrimmed.Length >= 3 &&
            (hrTrimmed.All(c => c == '-') || hrTrimmed.All(c => c == '*') || hrTrimmed.All(c => c == '_')))
        {
            runs.Add(new TextRun { HorizontalRule = true });
            return runs;
        }

        // Heading detection
        if (line.StartsWith('#'))
        {
            var level = 0;
            while (level < line.Length && level < 6 && line[level] == '#')
                level++;
            if (level < line.Length && line[level] == ' ')
            {
                var content = line.Substring(level + 1).Trim();
                _isFirstBlock = false;
                runs.AddRange(ParseInline(content).Select(r => { r.Heading = level; return r; }));
                return runs;
            }
        }

        // Blockquote detection
        if (line.StartsWith("> ") || line == ">")
        {
            var content = line.Length > 2 ? line.Substring(2) : "";
            _isFirstBlock = false;
            runs.AddRange(ParseInline(content).Select(r => { r.Blockquote = true; return r; }));
            return runs;
        }

        // Check for indented list items first
        if (line.Length > 2)
        {
            var stripped = line.TrimStart();
            var indent = line.Length - stripped.Length;
            if (indent >= 2 && (stripped.StartsWith("- ") || stripped.StartsWith("* ")))
            {
                var content = stripped.Substring(2).Trim();
                var nestLevel = (indent / 2) + 1;
                _isFirstBlock = false;
                runs.AddRange(ParseInline(content).Select(r => { r.BulletItem = nestLevel; return r; }));
                return runs;
            }
        }

        // Unordered list detection
        if ((line.StartsWith("- ") || line.StartsWith("* ")) && !hrTrimmed.All(c => c == '*'))
        {
            var content = line.Substring(2).Trim();
            _isFirstBlock = false;
            runs.AddRange(ParseInline(content).Select(r => { r.BulletItem = 1; return r; }));
            return runs;
        }

        // Ordered list detection
        var orderedMatch = TryParseOrderedList(line);
        if (orderedMatch.HasValue)
        {
            var (num, content) = orderedMatch.Value;
            _isFirstBlock = false;
            runs.AddRange(ParseInline(content).Select(r => { r.OrderedItem = num; return r; }));
            return runs;
        }

        // Regular paragraph
        _isFirstBlock = false;

        var inlineRuns = ParseInline(line);
        if (inlineRuns.Count > 0)
        {
            inlineRuns[0].Paragraph = true;
        }
        runs.AddRange(inlineRuns);
        return runs;
    }

    private static (int number, string content)? TryParseOrderedList(string line)
    {
        var trimmed = line.TrimStart();
        var i = 0;
        while (i < trimmed.Length && char.IsDigit(trimmed[i]))
            i++;
        if (i > 0 && i < trimmed.Length - 1 && trimmed[i] == '.' && trimmed[i + 1] == ' ')
        {
            var num = int.Parse(trimmed.Substring(0, i));
            var content = trimmed.Substring(i + 2).Trim();
            return (num, content);
        }
        return null;
    }

    private static List<TextRun> ParseInline(string text)
    {
        var runs = new List<TextRun>();
        var sb = new StringBuilder();
        var i = 0;

        while (i < text.Length)
        {
            // Inline math \(...\)
            if (i + 1 < text.Length && text[i] == '\\' && text[i + 1] == '(')
            {
                FlushText(runs, sb);
                i += 2;
                var mathContent = new StringBuilder();
                while (i < text.Length)
                {
                    if (i + 1 < text.Length && text[i] == '\\' && text[i + 1] == ')')
                    {
                        i += 2;
                        break;
                    }
                    mathContent.Append(text[i]);
                    i++;
                }
                runs.Add(new TextRun(mathContent.ToString()) { Math = "inline" });
                continue;
            }

            // Inline math $...$
            if (text[i] == '$')
            {
                FlushText(runs, sb);
                i++;
                var mathContent = new StringBuilder();
                while (i < text.Length && text[i] != '$')
                {
                    mathContent.Append(text[i]);
                    i++;
                }
                if (i < text.Length) i++; // skip closing $
                runs.Add(new TextRun(mathContent.ToString()) { Math = "inline" });
                continue;
            }

            // Inline code
            if (text[i] == '`')
            {
                FlushText(runs, sb);
                i++;
                var codeContent = new StringBuilder();
                while (i < text.Length && text[i] != '`')
                {
                    codeContent.Append(text[i]);
                    i++;
                }
                if (i < text.Length) i++; // skip closing `
                runs.Add(new TextRun(codeContent.ToString()) { Code = true });
                continue;
            }

            // Bold + Italic (***text***)
            if (i + 2 < text.Length && text[i] == '*' && text[i + 1] == '*' && text[i + 2] == '*')
            {
                FlushText(runs, sb);
                i += 3;
                var content = new StringBuilder();
                while (i < text.Length)
                {
                    if (i + 2 < text.Length && text[i] == '*' && text[i + 1] == '*' && text[i + 2] == '*')
                    {
                        i += 3;
                        break;
                    }
                    content.Append(text[i]);
                    i++;
                }
                runs.Add(new TextRun(content.ToString()) { Bold = true, Italic = true });
                continue;
            }

            // Bold (**text**)
            if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
            {
                FlushText(runs, sb);
                i += 2;
                var content = new StringBuilder();
                while (i < text.Length)
                {
                    if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
                    {
                        i += 2;
                        break;
                    }
                    content.Append(text[i]);
                    i++;
                }
                runs.Add(new TextRun(content.ToString()) { Bold = true });
                continue;
            }

            // Italic (*text*)
            if (text[i] == '*')
            {
                FlushText(runs, sb);
                i++;
                var content = new StringBuilder();
                while (i < text.Length && text[i] != '*')
                {
                    content.Append(text[i]);
                    i++;
                }
                if (i < text.Length) i++; // skip closing *
                runs.Add(new TextRun(content.ToString()) { Italic = true });
                continue;
            }

            // Italic with underscore (_text_) - only at word boundaries
            if (text[i] == '_' && (i == 0 || text[i - 1] == ' '))
            {
                var closeIdx = text.IndexOf('_', i + 1);
                if (closeIdx > i + 1 && (closeIdx == text.Length - 1 || text[closeIdx + 1] == ' ' || char.IsPunctuation(text[closeIdx + 1])))
                {
                    FlushText(runs, sb);
                    var content = text.Substring(i + 1, closeIdx - i - 1);
                    runs.Add(new TextRun(content) { Italic = true });
                    i = closeIdx + 1;
                    continue;
                }
            }

            // Strikethrough (~~text~~)
            if (i + 1 < text.Length && text[i] == '~' && text[i + 1] == '~')
            {
                FlushText(runs, sb);
                i += 2;
                var content = new StringBuilder();
                while (i < text.Length)
                {
                    if (i + 1 < text.Length && text[i] == '~' && text[i + 1] == '~')
                    {
                        i += 2;
                        break;
                    }
                    content.Append(text[i]);
                    i++;
                }
                runs.Add(new TextRun(content.ToString()) { StrikeThrough = true });
                continue;
            }

            // Links [text](url)
            if (text[i] == '[')
            {
                var closeIdx = text.IndexOf(']', i + 1);
                if (closeIdx > i && closeIdx + 1 < text.Length && text[closeIdx + 1] == '(')
                {
                    var urlEnd = text.IndexOf(')', closeIdx + 2);
                    if (urlEnd > closeIdx + 2)
                    {
                        FlushText(runs, sb);
                        var linkText = text.Substring(i + 1, closeIdx - i - 1);
                        var linkUrl = text.Substring(closeIdx + 2, urlEnd - closeIdx - 2);
                        runs.Add(new TextRun(linkText) { Link = linkUrl });
                        i = urlEnd + 1;
                        continue;
                    }
                }
            }

            sb.Append(text[i]);
            i++;
        }

        FlushText(runs, sb);
        return runs;
    }

    private static void FlushText(List<TextRun> runs, StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            runs.Add(new TextRun(sb.ToString()));
            sb.Clear();
        }
    }
}
