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

namespace HQ.Extensions.Metrics
{
    public enum SampleType
    {
        /// <summary>
        ///     Uses a uniform sample of 1028 elements, which offers a 99.9%
        ///     confidence level with a 5% margin of error assuming a normal
        ///     distribution.
        /// </summary>
        Uniform,

        /// <summary>
        ///     Uses an exponentially decaying sample of 1028 elements, which offers
        ///     a 99.9% confidence level with a 5% margin of error assuming a normal
        ///     distribution, and an alpha factor of 0.015, which heavily biases
        ///     the sample to the past 5 minutes of measurements.
        /// </summary>
        Biased
    }
}
