// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Blowdart.UI.Web.Internal
{
    internal static class StringBuilderHelper
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static StringBuilder Get()
        {
            return StringBuilderPool.Get();
        }

        public static void Return(StringBuilder sb)
        {
            StringBuilderPool.Return(sb);
        }

        public static string BuildString(Action<StringBuilder> action)
        {
            var sb = StringBuilderPool.Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }
    }
}