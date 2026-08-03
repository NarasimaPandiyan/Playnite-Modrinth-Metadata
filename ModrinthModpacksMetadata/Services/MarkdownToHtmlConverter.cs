using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ModrinthModpacksMetadata.Services
{
    public static class MarkdownToHtmlConverter
    {
        public static string Convert(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var lines = markdown.Replace("\r\n", "\n").Split('\n');
            bool inList = false;
            bool inCodeBlock = false;

            foreach (var rawLine in lines)
            {
                string line = rawLine;

                // Code blocks
                if (line.TrimStart().StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        sb.AppendLine("</code></pre>");
                        inCodeBlock = false;
                    }
                    else
                    {
                        if (inList)
                        {
                            sb.AppendLine("</ul>");
                            inList = false;
                        }
                        sb.AppendLine("<pre><code>");
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    sb.AppendLine(System.Net.WebUtility.HtmlEncode(line));
                    continue;
                }

                // Unordered Lists
                var listMatch = Regex.Match(line, @"^\s*[\-\*\+]\s+(.*)$");
                if (listMatch.Success)
                {
                    if (!inList)
                    {
                        sb.AppendLine("<ul>");
                        inList = true;
                    }
                    string content = ProcessInline(listMatch.Groups[1].Value);
                    sb.AppendLine($"  <li>{content}</li>");
                    continue;
                }
                else if (inList)
                {
                    sb.AppendLine("</ul>");
                    inList = false;
                }

                // Headers
                var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
                if (headerMatch.Success)
                {
                    int level = headerMatch.Groups[1].Value.Length;
                    string content = ProcessInline(headerMatch.Groups[2].Value);
                    sb.AppendLine($"<h{level}>{content}</h{level}>");
                    continue;
                }

                // Blockquotes
                var quoteMatch = Regex.Match(line, @"^\s*>\s*(.*)$");
                if (quoteMatch.Success)
                {
                    string content = ProcessInline(quoteMatch.Groups[1].Value);
                    sb.AppendLine($"<blockquote>{content}</blockquote>");
                    continue;
                }

                // Horizontal rules
                if (Regex.IsMatch(line, @"^\s*([\*\-_]\s*){3,}\s*$"))
                {
                    sb.AppendLine("<hr />");
                    continue;
                }

                // Regular lines / Paragraphs
                if (string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine("<br />");
                }
                else
                {
                    string content = ProcessInline(line);
                    sb.AppendLine($"<p>{content}</p>");
                }
            }

            if (inList)
            {
                sb.AppendLine("</ul>");
            }
            if (inCodeBlock)
            {
                sb.AppendLine("</code></pre>");
            }

            return sb.ToString();
        }

        private static string ProcessInline(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Escape HTML characters before converting markdown tags
            string text = input;

            // Images ![alt](url) -> <img src="url" alt="alt" />
            text = Regex.Replace(text, @"!\[([^\]]*)\]\(([^)]+)\)", "<img src=\"$2\" alt=\"$1\" style=\"max-width:100%;\" />");

            // Links [text](url) -> <a href="url">text</a>
            text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");

            // Bold **text** or __text__
            text = Regex.Replace(text, @"(\*\*|__)(.*?)\1", "<b>$2</b>");

            // Italics *text* or _text_
            text = Regex.Replace(text, @"(\*|_)(.*?)\1", "<i>$2</i>");

            // Inline code `code`
            text = Regex.Replace(text, @"`([^`]+)`", "<code>$1</code>");

            return text;
        }
    }
}
