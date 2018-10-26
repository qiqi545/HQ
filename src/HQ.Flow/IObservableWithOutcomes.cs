// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Flow
{
	public interface IObservableWithOutcomes<out T> : IObservable<T>
	{
		bool Handled { get; }
		ICollection<ObservableOutcome> Outcomes { get; }
	}
}