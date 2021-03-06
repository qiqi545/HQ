﻿#region LICENSE

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

using HQ.Data.Contracts.Schema.Models;
using Newtonsoft.Json;

namespace HQ.Integration.DocumentDb.Schema.Documents
{
	public class SchemaVersionDocument : DocumentBase<SchemaVersionDocument>
	{
		public SchemaVersionDocument()
		{
			/* required for serialization */
		}

		public SchemaVersionDocument(SchemaVersion model)
		{
			Fingerprint = model.Fingerprint;
			ApplicationId = model.ApplicationId;
			Type = model.Type;
			Namespace = model.Namespace;
			Name = model.Name;
			Data = model.Data;
			Revision = model.Revision;
		}

		public ulong Fingerprint { get; set; }
		public string ApplicationId { get; set; }
		public SchemaType Type { get; set; }
		public string Namespace { get; set; }
		public string Name { get; set; }
		public Data.Contracts.Schema.Models.Schema Data { get; set; }
		public int Revision { get; set; }

		[JsonIgnore]
		public SchemaVersion Model => new SchemaVersion
		{
			Fingerprint = Fingerprint,
			ApplicationId = ApplicationId,
			Data = Data,
			Name = Name,
			Namespace = Namespace,
			Revision = Revision,
			Type = Type
		};
	}
}