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

namespace HQ.Cadence
{
    public interface IMetered
    {
        /// <summary>
        ///     Returns the meter's rate unit
        /// </summary>
        /// <returns></returns>
        TimeUnit RateUnit { get; }

        /// <summary>
        ///     Returns the type of events the meter is measuring
        /// </summary>
        /// <returns></returns>
        string EventType { get; }

        /// <summary>
        ///     Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        long Count { get; }

        /// <summary>
        ///     Returns the fifteen-minute exponentially-weighted moving average rate at
        ///     which events have occured since the meter was created
        ///     <remarks>
        ///         This rate has the same exponential decay factor as the fifteen-minute load
        ///         average in the top Unix command.
        ///     </remarks>
        /// </summary>
        double FifteenMinuteRate { get; }

        /// <summary>
        ///     Returns the five-minute exponentially-weighted moving average rate at
        ///     which events have occured since the meter was created
        ///     <remarks>
        ///         This rate has the same exponential decay factor as the five-minute load
        ///         average in the top Unix command.
        ///     </remarks>
        /// </summary>
        double FiveMinuteRate { get; }

        /// <summary>
        ///     Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        double MeanRate { get; }

        /// <summary>
        ///     Returns the one-minute exponentially-weighted moving average rate at
        ///     which events have occured since the meter was created
        ///     <remarks>
        ///         This rate has the same exponential decay factor as the one-minute load
        ///         average in the top Unix command.
        ///     </remarks>
        /// </summary>
        /// <returns></returns>
        double OneMinuteRate { get; }
    }
}
