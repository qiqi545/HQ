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

// ReSharper disable once CheckNamespace

namespace HQ.Data.Sql.Helpers
{
	/// <summary>
	///     Presents the SQL parts.
	/// </summary>
	public struct SqlParts
	{
		/// <summary>
		///     The SQL.
		/// </summary>
		public string Sql;

		/// <summary>
		///     The SQL COUNT.
		/// </summary>
		public string SqlCount;

		/// <summary>
		///     The SQL FROM
		/// </summary>
		public string SqlFrom;

		/// <summary>
		///     The SQL SELECT
		/// </summary>
		public string SqlSelectRemoved;

		/// <summary>
		///     The SQL ORDER BY
		/// </summary>
		public string SqlOrderBy;

		/// <summary>
		///     The SQL ORDER BY Fields
		/// </summary>
		public string[] SqlOrderByFields;
	}
}