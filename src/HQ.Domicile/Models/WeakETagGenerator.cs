// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Security.Cryptography;

namespace HQ.Domicile.Models
{
	public class WeakETagGenerator : IETagGenerator
	{
		public string GenerateFromBuffer(byte[] data)
		{
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(data);
				var hex = BitConverter.ToString(hash);
				return $"W/\"{hex.Replace("-", "")}\"";
			}
		}
	}
}