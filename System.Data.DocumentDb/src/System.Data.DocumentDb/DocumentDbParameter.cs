// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace System.Data.DocumentDb
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
