// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using FastMember;
using LiteGuard;

namespace System.Data.DocumentDb
{
    public sealed class DocumentDbDataReader<TRow> : DbDataReader where TRow : IDictionary<string, object>
    {
        private readonly Dictionary<string, Member> _members;
        private readonly IResultSet<TRow> _resultSet;
        private bool _closed;
        private int _index = -1;

        public DocumentDbDataReader(IResultSet<TRow> resultSet, Type type)
        {
            _resultSet = resultSet;
            _members = TypeAccessor.Create(type).GetMembers().ToDictionary(k => k.Name, v => v);
        }

        private TRow CurrentRow
        {
            get
            {
                if (!_closed && _index == -1 && HasRows)
                    return _resultSet[0];

                if (_index == -1)
                    return default;

                return _resultSet[_index];
            }
        }

        public override void Close()
        {
            _closed = true;
        }

        public override bool Read()
        {
            return NextResult();
        }

        public override bool NextResult()
        {
            _index++;
            return _index < _resultSet.Count;
        }

        public override IEnumerator GetEnumerator()
        {
            return _resultSet.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            var name = GetName(ordinal);

            _members.TryGetValue(name, out var member);

            // If the consumer isn't using "id" explicitly, try to redirect to "Id" instead.
            if (member == null && name == Constants.IdKey)
                _members.TryGetValue(Constants.IdProperty, out member);

            // Deal with document cruft
            if (member == null && Defaults.Metadata.TryGetValue(name, out var type))
                return type;

            Debug.Assert(member != null, nameof(member) + " != null");

            // The Azure SDK is likely using JSON.NET to deserialize the result set,
            // which means that the untyped model will return Int64 for all integer values;
            // therefore, we need to harmonize w/ the accessor model to prevent Dapper
            // from throwing an exception when it encounters the wrong type
            if (member.Type == typeof(short) || member.Type == typeof(int))
                return typeof(long);

            return member.Type;
        }

        public override string GetDataTypeName(int ordinal)
        {
            return GetFieldType(ordinal)?.Name;
        }

        public override bool IsDBNull(int ordinal)
        {
            return GetValue(ordinal) == null;
        }

        public override int GetOrdinal(string name)
        {
            Guard.AgainstNullArgument(nameof(name), name);

            if (_members.TryGetValue(name, out var member))
            {
                var count = CurrentRow.Count();
                for (var i = 0; i < count; i++)
                {
                    var row = CurrentRow.ElementAt(i);
                    if (row.Key == member.Name)
                        return i;
                }
            }

            throw new IndexOutOfRangeException("The name specified is not a valid column name.");
        }

        public override object GetValue(int ordinal)
        {
            return this[ordinal];
        }

        public override int GetValues(object[] values)
        {
            Guard.AgainstNullArgument(nameof(values), values);

            for (var i = 0; i < values.Length; i++)
            {
                if (CurrentRow.Count() <= i)
                    continue;
                values[i] = CurrentRow.ElementAtOrDefault(i);
            }

            return 1;
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            Guard.AgainstNullArgument(nameof(buffer), buffer);
            if (!_resultSet.SupportsBinary)
                throw new NotSupportedException();
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            Guard.AgainstNullArgument(nameof(buffer), buffer);
            if (!_resultSet.SupportsBinary)
                throw new NotSupportedException();
            throw new NotImplementedException();
        }

        #region Passthrough

        public override object this[string name] => _index == -1 ? null : CurrentRow.Single(s => s.Key == name).Value;
        public override object this[int i] => _index == -1 ? null : CurrentRow.ElementAt(i).Value;

        public override bool IsClosed => _closed;
        public override int FieldCount => HasRows ? _resultSet[0].Count() : 0;
        public override int Depth => _resultSet.Count == 0 ? 0 : 1;
        public override int RecordsAffected => -1;
        public override bool HasRows => _resultSet.Count > 0;

        public override string GetName(int ordinal)
        {
            return CurrentRow.ElementAt(ordinal).Key;
        }

        public override byte GetByte(int ordinal)
        {
            return (byte) GetValue(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            return (char) GetValue(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return (short) GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int) GetValue(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return (long) GetValue(ordinal);
        }

        public override string GetString(int ordinal)
        {
            return (string) GetValue(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid) GetValue(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime) GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal) GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (double) GetValue(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return (float) GetValue(ordinal);
        }

        public override bool GetBoolean(int ordinal)
        {
            return (bool) GetValue(ordinal);
        }

        #endregion
    }
}
