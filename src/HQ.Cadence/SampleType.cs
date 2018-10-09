// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Cadence
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