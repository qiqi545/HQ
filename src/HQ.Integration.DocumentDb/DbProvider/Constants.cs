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

namespace HQ.Integration.DocumentDb.DbProvider
{
	internal class Constants
	{
		public const string IdKey = "id";
		public const string IdProperty = "Id";

		public const string Insert = "INSERT INTO ";
		public const string Update = "UPDATE ";
		public const string Delete = "DELETE FROM ";

		public const string AccountEndpointKey = "AccountEndpoint";
		public const string AccountKeyKey = "AccountKey";
		public const string DatabaseKey = "Database";
		public const string DefaultCollectionKey = "DefaultCollection";
		public const string SharedCollectionKey = "SharedCollection";

		public const string SequenceDocumentType = "Sequence";
		public const string SequenceTypeField = "SequenceType";
		public const string DocumentTypeField = "DocumentType";
	}
}