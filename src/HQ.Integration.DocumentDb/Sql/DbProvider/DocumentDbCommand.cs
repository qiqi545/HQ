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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;

using RequestOptions = Microsoft.Azure.Documents.Client.RequestOptions;

#pragma warning disable 649

namespace HQ.Integration.DocumentDb.Sql.DbProvider
{
	public sealed class DocumentDbCommand : DbCommand
	{
		private readonly DocumentDbOptions _options;
		private readonly DocumentDbParameterCollection _parameters;

		private DocumentDbConnection _connection;

		public DocumentDbCommand(DocumentDbOptions options)
		{
			_options = options;
			_parameters = new DocumentDbParameterCollection();
		}

		public DocumentDbCommand(DocumentDbConnection connection, DocumentDbOptions options) : this(options) => _connection = connection;

		protected override DbParameterCollection DbParameterCollection => _parameters;

		public override string CommandText { get; set; }

		protected override DbConnection DbConnection
		{
			get => _connection;
			set
			{
				if (value is DocumentDbConnection connection)
					_connection = connection;
				else
					throw new InvalidCastException($"The connection passed was not a {nameof(DocumentDbConnection)}.");
			}
		}

		protected override DbParameter CreateDbParameter()
		{
			return new DocumentDbParameter();
		}

		public override object ExecuteScalar()
		{
			if (CommandText.Contains("COUNT"))
			{
				var options = new FeedOptions {MaxItemCount = 1, EnableCrossPartitionQuery = true};
				var uri = UriFactory.CreateDocumentCollectionUri(_connection.Database, Collection);
				var query = this.ToQuerySpec();
				MaybeTypeDiscriminate(query);

				var result = _connection.Client.CreateDocumentQuery<long>(uri, query, options).AsDocumentQuery();
				var count = result.ExecuteNextAsync<long>().GetAwaiter().GetResult();
				return count.SingleOrDefault();
			}

			var resultSet = GetQueryResultSet();
			return resultSet?[0]?.ElementAt(0).Value;
		}

		public override int ExecuteNonQuery()
		{
			if (CommandText.StartsWith(Constants.Insert))
				return InsertImpl();

			if (CommandText.StartsWith(Constants.Update))
				return UpdateImpl();

			if (CommandText.StartsWith(Constants.Delete))
				return DeleteImpl();

			var result = ExecuteScalar();
			return result is int value ? value : default;
		}

		private int UpdateImpl()
		{
			var document = CommandToDocument(Constants.Update);

			var uri = UriFactory.CreateDocumentCollectionUri(_connection.Database, Collection);

			const bool disableAutomaticIdGeneration = true;
			
			var response = _connection.Client.UpsertDocumentAsync(uri, document, GetDocumentRequestOptions(document, uri), disableAutomaticIdGeneration).Result;
			return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
		}

		private RequestOptions GetDocumentRequestOptions(IDictionary<string, object> document, Uri uri)
		{
			var id = !document.ContainsKey(Constants.IdKey)
				? SetSurrogateKeyForUpdate(document, uri)
				: document[Constants.IdKey]?.ToString();

			var options = DocumentDbRepository<IDocument>.GetRequestOptions(id);
			return options;
		}

		private string SetSurrogateKeyForUpdate(IDictionary<string, object> document, Uri uri)
		{
			if (string.IsNullOrWhiteSpace(Id))
				return null;

			var query = new SqlQuerySpec($"SELECT VALUE r.id FROM {DocumentType} r WHERE r.{Id} = @Id AND r.DocumentType = @DocumentType");
			query.Parameters.Add(new SqlParameter("@Id", document[Id]));
			query.Parameters.Add(new SqlParameter($"@{nameof(DocumentType)}", DocumentType));

			var ids = new List<string>();
			var feedOptions = new FeedOptions {EnableCrossPartitionQuery = true};
			var projection = _connection.Client.CreateDocumentQuery<List<string>>(uri, query, feedOptions)
				.AsDocumentQuery();
			while (projection.HasMoreResults)
			{
				var next = projection.ExecuteNextAsync().GetAwaiter().GetResult();
				if (next.Count > 1)
				{
					foreach (var entry in next)
					{
						if (entry is JValue jv)
							ids.Add(jv.Value as string);
					}
				}
				else
				{
					if (next.SingleOrDefault() is JValue jv)
						ids.Add(jv.Value as string);
				}
			}

			var id = ids.SingleOrDefault();
			if (!string.IsNullOrWhiteSpace(id))
			{
				if(!document.TryAdd(Constants.IdKey, id))
					document[Constants.IdKey] = id;
			}

			if (!document.ContainsKey(Constants.IdKey))
				throw new ArgumentNullException();

			return id;
		}

