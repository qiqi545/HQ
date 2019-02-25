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
