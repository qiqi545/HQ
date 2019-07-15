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

using System.Data;
using System.Data.Common;

namespace HQ.Integration.DocumentDb.DbProvider
{
	/// <inheritdoc />
	/// <summary>
	///     <remarks>
	///         This exists because
	///         <see cref="T:Microsoft.Azure.Documents.SqlParameter">
	///             does not inherit from
	///             <see cref="T:System.Data.Common.DbParameter" />.
	///         </see>
	///     </remarks>
	/// </summary>
	public sealed class DocumentDbParameter : DbParameter
	{
		private string _parameterName;

		public override bool IsNullable { get; set; }
		public override object Value { get; set; }
		public override DbType DbType { get; set; }
		public override int Size { get; set; }

		public override ParameterDirection Direction
		{
			get => ParameterDirection.Input;
			set { }
		}

		public override string SourceColumn
		{
			get => string.Empty;
			set { }
		}

		public override bool SourceColumnNullMapping
		{
			get => false;
			set { }
		}

		public override string ParameterName
		{
			get => _parameterName;
			set
			{
				if (value == null)
					return;
				_parameterName = _parameterName ?? value;
			}
		}

		public override void ResetDbType()
		{
		}
	}
}