		private int InsertImpl()
		{
			var document = CommandToDocument(Constants.Insert);

			var disableAutomaticIdGeneration = document.ContainsKey(Constants.IdKey);
			var uri = UriFactory.CreateDocumentCollectionUri(_connection.Database, Collection);
			var response = _connection.Client.CreateDocumentAsync(uri, document, GetDocumentRequestOptions(document, uri), disableAutomaticIdGeneration).Result;
			return response.StatusCode == HttpStatusCode.Created ? 1 : 0;
		}

		private int DeleteImpl()
		{
			var document = CommandToDocument(Constants.Delete);

			var feedOptions = new FeedOptions {EnableCrossPartitionQuery = true};
			
			object id;
			if(!string.IsNullOrWhiteSpace(Id))
			{
				if (!document.ContainsKey(Constants.IdKey) && !string.IsNullOrWhiteSpace(Id))
				{
					if (!document.TryGetValue(Id, out var objectId))
						return 0;

					var collectionUri = UriFactory.CreateDocumentCollectionUri(_connection.Database, Collection);

					var sql = $"SELECT c.id FROM c WHERE c.{Id} = @{Id}";
					var parameters = new SqlParameterCollection(new[] {new SqlParameter($"@{Id}", objectId)});
					var query = new SqlQuerySpec(sql, parameters);

					if (MaybeTypeDiscriminate(query))
						query.QueryText += " AND c.DocumentType = @DocumentType";
					
					var getId = _connection.Client.CreateDocumentQuery(collectionUri, query, feedOptions).ToList()
						.SingleOrDefault();

					if (getId == null)
						return 0;

					id = getId.id;
				}
				else
				{
					id = document[Constants.IdKey];
				}
			}
			else
			{
				throw new InvalidOperationException("CosmosDB cannot handle a 'DELETE WHERE' convention; ids must be known.");
			}

			//
			// Delete By Id:
			var uri = UriFactory.CreateDocumentUri(_connection.Database, Collection, $"{id}");
			var deleted = _connection.Client.DeleteDocumentAsync(uri,  GetDocumentRequestOptions(document, uri)).Result;
			return deleted.StatusCode == HttpStatusCode.NoContent ? 1 : 0;
		}

		private Dictionary<string, object> CommandToDocument(string preamble)
		{
			var document = StartDocumentDefinition();

			switch (preamble)
			{
				case Constants.Insert:
				{
					var commandBase = CommandText.Substring(Constants.Insert.Length);
					var collectionName = commandBase.Truncate(commandBase.IndexOf(" ", StringComparison.Ordinal));
					var qualifier = collectionName + ".";

					foreach (DocumentDbParameter parameter in _parameters)
					{
						var parameterName = parameter.ParameterName.Substring(qualifier.Length);
						document.Add(parameterName, parameter.Value);

						var parameterType = parameter.Value.GetType();
						var isValidIdType = parameterType == typeof(string) || parameterType == typeof(Guid);
						if (isValidIdType && parameterName == Id)
							document.Add(Constants.IdKey, parameter.Value);

						var isSequenceIdType = parameterType == typeof(long) || parameterType == typeof(int) ||
						                       parameterType == typeof(short);
						if (parameterName == Id && isSequenceIdType)
						{
							_connection.Client.GetNextValueForSequenceAsync(Type.Name, _connection.Database, Collection)
								.GetAwaiter().GetResult();
						}
					}

					break;
				}
				default:
				{
					foreach (DocumentDbParameter parameter in _parameters)
					{
						document.Add(parameter.ParameterName, parameter.Value);

						var parameterName = parameter.ParameterName;

						var parameterType = parameter.Value.GetType();
						var isValidIdType = parameterType == typeof(string) || parameterType == typeof(Guid);
						if (isValidIdType && parameterName == Id)
							document[Constants.IdKey] = parameter.Value;
					}

					break;
				}
			}

			return document;
		}

