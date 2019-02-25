// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Microsoft.CodeAnalysis.PooledObjects
{
    /// <summary>
    ///     The usage is:
    ///     var inst = PooledStringBuilder.GetInstance();
    ///     var sb = inst.builder;
    ///     ... Do Stuff...
    ///     ... sb.ToString() ...
    ///     inst.Free();
    /// </summary>
    internal class PooledStringBuilder
    {
        // global pool
        private static readonly ObjectPool<PooledStringBuilder> s_poolInstance = CreatePool();
        private readonly ObjectPool<PooledStringBuilder> _pool;
        public readonly StringBuilder Builder = new StringBuilder();

        private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool)
        {
            Debug.Assert(pool != null);
            _pool = pool;
        }

        public int Length => Builder.Length;

        public void Free()
        {
            var builder = Builder;

            // do not store builders that are too large.
            if (builder.Capacity <= 1024)
            {
                builder.Clear();
                _pool.Free(this);
            }
            else
            {
                _pool.ForgetTrackedObject(this);
            }
        }

        [Obsolete("Consider calling ToStringAndFree instead.")]
        public new string ToString()
        {
            return Builder.ToString();
        }

        public string ToStringAndFree()
        {
            var result = Builder.ToString();
            Free();

            return result;
        }

        public string ToStringAndFree(int startIndex, int length)
        {
            var result = Builder.ToString(startIndex, length);
            Free();

            return result;
        }

        // if someone needs to create a private pool;
        /// <summary>
        ///     If someone need to create a private pool
        /// </summary>
        /// <param name="size">The size of the pool.</param>
        /// <returns></returns>
        public static ObjectPool<PooledStringBuilder> CreatePool(int size = 32)
        {
            ObjectPool<PooledStringBuilder> pool = null;
            pool = new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool), size);
            return pool;
        }

        public static PooledStringBuilder GetInstance()
        {
            var builder = s_poolInstance.Allocate();
            Debug.Assert(builder.Builder.Length == 0);
            return builder;
        }

        public static implicit operator StringBuilder(PooledStringBuilder obj)
        {
            return obj.Builder;
        }
    }
}
