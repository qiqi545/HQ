// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace HQ.Cadence.Internal
{
    /// <summary>
    /// Serializes JSON like the rest of the universe
    /// </summary>
    internal class JsonConventionResolver : DefaultContractResolver
    {
	    protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            return CreatePropertiesImpl(properties);
        }

        private static IList<JsonProperty> CreatePropertiesImpl(IList<JsonProperty> properties)
        {
            foreach (var property in properties)
            {
                property.PropertyName = PascalCaseToElement(property.PropertyName);
            }
            
            return properties;
        }

        private static string PascalCaseToElement(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(input[0]));
            
            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsLower(input[i]))
                {
                    result.Append(input[i]);
                }
                else
                {
                    result.Append("_");
                    result.Append(char.ToLowerInvariant(input[i]));
                }
            }

            return result.ToString();
        }
    }
}