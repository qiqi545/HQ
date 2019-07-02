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
using System.Linq;
using System.Reflection;
using Bogus.Platform;
using TypeKitchen;
using Xunit.Abstractions;

namespace HQ.Test.Sdk.Xunit.Extensions
{
    internal static class XunitSerializationInfo 
    {
        private static readonly Type[] SupportedSerializationTypes = {
            typeof (IXunitSerializable),
            typeof (char),
            typeof (char?),
            typeof (string),
            typeof (byte),
            typeof (byte?),
            typeof (sbyte),
            typeof (sbyte?),
            typeof (short),
            typeof (short?),
            typeof (ushort),
            typeof (ushort?),
            typeof (int),
            typeof (int?),
            typeof (uint),
            typeof (uint?),
            typeof (long),
            typeof (long?),
            typeof (ulong),
            typeof (ulong?),
            typeof (float),
            typeof (float?),
            typeof (double),
            typeof (double?),
            typeof (decimal),
            typeof (decimal?),
            typeof (bool),
            typeof (bool?),
            typeof (DateTime),
            typeof (DateTime?),
            typeof (DateTimeOffset),
            typeof (DateTimeOffset?)
        };
        
        internal static bool CanSerializeObject(object value)
        {
            if (value == null)
                return true;
            var type = value.GetType();
            if (!type.IsArray)
            {
                return SupportedSerializationTypes.Any(serializationType => serializationType.IsAssignableFrom(type)) ||
                       (type.IsEnum() || type.IsNullableEnum()) && type.IsFromLocalAssembly();
            }

            if (value is object[] array)
            {
                return array.All(CanSerializeObject);
            }
                
            foreach (var item in (Array)value)
            {
                if (!CanSerializeObject(item))
                    return false;
            }

            return true;
        }

        public static bool IsFromLocalAssembly(this Type type)
        {
            var name = type.Assembly.GetName().Name;
            try
            {
                Assembly.Load(new AssemblyName
                {
                    Name = name
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
