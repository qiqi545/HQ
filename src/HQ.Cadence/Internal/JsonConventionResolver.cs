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
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HQ.Cadence.Internal
{
    internal class JsonConventionResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            return CreatePropertiesImpl(properties);
        }

        private static IList<JsonProperty> CreatePropertiesImpl(IList<JsonProperty> properties)
        {
            foreach (var property in properties) property.PropertyName = PascalCaseToElement(property.PropertyName);

            return properties;
        }

        private static string PascalCaseToElement(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(input[0]));

            for (var i = 1; i < input.Length; i++)
                if (char.IsLower(input[i]))
                {
                    result.Append(input[i]);
                }
                else
                {
                    result.Append("_");
                    result.Append(char.ToLowerInvariant(input[i]));
                }

            return result.ToString();
        }
    }
}
