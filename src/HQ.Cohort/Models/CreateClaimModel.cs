// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HQ.Cohort.Models
{
	public class CreateClaimModel
	{
		[Required] public string Type { get; set; }

		[Required] public string Value { get; set; }

		[Required] public string ValueType { get; set; } = ClaimValueTypes.String;
	}
}