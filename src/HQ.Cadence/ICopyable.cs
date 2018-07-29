// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HQ.Cadence
{
    /// <summary>
    /// A marker for types that can copy themselves to another type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICopyable<out T>
    {
        /// <summary>
        /// Obtains a copy of the current type that is used in <see cref="ReadOnlyDictionary{TKey,TValue}" /> to provide immutability
        /// </summary>
        [JsonIgnore]
        T Copy { get; }
    }
}