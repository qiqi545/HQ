// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Producers
{
	/// <summary>
	///     An observer that handles the next item in a sequence using a delegate
	///     <see href="http://blogs.msdn.com/b/pfxteam/archive/2010/04/06/9990420.aspx" />
	///     <seealso href="http://code.msdn.microsoft.com/ParExtSamples" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ActionObserver<T> : IObserver<T>
	{
		private readonly Action _onCompleted;
		private readonly Action<Exception> _onError;
		private readonly Action<T> _onNext;

		public ActionObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			if (onNext == null) throw new ArgumentNullException(nameof(onNext));
			if (onError == null) throw new ArgumentNullException(nameof(onError));
			if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
			_onNext = onNext;
			_onError = onError;
			_onCompleted = onCompleted;
		}

		public void OnCompleted()
		{
			_onCompleted();
		}

		public void OnError(Exception error)
		{
			_onError(error);
		}

		public void OnNext(T value)
		{
			_onNext(value);
		}
	}
}