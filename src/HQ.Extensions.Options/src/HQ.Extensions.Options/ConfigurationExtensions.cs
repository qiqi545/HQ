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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TypeKitchen;

namespace HQ.Extensions.Options
{
    internal static class ConfigurationExtensions
    {
        public static TOptions Valid<TOptions>(this TOptions instance, IServiceProvider serviceProvider)
        {
            var results = Pooling.ListPool<ValidationResult>.Get();
            try
            {
                var context = new ValidationContext(instance, serviceProvider, null);
                Validator.TryValidateObject(instance, context, results, true);
                if (results.Count == 0)
                {
                    return instance;
                }

                var message = Pooling.StringBuilderPool.Scoped(sb =>
                {
                    sb.Append(typeof(TOptions).Name).Append(": ");
                    sb.AppendLine();

                    foreach (var result in results)
                    {
                        sb.Append(result.ErrorMessage);
                        sb.Append(" [");
                        var count = 0;
                        foreach (var field in result.MemberNames)
                        {
                            if (count != 0)
                            {
                                sb.Append(", ");
                            }

                            sb.Append(field);
                            count++;
                        }

                        sb.AppendLine("]");
                    }
                });

                throw new ValidationException(message);
            }
            finally
            {
                Pooling.ListPool<ValidationResult>.Return(results);
            }
        }

        public static bool IsValid<TOptions>(this TOptions instance, IServiceProvider serviceProvider)
        {
            var results = Pooling.ListPool<ValidationResult>.Get();
            try
            {
                var context = new ValidationContext(instance, serviceProvider, null);
                Validator.TryValidateObject(instance, context, results, true);
                return results.Count == 0;
            }
            finally
            {
                Pooling.ListPool<ValidationResult>.Return(results);
            }
        }
    }
}
