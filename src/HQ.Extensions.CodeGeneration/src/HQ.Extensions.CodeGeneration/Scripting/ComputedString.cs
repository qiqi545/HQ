using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using DotLiquid;
using HQ.Data.Contracts;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace HQ.Extensions.CodeGeneration.Scripting
{
    public static class ComputedPredicate<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, ScriptOptions> Options = new Dictionary<Type, ScriptOptions>();
            
        static ComputedPredicate()
        {
            Options[typeof(T)] = ScriptOptions.Default
                .WithReferences(typeof(T).Assembly, typeof(MemberExpression).Assembly)
                .WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");
        }

        public static Func<T, bool> AsPredicate(string expression)
        {
            return CSharpScript.EvaluateAsync<Func<T, bool>>(expression, Options[typeof(T)]).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static Expression<Func<T, bool>> AsExpression(string memberName, FilterOperator @operator, object value)
        {
            var parameter = Expression.Parameter(typeof(T), memberName);
            var memberExpression = Expression.PropertyOrField(parameter, memberName);

            BinaryExpression expression;
            switch (@operator)
            {
                case FilterOperator.Equal:
                    expression = Expression.Equal(memberExpression, Expression.Constant(value));
                    break;
                case FilterOperator.NotEqual:
                    expression = Expression.NotEqual(memberExpression, Expression.Constant(value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }

            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }
    }

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
