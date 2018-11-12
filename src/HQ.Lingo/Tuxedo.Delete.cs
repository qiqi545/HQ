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

namespace HQ.Lingo
{
    partial class Tuxedo
    {
        public static Query DeleteAll<T>()
        {
            return new Query(DeleteFrom(GetDescriptor<T>()));
        }

        public static Query Delete<T>(dynamic where)
        {
            var descriptor = GetDescriptor<T>();
            var deleteFrom = DeleteFrom(descriptor);
            Query whereClause = WhereClauseByExample(descriptor, where);
            var sql = string.Concat(deleteFrom, " WHERE ", whereClause);
            return new Query(sql, whereClause.Parameters);
        }

        private static string DeleteFrom(Descriptor.TableDescriptor.Descriptor descriptor)
        {
            return string.Concat("DELETE FROM ", TableName(descriptor));
        }
    }
}
