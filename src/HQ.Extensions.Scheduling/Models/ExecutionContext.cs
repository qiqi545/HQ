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
using System.Threading;
using HQ.Data.Contracts.Runtime;

namespace HQ.Extensions.Scheduling.Models
{
    public class ExecutionContext
    {
        private readonly IKeyValueStore<string, object> _data;

        public IServiceProvider ExecutionServices { get; }
        public CancellationToken CancellationToken { get; }

        public bool Continue { get; private set; }
        public bool Successful { get; private set; }
        
        public ExecutionContext(IServiceProvider serviceProvider, IKeyValueStore<string, object> data, CancellationToken cancellationToken = default)
        {
            Continue = true;
            Successful = true;
            ExecutionServices = serviceProvider;
            CancellationToken = cancellationToken;
            _data = data;
        }

        public void Fail()
        {
            if(!Continue)
                throw new ExecutionException(this, $"{nameof(Fail)} was called on a halted execution");
            Continue = false;
            Successful = false;
        }

        public void Succeed()
        {
            if (!Continue)
                throw new ExecutionException(this, $"{nameof(Succeed)} was called on a halted execution");
            Continue = false;
            Successful = true;
        }

        public void AddData<T>(string key, T item)
        {
            _data.AddOrUpdate(key, item);
        }

        public bool TryGetData<T>(string key, out T item) where T : class
        {
            if (_data.TryGetValue(key, out var value))
            {
                item = value as T;
                return item != null;
            }
            item = default;
            return false;
        }

        public bool TryGetData(string key, out bool item)
        {
	        if (_data.TryGetValue(key, out var value))
	        {
		        if (value is bool flag)
		        {
			        item = flag;
			        return true;
		        }
	        }
	        item = default;
	        return false;
        }
	}
}
