// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using DotLiquid;

namespace Blowdart.UI.Web.Internal
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendTag(this StringBuilder sb, string el, object attributes = null)
        {
            return sb.OpenBlock(el, attributes).CloseBlock(el, true);
        }

        public static StringBuilder AppendTag(this StringBuilder sb, string el, string innerText,
            object attributes = null)
        {
            return sb.OpenBlock(el, attributes).Append(innerText).CloseBlock(el, true);
        }

        public static StringBuilder AppendTag(this StringBuilder sb, string el, Value128 id, string innerText,
            object attributes = null)
        {
            return sb.OpenBlock(el, id, attributes).Append(innerText).CloseBlock(el, true);
        }

        public static StringBuilder OpenBlock(this StringBuilder sb, string el, Value128 id, object attributes = null)
        {
            if (attributes == null)
                return sb.Append('<').Append(el).Append(" id='").Append(id).Append("'>");
            sb.Append('<').Append(el).Append(" id='").Append(id).Append("'").AppendAttributes(attributes).Append('>');
            return sb;
        }

        public static StringBuilder OpenBlock(this StringBuilder sb, string el, object attributes = null)
        {
            if (attributes == null)
                return sb.Append('<').Append(el).Append('>');
            sb.Append('<').Append(el).AppendAttributes(attributes).Append('>');
            return sb;
        }

        private static StringBuilder AppendAttributes(this StringBuilder sb, object attributes)
        {
            foreach (var item in Hash.FromAnonymousObject(attributes))
            {
                sb.Append(" ");
                sb.Append(item.Key);
                if (item.Value == null)
                    continue;
                sb.Append("='");
                sb.Append(item.Value);
                sb.Append("'");
            }

            return sb;
        }

        public static StringBuilder CloseBlock(this StringBuilder sb, string el, bool open = false)
        {
            if (open)
                sb.Append('<');
            return sb.Append('/').Append(el).Append('>');
        }

        public static StringBuilder AppendClick(this StringBuilder sb, Value128 id)
        {
            sb.AppendLine();
            return sb
                .AppendLine($"document.getElementById('{id}').addEventListener('click', function(e) {{ ")
                .AppendLine($"    ui.invoke('e', window.location.toString(), '{id}', 'click').catch(function (err) {{ ")
                .AppendLine("        return console.error(err.toString()); ")
                .AppendLine("    });")
                .AppendLine("    e.preventDefault();")
                .AppendLine("});");
        }
    }
}