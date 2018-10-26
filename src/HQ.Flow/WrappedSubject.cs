// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace HQ.Flow
{
	public class WrappedSubject<T> : ISubject<T>, IObservableWithOutcomes<T>, IDisposable
	{
		private readonly OutcomePolicy _policy;
		private readonly ISubject<T> _subject;

		public WrappedSubject(ISubject<T> subject, OutcomePolicy policy)
		{
			_subject = subject;
			_policy = policy;

			Outcomes = new List<ObservableOutcome>();
		}

		public void Dispose()
		{
			(_subject as IDisposable)?.Dispose();
		}

		public bool Handled
		{
			get
			{
				switch (_policy)
				{
					case OutcomePolicy.Pessimistic:
						return Outcomes.All(o => o.Result);
					case OutcomePolicy.Optimistic:
						return Outcomes.Any(o => o.Result);
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public ICollection<ObservableOutcome> Outcomes { get; }

		public void OnNext(T value)
		{
			_subject.OnNext(value);
		}

		public void OnError(Exception error)
		{
			_subject.OnError(error);
		}

		public void OnCompleted()
		{
			_subject.OnCompleted();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _subject.Subscribe(observer);
		}
	}
}