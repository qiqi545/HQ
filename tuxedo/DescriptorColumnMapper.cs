using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper;
using table_descriptor;

namespace tuxedo
{
    /// <summary> Intercept the DB's column name in a result set and give it back to the real C# property </summary>
    public class DescriptorColumnMapper : SqlMapper.ITypeMap
    {
        private readonly Type _type;
        private readonly IDictionary<string, PropertyToColumnMemberMap> _memberMap;

        public DescriptorColumnMapper(Type type, IEnumerable<PropertyToColumn> columns)
        {
            _type = type;
            _memberMap = new Dictionary<string, PropertyToColumnMemberMap>();
            foreach (var column in columns)
            {
                _memberMap.Add(column.ColumnName.ToLower(), new PropertyToColumnMemberMap(column));
            }
        }
        
        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            PropertyToColumnMemberMap value;
            _memberMap.TryGetValue(columnName.ToLower(), out value);
            return value;
        }

        #region Unused
        
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _type.GetConstructor(types) ?? _type.GetConstructor(new Type[0]);
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

        private class PropertyToColumnMemberMap : SqlMapper.IMemberMap
        {
            private readonly PropertyInfo _underlyingProperty;
            private readonly string _columnName;

            public PropertyToColumnMemberMap(PropertyToColumn propertyToColumn)
            {
                _underlyingProperty = propertyToColumn.Property.PropertyInfo;
                _columnName = propertyToColumn.ColumnName;
            }

            PropertyInfo SqlMapper.IMemberMap.Property
            {
                get { return _underlyingProperty; }
            }

            Type SqlMapper.IMemberMap.MemberType
            {
                get { return _underlyingProperty.PropertyType; }
            }

            #region Unused

            string SqlMapper.IMemberMap.ColumnName
            {
                get { return _columnName; }
            }

            FieldInfo SqlMapper.IMemberMap.Field
            {
                get { return null; }
            }

            ParameterInfo SqlMapper.IMemberMap.Parameter
            {
                get { return null; }
            }

            #endregion
        }
    }
}