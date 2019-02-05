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
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;

namespace HQ.Data.Sql.Queries
{
    // todo kill all static access and use DI

    public static partial class SqlBuilder
    {
        static SqlBuilder()
        {
            Dialect = NoDialect.Default;
        }

        public static ISqlDialect Dialect { get; set; }
        public static Func<Type, IDataDescriptor> DescriptorFunction { get; set; } = SimpleDataDescriptor.Create;

        public static IDataDescriptor GetDescriptor<T>()
        {
            return DescriptorFunction(typeof(T));
        }

        internal static IDataDescriptor GetDescriptor(Type type)
        {
            return DescriptorFunction(type);
        }

        public static object Asc(this object clause)
        {
            return clause;
        }

        public static object Desc(this object clause)
        {
            return clause;
        }
    }
}
