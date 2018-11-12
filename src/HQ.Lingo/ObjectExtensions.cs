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

namespace HQ.Lingo
{
    internal static class ObjectExtensions
    {
        public static bool Implements(this object instance, Type interfaceType)
        {
            return interfaceType.IsGenericTypeDefinition
                ? instance.ImplementsGeneric(interfaceType)
                : interfaceType.IsInstanceOfType(instance);
        }

        public static bool Implements<T>(this object instance)
        {
            var type = instance.GetType();
            var interfaceType = typeof(T);
            return interfaceType.IsGenericTypeDefinition
                ? instance.ImplementsGeneric(interfaceType)
                : interfaceType.IsAssignableFrom(type);
        }

        private static bool ImplementsGeneric(this Type type, Type targetType)
        {
            var interfaceTypes = type.GetInterfaces();
            if (interfaceTypes.Where(interfaceType => interfaceType.IsGenericType).Any(interfaceType =>
                interfaceType.GetGenericTypeDefinition() == targetType)) return true;

            var baseType = type.BaseType;
            if (baseType == null) return false;

            return baseType.IsGenericType
                ? baseType.GetGenericTypeDefinition() == targetType
                : baseType.ImplementsGeneric(targetType);
        }

        private static bool ImplementsGeneric(this object instance, Type targetType)
        {
            return instance.GetType().ImplementsGeneric(targetType);
        }

        public static Type GetDeclaredTypeForGeneric(this object instance, Type interfaceType)
        {
            return instance.GetType().GetDeclaredTypeForGeneric(interfaceType);
        }

        public static Type GetDeclaredTypeForGeneric(this Type baseType, Type interfaceType)
        {
            var type = default(Type);

            if (baseType.ImplementsGeneric(interfaceType))
                if (interfaceType != null && interfaceType.FullName != null)
                {
                    var generic = baseType.GetInterface(interfaceType.FullName, true);
                    if (generic.IsGenericType)
                    {
                        var args = generic.GetGenericArguments();
                        if (args.Length == 1) type = args[0];
                    }
                }

            return type;
        }
    }
}
