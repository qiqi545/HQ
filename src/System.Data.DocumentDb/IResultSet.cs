// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Data.DocumentDb
{
    public interface IResultSet<T> : IList<T>
    {
        bool SupportsBinary { get; }
    }
}
