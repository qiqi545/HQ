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
using System.Globalization;
using System.Linq.Expressions;
using DotLiquid;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

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
            // Pass 1: Resolve any {{ Property }} against self.
            var template = Template.Parse(expression);
            var parameters = new RenderParameters(CultureInfo.InvariantCulture);
            parameters.LocalVariables = Hash.FromAnonymousObject(@this, true);
            var code = $"return \"{template.Render(parameters)}\";";

            //
            // Pass 2: Execute script in context.
            var script = CSharpScript.Create(code, Options, typeof(ScriptContext));
            var context = new ScriptContext {@this = @this};
            var state = script.RunAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
            return state.ReturnValue.ToString();
        }
    }
}
