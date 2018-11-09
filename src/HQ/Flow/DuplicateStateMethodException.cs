// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow
{
    [Serializable]
    public class DuplicateStateMethodException : Exception
    {
        public DuplicateStateMethodException(params string[] stateMethods) : base(
            "Duplicate state methods were found: \n" + string.Join("\n", stateMethods))
        {
        }
    }
}
