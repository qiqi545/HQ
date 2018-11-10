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

namespace HQ.Touchstone
{
    public static class Method<TReturn>
    {
        public static Func<TReturn> Call(Func<TReturn> func)
        {
            return func;
        }

        public static Func<T1, TReturn> Call<T1>(Func<T1, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, TReturn> Call<T1, T2>(Func<T1, T2, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, TReturn> Call<T1, T2, T3>(Func<T1, T2, T3, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, TReturn> Call<T1, T2, T3, T4>(Func<T1, T2, T3, T4, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, TReturn> Call<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, TReturn> Call<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, TReturn> Call<T1, T2, T3, T4, T5, T6, T7>(
            Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> Call<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9,
            T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> Call<T1, T2, T3, T4, T5, T6, T7, T8,
            T9, T10, T11>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> Call<T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> Call<T1, T2, T3, T4, T5, T6,
            T7, T8, T9, T10, T11, T12, T13>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> Call<T1, T2, T3, T4,
            T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> Call<T1, T2, T3,
            T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> Call<T1, T2,
            T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> func)
        {
            return func;
        }
    }
}
