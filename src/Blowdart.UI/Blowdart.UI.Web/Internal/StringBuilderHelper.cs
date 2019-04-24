// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Blowdart.UI.Internal;

namespace Blowdart.UI.Web.Internal
{
    internal static class StringBuilderHelper
    {
        public static StringBuilder Get()
        {
            return Pools.StringBuilderPool.Get();
        }

        public static void Return(StringBuilder sb)
        {
            Pools.StringBuilderPool.Return(sb);
        }

        public static string BuildString(Action<StringBuilder> action)
        {
            var sb = Pools.StringBuilderPool.Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                Pools.StringBuilderPool.Return(sb);
            }
        }
    }
}