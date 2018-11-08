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

namespace HQ.Domicile.Models
{
    internal class SnakeCase : ITextTransform
    {
        public string Name => "Snake";

        public string Transform(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length > 0 && char.IsLower(input[0])) return input;

            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append(char.ToLowerInvariant(input[0]));
                for (var i = 1; i < input.Length; i++)
                    if (char.IsLower(input[i]))
                    {
                        sb.Append(input[i]);
                    }
                    else
                    {
                        sb.Append('_');
                        sb.Append(char.ToLowerInvariant(input[i]));
                    }
            });
        }
    }
}
