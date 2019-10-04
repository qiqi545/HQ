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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Extensions.Metrics.SplitTesting
{
	public class Experiment
	{
		private readonly object[] _alternatives;
		private readonly HashSet<string> _metrics;

		private readonly IDictionary<string, Participant> _participants;

		protected internal Experiment(ICohortIdentifier identifier, string name, string description,
			object[] alternatives = null, params string[] metrics)
		{
			Name = name;
			Description = description;

			_alternatives = alternatives ?? new object[] {true, false};
			_metrics = new HashSet<string>(metrics);

			Identify = identifier.Identify;
			Conclude = experiment => false;
			Choose = HighestDistinctConvertingAlternative();

			_participants = new ConcurrentDictionary<string, Participant>();

			Conversions = new ConcurrentDictionary<int, int>();
			for (var i = 1; i <= Alternatives; i++)
			{
				Conversions.Add(i, 0);
			}
		}

		public string Name { get; }
		public string Description { get; set; }
		public Func<string> Identify { get; set; }
		public int Alternatives => _alternatives.Length;

		public int? Outcome { get; set; }
		public DateTimeOffset? ConcludedAt { get; set; }
		public Func<Experiment, bool> Conclude { get; set; }
		public Func<Experiment, int> Choose { get; set; }
		public IEnumerable<Participant> Participants => _participants.Values;

		internal IDictionary<int, int> Conversions { get; }

		public object CurrentAlternative => _alternatives[Alternative - 1];

		public Participant CurrentParticipant => EnsureParticipant(Identify());

		internal int Alternative
		{
			get
			{
				if (Outcome.HasValue)
				{
					return Outcome.Value;
				}

				var identity = Identify();

				var participant = EnsureParticipant(identity);

				if (participant.Shown.HasValue)
				{
					return participant.Shown.Value;
				}

				var alternative = Audience.Split.Value(identity, Alternatives);
				participant.Shown = alternative;

				if (Conclude(this))
				{
					ConcludedAt = DateTime.Now;
					Outcome = Choose(this);
				}

				return alternative;
			}
		}

		internal bool HasMetric(string metric)
		{
			return _metrics.Contains(metric);
		}

		private Func<Experiment, int> HighestDistinctConvertingAlternative()
		{
			return experiment =>
			{
				if (_participants.Count == 0)
				{
					return 1;
				}

				var hash = new Dictionary<int, int>();
				foreach (var participant in _participants.Values)
				{
					if (!participant.Shown.HasValue)
					{
						continue;
					}

					if (!hash.TryGetValue(participant.Shown.Value, out var alternative))
					{
						hash.Add(alternative, 1);
					}
					else
					{
						hash[alternative]++;
					}
				}

				var winner = hash.First();
				foreach (var alternative in hash)
				{
					if (alternative.Value > winner.Value)
					{
						winner = alternative;
					}
				}

				return winner.Key;
			};
		}

		private Participant EnsureParticipant(string identity)
		{
			if (_participants.TryGetValue(identity, out var participant))
				return participant;

			participant = new Participant {Identity = identity};
			_participants.Add(identity, participant);
			return participant;
		}
	}
}