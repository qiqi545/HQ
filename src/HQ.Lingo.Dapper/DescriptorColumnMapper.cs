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
using System.Reflection;
using Dapper;
using HQ.Lingo.Descriptor;

namespace HQ.Lingo.Dapper
{
    public class DescriptorColumnMapper : SqlMapper.ITypeMap
    {
        private readonly IDictionary<string, PropertyToColumnMemberMap> _memberMap;
        private readonly Type _type;

        public static void AddTypeMap<T>(IEqualityComparer<string> comparer)
        {
            var descriptor = SimpleDataDescriptor.Create<T>();
            SqlMapper.SetTypeMap(typeof(T), new DescriptorColumnMapper(typeof(T), descriptor.All, comparer));
        }

        public DescriptorColumnMapper(Type type, IEnumerable<PropertyToColumn> columns, IEqualityComparer<string> comparer)
        {
            _type = type;
            _memberMap = new Dictionary<string, PropertyToColumnMemberMap>(comparer);
            foreach (var column in columns)
                _memberMap.Add(column.ColumnName, new PropertyToColumnMemberMap(column));
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            _memberMap.TryGetValue(columnName, out var value);
            return value;
        }

        private class PropertyToColumnMemberMap : SqlMapper.IMemberMap
        {
            private readonly PropertyToColumn _propertyToColumn;
            private readonly PropertyInfo _underlyingProperty;

            public PropertyToColumnMemberMap(PropertyToColumn propertyToColumn)
            {
                _propertyToColumn = propertyToColumn;
                _underlyingProperty = propertyToColumn.Property.Info;
            }

            PropertyInfo SqlMapper.IMemberMap.Property => _underlyingProperty;
            Type SqlMapper.IMemberMap.MemberType => _underlyingProperty.PropertyType;

            #region Unused

            public string ColumnName => _propertyToColumn.ColumnName;

            FieldInfo SqlMapper.IMemberMap.Field => null;

            ParameterInfo SqlMapper.IMemberMap.Parameter => null;

            #endregion
        }

        #region Unused

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _type.GetConstructor(types) ?? _type.GetConstructor(Type.EmptyTypes);
        }

        public ConstructorInfo FindExplicitConstructor()
        {
            return _type.GetConstructor(new Type[0]);
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            return null;
        }

        #endregion
    }
}

