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

using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using HQ.Extensions.CodeGeneration.Internal;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using TypeKitchen;

namespace HQ.Extensions.CodeGeneration.Scripting
{
    public static class ComputedString
    {
        private static readonly ScriptOptions Options;

        static ComputedString()
        {
            Options = ScriptOptions.Default
                .WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
                .WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");
        }

        public static string Compute(object @this, string expression)
        {
            //
            // Pass 1: Resolve any {{ Member }} against self.
            var code = StringBuilderPool.Scoped(sb =>
            {
                var accessor = ReadAccessor.Create(@this.GetType());
                foreach (Match match in Regex.Matches(expression, @"{{([a-zA-Z\s]+)}}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled))
                {
                    var key = match.Groups[1].Value;

                    if (accessor.TryGetValue(@this, key, out var value))
                        expression = expression.Replace(match.Groups[0].Value, value.ToString());
                }
                sb.Append($"return \"{expression}\";");
            });
            

            //
            // Pass 2: Execute script in context.
            var script = CSharpScript.Create(code, Options, typeof(ScriptContext));
            var context = new ScriptContext {@this = @this};
            var state = script.RunAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
            return state.ReturnValue.ToString();
        }
    }
}
