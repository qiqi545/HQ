// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace System.Data.DocumentDb
{
    /// <inheritdoc />
    /// <summary>
    ///     <remarks>
    ///         This exists because
    ///         <see cref="T:Microsoft.Azure.Documents.SqlParameterCollection">
    ///             does not inherit from
    ///             <see cref="T:System.Data.Common.DbParameterCollection" />.
    ///         </see>
    ///     </remarks>
    /// </summary>
    public sealed class DocumentDbParameterCollection : DbParameterCollection
    {
        private readonly List<DocumentDbParameter> _parameters;

        public DocumentDbParameterCollection()
        {
            SyncRoot = new object();
            _parameters = new List<DocumentDbParameter>();
        }

        public override int Count => _parameters.Count;

        public override object SyncRoot { get; }

        public override int Add(object value)
        {
            if (value is DocumentDbParameter parameter)
            {
                if (_parameters.Any(p => p.ParameterName == parameter.ParameterName))
                    throw new ArgumentException("The " + nameof(DocumentDbParameter) + " specified in the + " +
                                                nameof(value) + " parameter is already added to this or another " +
                                                nameof(DocumentDbParameterCollection) + ".");

                _parameters.Add(parameter);
                return _parameters.Count - 1;
            }

            throw new InvalidCastException("The parameter passed was not a " + nameof(DocumentDbParameter) + ".");
        }

        public override void AddRange(Array values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            foreach (var value in values)
                if (value is DocumentDbParameter parameter)
                    _parameters.Add(parameter);
                else
                    throw new InvalidCastException(
                        "The parameter passed was not a " + nameof(DocumentDbParameter) + ".");
        }

        public void AddRange(DocumentDbParameter[] values)
        {
            foreach (var value in values)
                if (value != null)
                    _parameters.Add(value);
                else
                    throw new ArgumentNullException(nameof(value));
        }

        public override void Clear()
        {
            _parameters.Clear();
        }

        public override bool Contains(string parameterName)
        {
            return _parameters.Any(p => p.ParameterName == parameterName);
        }

        public override bool Contains(object value)
        {
            return _parameters.Any(p => p.Value == value);
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return _parameters.Single(p => p.ParameterName == parameterName);
        }

        public override int IndexOf(string parameterName)
        {
            return _parameters.IndexOf(_parameters.Single(p => p.ParameterName == parameterName));
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf((DocumentDbParameter) value);
        }

        public override void Insert(int index, object value)
        {
            _parameters.Insert(index, (DocumentDbParameter) value);
        }

        public override void Remove(object value)
        {
            _parameters.Remove((DocumentDbParameter) value);
        }

        public override void RemoveAt(string parameterName)
        {
            _parameters.Remove(_parameters.Single(p => p.ParameterName == parameterName));
        }

        public override void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        protected override DbParameter GetParameter(int index)
        {
            return _parameters[index];
        }

        public override void CopyTo(Array array, int index)
        {
            Debug.Assert(array != null, nameof(array) + " != null");

            foreach (var item in array.Cast<DocumentDbParameter>())
                _parameters.Add(item);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            if (value is DocumentDbParameter parameter)
                _parameters[index] = parameter;
            else
                throw new InvalidCastException("The parameter passed was not a " + nameof(DocumentDbParameter) + ".");
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index > -1)
            {
                if (value is DocumentDbParameter parameter)
                    _parameters[index] = parameter;
                else
                    throw new InvalidCastException(
                        "The parameter passed was not a " + nameof(DocumentDbParameter) + ".");
            }
        }
    }
}
