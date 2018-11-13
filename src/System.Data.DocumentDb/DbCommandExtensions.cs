// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Azure.Documents;

namespace System.Data.DocumentDb
{
    public static class DbCommandExtensions
    {
        public static SqlQuerySpec ToQuerySpec(this DbCommand command)
        {
            return new SqlQuerySpec(command.CommandText, new SqlParameterCollection(YieldParameters(command)));
        }

        private static IEnumerable<SqlParameter> YieldParameters(DbCommand command)
        {
            foreach (DbParameter parameter in command.Parameters)
                yield return new SqlParameter("@" + parameter.ParameterName, parameter.Value);
        }
    }
}
