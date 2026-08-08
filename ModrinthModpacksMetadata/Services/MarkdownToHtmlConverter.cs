using System;
using System.Collections.Generic;
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

            string text = markdown.Replace("\r\n", "\n");

            // 1. Fix GitHub blob image URLs -> raw.githubusercontent.com
            text = Regex.Replace(
                text,
                @"https://github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*?\.(png|jpg|jpeg|webp|gif))(\?raw=true)?",
                "https://raw.githubusercontent.com/$1/$2/$3/$4",
                RegexOptions.IgnoreCase);

            text = Regex.Replace(
                text,
                @"https://github\.com/([^/]+)/([^/]+)/blob/([^/]+)/(.*?)\?raw=true",
                "https://raw.githubusercontent.com/$1/$2/$3/$4",
                RegexOptions.IgnoreCase);

            // 2. Convert unsupported <iframe ...> embeds (e.g. YouTube) into clean clickable links
            text = Regex.Replace(
                text,
                @"<iframe[^>]*src=[""']([^""']+)[""'][^>]*>.*?</iframe>",
                m => FormatIframeReplacement(m.Groups[1].Value),
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            text = Regex.Replace(
                text,
                @"<iframe[^>]*src=[""']([^""']+)[""'][^>]*/>",
                m => FormatIframeReplacement(m.Groups[1].Value),
                RegexOptions.IgnoreCase);

            // 3. Convert Shields.io & SVG badge images (unsupported vector formats in Playnite MSHTML) into clean text badges
            text = Regex.Replace(
                text,
                @"<img[^>]*src=[""'][^""']*(?:img\.shields\.io|\.svg)[^""']*[""'][^>]*alt=[""']([^""']+)[""'][^>]*>",
                "<b>[ $1 ]</b>",
                RegexOptions.IgnoreCase);

            text = Regex.Replace(
                text,
                @"<img[^>]*alt=[""']([^""']+)[""'][^>]*src=[""'][^""']*(?:img\.shields\.io|\.svg)[^""']*[""'][^>]*>",
                "<b>[ $1 ]</b>",
                RegexOptions.IgnoreCase);

            text = Regex.Replace(
                text,
                @"<img[^>]*src=[""'][^""']*(?:img\.shields\.io|\.svg)[^""']*[""'][^>]*>",
                "",
                RegexOptions.IgnoreCase);

            // 4. Handle Markdown image inside Markdown link: [![alt](imgurl)](linkurl)
            text = Regex.Replace(
                text,
                @"\[!\[([^\]]*)\]\(([^)]+)\)\]\(([^)]+)\)",
                "<a href=\"$3\" target=\"_blank\"><img src=\"$2\" alt=\"$1\" style=\"max-width:100%;\" onerror=\"this.style.display='none';\" /></a>",
                RegexOptions.IgnoreCase);

            var sb = new StringBuilder();
            var lines = text.Split('\n');
            bool inList = false;
            bool inCodeBlock = false;

            foreach (var rawLine in lines)
            {
                string line = rawLine;

                // Code blocks ```
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

                // Unordered Lists (- item, * item)
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

                // Headers (# Header)
                var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
                if (headerMatch.Success)
                {
                    int level = headerMatch.Groups[1].Value.Length;
                    string content = ProcessInline(headerMatch.Groups[2].Value);
                    sb.AppendLine($"<h{level}>{content}</h{level}>");
                    continue;
                }

                // Blockquotes (> quote)
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

                // Empty / Regular lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine("<br />");
                }
                else
                {
                    string trimmed = line.TrimStart();
                    string processedLine = ProcessInline(line);

                    // Add onerror="this.style.display='none';" to any raw <img> tags to hide broken 404 images
                    processedLine = AddOnErrorToImgTags(processedLine);

                    // If line is already an HTML block tag or element, don't double-wrap in <p>...</p>
                    if (IsHtmlTag(trimmed))
                    {
                        sb.AppendLine(processedLine);
                    }
                    else
                    {
                        sb.AppendLine($"<p>{processedLine}</p>");
                    }
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

        private static string AddOnErrorToImgTags(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !text.ToLowerInvariant().Contains("<img"))
            {
                return text;
            }

            return Regex.Replace(
                text,
                @"<img\s+([^>]*?)>",
                m =>
                {
                    string tag = m.Value;
                    if (!tag.ToLowerInvariant().Contains("onerror="))
                    {
                        return tag.Insert(4, " onerror=\"this.style.display='none';\" ");
                    }
                    return tag;
                },
                RegexOptions.IgnoreCase);
        }

        private static string FormatIframeReplacement(string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return string.Empty;

            // Check for YouTube embeds
            var ytMatch = Regex.Match(src, @"(?:embed/|v=)([\w\-]+)", RegexOptions.IgnoreCase);
            if (ytMatch.Success)
            {
                string videoId = ytMatch.Groups[1].Value;
                return $"<p align=\"center\"><a href=\"https://www.youtube.com/watch?v={videoId}\" target=\"_blank\"><b>&#9654; Watch Video on YouTube</b></a></p>";
            }

            return $"<p align=\"center\"><a href=\"{src}\" target=\"_blank\">View Embedded Content</a></p>";
        }

        private static bool IsHtmlTag(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string lower = line.ToLowerInvariant();
            return lower.StartsWith("<p") || lower.StartsWith("</p") ||
                   lower.StartsWith("<div") || lower.StartsWith("</div") ||
                   lower.StartsWith("<a ") || lower.StartsWith("</a>") ||
                   lower.StartsWith("<img") ||
                   lower.StartsWith("<h1") || lower.StartsWith("<h2") || lower.StartsWith("<h3") ||
                   lower.StartsWith("<h4") || lower.StartsWith("<h5") || lower.StartsWith("<h6") ||
                   lower.StartsWith("<ul") || lower.StartsWith("<ol") || lower.StartsWith("<li") ||
                   lower.StartsWith("<table") || lower.StartsWith("<tr") || lower.StartsWith("<td") ||
                   lower.StartsWith("<blockquote") || lower.StartsWith("<hr") ||
                   lower.StartsWith("<!--");
        }

        private static string ProcessInline(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // 1. Mask existing HTML tags so Markdown regexes don't corrupt HTML attributes (like target="_blank" or image src URLs)
            var htmlTags = new List<string>();
            string text = Regex.Replace(input, @"<[^>]+>", m =>
            {
                htmlTags.Add(m.Value);
                return $"\x1AHTML_{htmlTags.Count - 1}\x1A";
            });

            // 2. Standalone Markdown Images ![alt](url) -> <img src="url" alt="alt" />
            text = Regex.Replace(text, @"!\[([^\]]*)\]\(([^)]+)\)", "<img src=\"$2\" alt=\"$1\" style=\"max-width:100%;\" onerror=\"this.style.display='none';\" />");

            // 3. Standalone Markdown Links [text](url) -> <a href="url">text</a>
            text = Regex.Replace(text, @"(?<![=\>])\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\" target=\"_blank\">$1</a>");

            // 4. Bold **text** or __text__
            text = Regex.Replace(text, @"(?:\*\*|__)(.*?)(?:\*\*|__)", "<b>$1</b>");

            // 5. Italics *text* or _text_ (strictly requiring word boundaries so _blank or image_names are untouched)
            text = Regex.Replace(text, @"(?<=\s|^|\()\*([^*]+)\*(?=\s|$|\.|,|\))", "<i>$1</i>");
            text = Regex.Replace(text, @"(?<=\s|^|\()_([^_]+)_(?=\s|$|\.|,|\))", "<i>$1</i>");

            // 6. Inline code `code`
            text = Regex.Replace(text, @"`([^`]+)`", "<code>$1</code>");

            // 7. Restore HTML tags
            for (int i = 0; i < htmlTags.Count; i++)
            {
                text = text.Replace($"\x1AHTML_{i}\x1A", htmlTags[i]);
            }

            return text;
        }
    }
}
