#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using HQ.Common.Helpers;
using Microsoft.CodeAnalysis.CSharp;

namespace HQ.Platform.Schema.Extensions
{
    public static class StringExtensions
    {
        public static string Label(this string value)
        {
            return value?.Identifier()?.ToTitleCase();
        }

        private static string ToTitleCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            if (char.IsUpper(value[0]))
                return char.ToUpperInvariant(value[0]) + value.Substring(1);
            return value;
        }

        public static string Identifier(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "_";
            }

            if (SyntaxFacts.IsValidIdentifier(value))
            {
                return value;
            }

            return StringBuilderPool.Scoped(sb =>
            {
                if (IsCSharpKeyword(value))
                {
                    sb.Append("@");
                }

                foreach (var c in value)
                {
                    if (SyntaxFacts.IsIdentifierPartCharacter(c))
                    {
                        sb.Append(c);
                    }
                }

                sb.Append(value);
            });
        }

        // Source: http://source.roslyn.io/#BoundTreeGenerator/BoundNodeClassWriter.cs,1dcced07beac9209
        // License: Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
        public static bool IsCSharpKeyword(this string name)
        {
            switch (name)
            {
                case "bool":
                case "byte":
                case "sbyte":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "double":
                case "float":
                case "decimal":
                case "string":
                case "char":
                case "object":
                case "typeof":
                case "sizeof":
                case "null":
                case "true":
                case "false":
                case "if":
                case "else":
                case "while":
                case "for":
                case "foreach":
                case "do":
                case "switch":
                case "case":
                case "default":
                case "lock":
                case "try":
                case "throw":
                case "catch":
                case "finally":
                case "goto":
                case "break":
                case "continue":
                case "return":
                case "public":
                case "private":
                case "internal":
                case "protected":
                case "static":
                case "readonly":
                case "sealed":
                case "const":
                case "new":
                case "override":
                case "abstract":
                case "virtual":
                case "partial":
                case "ref":
                case "out":
                case "in":
                case "where":
                case "params":
                case "this":
                case "base":
                case "namespace":
                case "using":
                case "class":
                case "struct":
                case "interface":
                case "delegate":
                case "checked":
                case "get":
                case "set":
                case "add":
                case "remove":
                case "operator":
                case "implicit":
                case "explicit":
                case "fixed":
                case "extern":
                case "event":
                case "enum":
                case "unsafe":
                    return true;
                default:
                    return false;
            }
        }
    }
}
