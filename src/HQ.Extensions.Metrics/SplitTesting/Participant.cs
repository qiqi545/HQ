using System;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public class Participant
    {
        /// <summary>
        /// The unique identity of this participant
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// The alternative shown to this participant, by index
        /// </summary>
        public int? Shown { get; set; }

        /// <summary>
        /// Whether the participant converted on a shown identity, and if so, when it occurred.
        /// </summary>
        public DateTimeOffset? Converted { get; set; }
    }
}
