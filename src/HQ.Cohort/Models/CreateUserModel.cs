// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Models
{
	public class CreateUserModel
	{
		[Required]
		[ProtectedPersonalData]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[PersonalData] public string UserName { get; set; }

		[ProtectedPersonalData]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		[ProtectedPersonalData]
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }

		public bool PhoneNumberConfirmed { get; set; }

		public string ConcurrencyStamp { get; set; }
	}
}