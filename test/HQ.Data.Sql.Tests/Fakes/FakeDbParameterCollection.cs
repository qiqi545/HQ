using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HQ.Data.Sql.Tests.Fakes
{
    public class FakeDbParameterCollection : List<FakeDbParameter>, IDataParameterCollection
    {
        public void CopyTo(Array array, int index) { }
        public bool IsSynchronized => false;
        public object SyncRoot => null;

        public int Add(object value)
        {
            base.Add(value as FakeDbParameter);
            return Count;
        }

        public bool Contains(object value)
        {
            return base.Contains(value as FakeDbParameter);
        }

        public int IndexOf(object value)
        {
            return base.IndexOf(value as FakeDbParameter);
        }

        public void Insert(int index, object value)
        {
            base.Insert(index, value as FakeDbParameter);
        }

        public void Remove(object value)
        {
            base.Remove(value as FakeDbParameter);
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public bool Contains(string parameterName)
        {
            return this.FirstOrDefault(x => x.ParameterName == parameterName) != null;
        }

        public int IndexOf(string parameterName)
        {
            return base.IndexOf(this.FirstOrDefault(x => x.ParameterName == parameterName));
        }

        public void RemoveAt(string parameterName)
        {
            var indexOf = IndexOf(parameterName);
            if (indexOf == -1)
                return;
            base.RemoveAt(indexOf);
        }

        public object this[string parameterName]
        {
            get => this.FirstOrDefault(x => x.ParameterName == parameterName);
            set => throw new NotImplementedException();
        }
    }
}
