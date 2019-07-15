// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;

namespace System.Data.DocumentDb
{
    public sealed class QueryResultSet : List<ExpandoObject>, IResultSet<ExpandoObject>
    {
        public bool SupportsBinary => false;
    }
}