		private Dictionary<string, object> StartDocumentDefinition()
		{
			var document = new Dictionary<string, object>();

			if (UseTypeDiscrimination)
				document.Add(nameof(DocumentType), DocumentType);

			return document;
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behaviour)
		{
			Debug.Assert(Type != null, nameof(Type) + " != null");
			IResultSet<ExpandoObject> resultSet = GetQueryResultSet();
			return new DocumentDbDataReader<ExpandoObject>(resultSet, Type);
		}

		private QueryResultSet GetQueryResultSet()
		{
			var options = new FeedOptions {EnableCrossPartitionQuery = true};

			var uri = UriFactory.CreateDocumentCollectionUri(_connection.Database, Collection);

			var query = this.ToQuerySpec();

			return query.Parameters.Any(x => x.Name == "@Page")
				? FillResultSetPage(options, uri)
				: FillResultSet(query, uri, options);
		}

		private QueryResultSet FillResultSetPage(FeedOptions options, Uri uri)
		{
			var selectClause = CommandText.Substring(CommandText.IndexOf(":::", StringComparison.Ordinal) + 3);

			CommandText = CommandText.Replace(selectClause, "r.id").Replace(":::r.id", string.Empty)
				.Replace("SELECT", "SELECT VALUE ");

			if (UseTypeDiscrimination)
			{
				var clause = CommandText.Contains("WHERE") ? "AND" : "WHERE";
				CommandText += $" {clause} r.DocumentType = @DocumentType";
			}

			var query = this.ToQuerySpec();
			MaybeTypeDiscriminate(query);

			var page = (int) query.Parameters.Single(x => x.Name == "@Page").Value;
			var perPage = (int) query.Parameters.Single(x => x.Name == "@PerPage").Value;
			options.MaxItemCount = page * perPage;

			var ids = new List<string>();
			var projection = _connection.Client
				.CreateDocumentQuery<List<string>>(uri, query, options)
				.AsDocumentQuery();

			while (projection.HasMoreResults)
			{
				var next = projection.ExecuteNextAsync().GetAwaiter().GetResult();
				if (next.Count > 1)
				{
					foreach (var id in next)
					{
						if (id is JValue jv)
							ids.Add(jv.Value as string);
					}
				}
				else
				{
					if (next.SingleOrDefault() is JValue jv)
						ids.Add(jv.Value as string);
				}
			}

			{
				var pageIds = ids.Skip(perPage * (page - 1)).Take(perPage);
				var clause = CommandText.Contains("WHERE") ? "AND" : "WHERE";

				CommandText = CommandText.Replace("r.id", selectClause).Replace("SELECT VALUE", "SELECT");
				CommandText += $" {clause} r.id IN ('{string.Join("', '", pageIds)}')";

				query = this.ToQuerySpec();

				return FillResultSet(query, uri, options);
			}
		}

		private QueryResultSet FillResultSet(SqlQuerySpec query, Uri uri, FeedOptions options)
		{
			MaybeTypeDiscriminate(query);
			var result = _connection.Client.CreateDocumentQuery<ExpandoObject>(uri, query, options);
			var resultSet = new QueryResultSet();
			resultSet.AddRange(result);
			return resultSet;
		}

		public bool MaybeTypeDiscriminate(SqlQuerySpec query)
		{
			if (UseTypeDiscrimination)
				query.Parameters.Add(new SqlParameter($"@{nameof(DocumentType)}", DocumentType));
			return UseTypeDiscrimination;
		}

		#region Custom Properties

		public Type Type { get; set; }
		public string DocumentType { get; set; }
		public string Id { get; set; }
		public string Collection { get; set; }

		private bool UseTypeDiscrimination => Type != null && Collection != DocumentType;

		#endregion

		#region Deactivated

		public override CommandType CommandType
		{
			get => CommandType.Text;
			set { }
		}

		public override bool DesignTimeVisible
		{
			get => false;
			set { }
		}

		protected override DbTransaction DbTransaction
		{
			get => null;
			set { }
		}

		public override int CommandTimeout
		{
			get => 0;
			set { }
		}

		public override UpdateRowSource UpdatedRowSource
		{
			get => UpdateRowSource.None;
			set { }
		}

		public override void Prepare()
		{
		}

		public override void Cancel()
		{
		}

		#endregion
	}
}