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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using FastMember;
using HQ.Common;
using HQ.Data.Sql.Descriptor.Attributes;

namespace HQ.Data.Sql.Descriptor
{
    public class SimpleDataDescriptor : IDataDescriptor
    {
        private static readonly Hashtable Descriptors = new Hashtable();

        protected SimpleDataDescriptor(Type type)
        {
            Types = new[] {type};

            ResolveTableInfo(this, type);

            All = new List<PropertyToColumn>();
            Keys = new List<PropertyToColumn>();
            Inserted = new List<PropertyToColumn>();
            Updated = new List<PropertyToColumn>();
            Computed = new List<PropertyToColumn>();

            var accessor = TypeAccessor.Create(type);
            var descriptors = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();
            var accessors = descriptors
                .Select(property => new PropertyAccessor(accessor, property.PropertyType, property.Name))
                .ToList();

            foreach (var property in accessors)
            {
                if (Exists(property) || property.HasAttribute<IgnoreDataMemberAttribute>())
                    continue;

                var column = new PropertyToColumn(property);

                All.Add(column);

                if (property.HasAttribute<KeyAttribute>())
                {
                    column.IsKey = true;
                    Keys.Add(column);
                }

                if (property.HasAttribute<OneToManyAttribute>())
                {
                    column.IsComputed = true;
                    Computed.Add(column);
                }

                if (property.GetAttribute<DatabaseGeneratedAttribute>() is DatabaseGeneratedAttribute generated)
                {
                    switch (generated.DatabaseGeneratedOption)
                    {
                        case DatabaseGeneratedOption.Computed:
                        {
                            column.IsComputed = true;
                            Computed.Add(column);
                            break;
                        }
                        case DatabaseGeneratedOption.Identity:
                        {
                            column.IsComputed = true;
                            column.IsIdentity = true;
                            Computed.Add(column);
                            break;
                        }
                        case DatabaseGeneratedOption.None:
                        {
                            Inserted.Add(column);
                            Updated.Add(column);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    Inserted.Add(column);
                    Updated.Add(column);
                }
            }

            if (Keys.Count == 0)
            {
                // try to add an exact match key
                var defaultKey = All.FirstOrDefault(x => x.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase));
                if (defaultKey != null)
                    Keys.Add(defaultKey);
            }
        }

        public static Func<string, string> TableNameConvention { get; set; }

        public IList<Type> Types { get; }
        public Type Type => MaybeGetSingleType();

        public string Schema { get; set; }
        public string Table { get; set; }

        public IList<PropertyToColumn> All { get; }
        public IList<PropertyToColumn> Keys { get; }
        public IList<PropertyToColumn> Inserted { get; }
        public IList<PropertyToColumn> Updated { get; }
        public IList<PropertyToColumn> Computed { get; }

        public PropertyToColumn Id => MaybeGetSingleId();

        public PropertyToColumn Timestamp => MaybeGetTimestamp();

        private PropertyToColumn MaybeGetTimestamp()
        {
            return Inserted.SingleOrDefault(x => x.IsTimestamp);
        }

        public static SimpleDataDescriptor Create<T>()
        {
            return Create(typeof(T));
        }

        public static SimpleDataDescriptor Create(Type type)
        {
            lock (Descriptors)
            {
                var obj = (SimpleDataDescriptor) Descriptors[type];
                if (obj != null) return obj;

                obj = (SimpleDataDescriptor) Descriptors[type];
                if (obj != null) return obj;

                obj = new SimpleDataDescriptor(type);
                Descriptors[type] = obj;
                return obj;
            }
        }

        private Type MaybeGetSingleType()
        {
            if (Types == null || Types.Count != 1)
                return null;
            return Types[0];
        }

        private PropertyToColumn MaybeGetSingleId()
        {
            if (Keys == null || Keys.Count != 1)
                return null;
            return Keys[0];
        }

        private static void ResolveTableInfo(SimpleDataDescriptor descriptor, Type type)
        {
            var tableAttributes = type.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0 && tableAttributes[0] is TableAttribute attribute)
            {
                descriptor.Table = TableNameConvention?.Invoke(attribute.Name);
                descriptor.Schema = attribute.Schema;
            }
            else
            {
                descriptor.Table = TableNameConvention?.Invoke(type.GetNonGenericName()) ?? type.GetNonGenericName();
            }
        }

        private bool Exists(PropertyAccessor accessor)
        {
            return All.Any(p => p.Property.Name.Equals(accessor.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
