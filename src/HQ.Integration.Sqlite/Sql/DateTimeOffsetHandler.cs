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
using System.Data;
using Dapper;

namespace HQ.Integration.Sqlite.Sql
{
	public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
	{
		public static readonly DateTimeOffsetHandler Default = new DateTimeOffsetHandler();
		protected DateTimeOffsetHandler() { }

		public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
		{
			if (value.HasValue)
			{
				parameter.Value = value.Value;
			}
			else
			{
				parameter.Value = DBNull.Value;
			}
		}

		public override DateTimeOffset? Parse(object value)
		{
			switch (value)
			{
				case null:
					return null;
				case DateTimeOffset offset:
					return offset;
				default:
					return Convert.ToDateTime(value);
			}
		}
	}
}