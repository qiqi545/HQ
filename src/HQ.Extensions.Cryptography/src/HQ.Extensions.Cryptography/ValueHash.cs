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
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using TypeKitchen;
using WyHash;

namespace HQ.Extensions.Cryptography
{
	/// <summary>
	///     A non-cryptographic hash for comparing objects by value.
	/// </summary>
	public static class ValueHash
	{
		static ValueHash() =>
			Seed = BitConverter.ToUInt64(
				new[] {(byte) 'H', (byte) 'Q', (byte) 'I', (byte) 'O', (byte) 'h', (byte) 'q', (byte) 'i', (byte) 'o'},
				0);

		public static ulong Seed { get; set; }

		public static ulong ComputeHash(object instance)
		{
			var accessor = ReadAccessor.Create(instance, out var members);

			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					foreach (var member in members)
					{
						if (member.HasAttribute<CompilerGeneratedAttribute>())
							continue; // backing fields

						if (member.HasAttribute<NotMappedAttribute>() ||
						    member.HasAttribute<IgnoreDataMemberAttribute>())
							continue; // explicitly non-value participating

						WriteValue(accessor[instance, member.Name], member.Type, bw);
					}
				}

				return WyHash64.ComputeHash64(ms.GetBuffer(), Seed);
			}
		}

		private static void WriteValue(object value, Type type, BinaryWriter bw)
		{
			switch (value)
			{
				case string v:
					bw.Write(v);
					break;
				case byte v:
					bw.Write(v);
					break;
				case bool v:
					bw.Write(v);
					break;
				case short v:
					bw.Write(v);
					break;
				case ushort v:
					bw.Write(v);
					break;
				case int v:
					bw.Write(v);
					break;
				case uint v:
					bw.Write(v);
					break;
				case long v:
					bw.Write(v);
					break;
				case ulong v:
					bw.Write(v);
					break;
				case float v:
					bw.Write(v);
					break;
				case double v:
					bw.Write(v);
					break;
				case decimal v:
					bw.Write(v);
					break;
				case char v:
					bw.Write(v);
					break;
				case char[] v:
					bw.Write(v);
					break;
				case byte[] v:
					bw.Write(v);
					break;
				case null:
					bw.Write(0);
					break;
				default:

					if (type.IsEnum)
					{
						var enumType = Enum.GetUnderlyingType(value.GetType());
						var enumValue = Convert.ChangeType(value, enumType);

						WriteValue(enumValue, enumType, bw);
						break;
					}

					if (typeof(IEnumerable).IsAssignableFrom(type))
					{
						foreach (var item in (IEnumerable) value)
						{
							WriteValue(item, item.GetType(), bw);
						}

						break;
					}

					bw.Write(ComputeHash(value));
					break;
			}
		}
	}
}