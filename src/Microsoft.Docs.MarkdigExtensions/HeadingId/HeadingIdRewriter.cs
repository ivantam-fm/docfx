// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Text.RegularExpressions;

using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Microsoft.Docs.MarkdigExtensions
{
    public class HeadingIdRewriter : IMarkdownObjectRewriter
    {
        private static readonly Regex s_openARegex = new(@"^\<a +(?:name|id)=\""([\w \-\.]+)\"" *\>$", RegexOptions.Compiled);
        private static readonly Regex s_closeARegex = new(@"^\<\/a\>$", RegexOptions.Compiled);

        public void PostProcess(IMarkdownObject markdownObject)
        {
        }

        public void PreProcess(IMarkdownObject markdownObject)
        {
        }

        public IMarkdownObject Rewrite(IMarkdownObject markdownObject)
        {
            if (markdownObject is HeadingBlock block)
            {
                if (block.Inline.Count() <= 2)
                {
                    return block;
                }

                var id = ParseHeading(block);
                if (string.IsNullOrEmpty(id))
                {
                    return block;
                }

                var attribute = block.GetAttributes();
                attribute.Id = id;

                return RemoveHtmlTag(block);
            }

            return markdownObject;
        }

        private static HeadingBlock RemoveHtmlTag(HeadingBlock block)
        {
            var inlines = block.Inline.Skip(2);
            block.Inline = new ContainerInline();
            foreach (var inline in inlines)
            {
                inline.Remove();
                block.Inline.AppendChild(inline);
            }
            return block;
        }

        private static string ParseHeading(HeadingBlock headBlock)
        {
            var tokens = headBlock.Inline.ToList();
            if (tokens[0] is not HtmlInline openATag || tokens[1] is not HtmlInline closeATag)
            {
                return null;
            }

            var m = s_openARegex.Match(openATag.Tag);
            if (!m.Success)
            {
                return null;
            }
            if (!s_closeARegex.IsMatch(closeATag.Tag))
            {
                return null;
            }

            return m.Groups[1].Value;
        }
    }
}