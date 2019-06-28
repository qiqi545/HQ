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
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace HQ.Data.Sql.DocumentDb
{
    public abstract class DocumentBase<T> : IDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("documentType")]
        public string DocumentType
        {
            get => typeof(T).Name;
            set
            {
                if (value != typeof(T).Name)
                    throw new InvalidOperationException();
            }
        }
        
        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "_ts")]
        public virtual DateTime Timestamp { get; internal set; }
    }
}
