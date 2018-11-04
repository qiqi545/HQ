// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.ComponentModel.DataAnnotations;

namespace HQ.Cohort.Models
{
	public class CreateRoleModel
	{
		[Required] public string Name { get; set; }

		public string ConcurrencyStamp { get; set; }
	}
}