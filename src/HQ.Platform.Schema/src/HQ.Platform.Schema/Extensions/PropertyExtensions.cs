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
using HQ.Platform.Schema.Models;

namespace HQ.Platform.Schema.Extensions
{
    public static class PropertyExtensions
    {
        public static bool IsModel(this Property property)
        {
            return property.Type == PropertyType.Object ||
                   property.Type == PropertyType.View ||
                   property.Rel  == PropertyRelationship.OneToMany ||
                   property.Rel  == PropertyRelationship.OneToOne;
        }

        public static string Label(this Property property)
        {
            return property?.Name?.Label();
        }

        public static string FullTypeString(this Property property, IDictionary<string, Models.Schema> map,
            bool parentOnly = false)
        {
            switch (property.Type)
            {
                case PropertyType.String:
                    return property.GetNativeTypeString("string");
                case PropertyType.Boolean:
                    return property.GetNativeTypeString("bool");
                case PropertyType.Byte:
                    return property.GetNativeTypeString("byte");
                case PropertyType.Single:
                    return property.GetNativeTypeString("float");
                case PropertyType.Double:
                    return property.GetNativeTypeString("double");
                case PropertyType.Decimal:
                    return property.GetNativeTypeString("decimal");
                case PropertyType.Int16:
                    return property.GetNativeTypeString("short");
                case PropertyType.Int32:
                    return property.GetNativeTypeString("int");
                case PropertyType.Int64:
                    return property.GetNativeTypeString("long");
                case PropertyType.Date:
                    return property.GetNativeTypeString("DateTime");
                case PropertyType.DateTime:
                    return property.GetNativeTypeString("DateTimeOffset");
                case PropertyType.TimeSpan:
                    return property.GetNativeTypeString("TimeSpan");

                case PropertyType.Money:
                case PropertyType.Email:
                case PropertyType.Password:
                case PropertyType.CreditCard:
                case PropertyType.Phone:
                    return property.GetNativeTypeString("string");

                case PropertyType.View:
                case PropertyType.Enum:
                case PropertyType.Object:
                    return GetDynamicTypeString(property, map, parentOnly);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static string GetDynamicTypeString(this Property property, IDictionary<string, Models.Schema> map,
            bool parentOnly = false)
        {
            switch (property.Rel)
            {
                case PropertyRelationship.Scalar:
                case PropertyRelationship.OneToOne:
                    foreach (var entry in map)
                    {
                        if (entry.Value.Name == property.From)
                        {
                            return $"{entry.Value.FullTypeString()}";
                        }
                    }
                    return "dynamic";

                case PropertyRelationship.OneToMany:
                    foreach (var entry in map)
                    {
                        if (entry.Value.Name == property.From)
                        {
                            return parentOnly
                                ? $"{entry.Value.FullTypeString()}"
                                : $"List<{entry.Value.FullTypeString()}>";
                        }
                    }
                    return parentOnly ? "dynamic" : "List<dynamic>";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetNativeTypeString(this Property property, string typeName)
        {
            switch (property.Rel)
            {
                case PropertyRelationship.Scalar:
                case PropertyRelationship.OneToOne:
                    return property.Nullable && property.Type != PropertyType.String ? $"{typeName}?" : typeName;
                case PropertyRelationship.OneToMany:
                    return $"List<{typeName}>";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
