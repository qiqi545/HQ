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
using System.Linq;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Schema.Extensions;
using HQ.Data.Contracts.Schema.Models;
using TypeKitchen.Differencing;
using S = HQ.Data.Contracts.Schema.Models.Schema;

namespace HQ.Data.Contracts.Schema.Services
{
	public class SchemaService
	{
		private readonly IApplicationVersionStore _applications;
		private readonly ISchemaVersionStore _schemas;

		public SchemaService(IApplicationVersionStore applications, ISchemaVersionStore schemas)
		{
			_applications = applications;
			_schemas = schemas;
		}

		public async Task<ulong> TrySaveRevisionAsync(string applicationId, IReadOnlyList<S> revisionSet)
		{
			var revisions = 0;
			foreach (var schema in revisionSet.OrderByTopology(GetSchemaDependents))
			{
				var hash = ValueHash.ComputeHash(schema);
				var notModified = await _schemas.GetByFingerprintAsync(hash);
				if (notModified != null)
					continue;

				var lastRevision = await _schemas.GetLastRevisionAsync(schema.Type, schema.Namespace, schema.Name);
				var revision = lastRevision?.Revision + 1 ?? 1;

				var schemaVersion = new SchemaVersion
				{
					Fingerprint = hash,
					ApplicationId = applicationId,
					Type = schema.Type,
					Namespace = schema.Namespace,
					Name = schema.Name,
					Revision = revision,
					Data = schema
				};

				await _schemas.CreateAsync(schemaVersion);
				revisions++;
			}

			var version = await DeriveApplicationVersion(applicationId);
			if (revisions > 0)
				await _applications.CreateAsync(version);

			return revisions == 0 ? 0 : version.Fingerprint;
		}

		public async Task<ApplicationVersion> DeriveApplicationVersion(string applicationId)
		{
			var current = await _schemas.GetByApplicationId(applicationId);

			// make a dictionary to order the hash lookup
			var manifest = current.OrderBy(x => x.Name).ToDictionary(k => k.Name, v => v.Fingerprint);

			// create a new application version that can be associated with an API version
			var hashes = current.Select(x => x.Fingerprint).ToList();
			var version = new ApplicationVersion
			{
				ApplicationId = applicationId,
				Fingerprint = ValueHash.ComputeHash(new {Fingerprints = hashes}),
				Manifest = manifest
			};
			return version;
		}

		private static IEnumerable<S> GetSchemaDependents(S schema)
		{
			var edges = new List<Tuple<S, PropertyRelationship, S>>();

			foreach (var property in schema.Properties)
			{
				if (!property.IsModel() && property.Type != PropertyType.Enum)
					continue;

				// we might have specified a relationship to an older schema that's not in the revision set
				// if we have a relationship schema in the revision set with this name, we assume it's a request to map to the version in the set
				var dependsOn = schema.Scope
					.FirstOrDefault(x => x.Key.Equals(property.From, StringComparison.OrdinalIgnoreCase)).Value;
				if (dependsOn == null)
					continue;

				// if we have the inverse of this already, don't add it as an edge
				var isInverse = false;
				foreach (var edge in edges)
				{
					if (edge.Item1 == dependsOn)
					{
						isInverse = edge.Item2 == PropertyRelationship.OneToMany &&
						            property.Rel == PropertyRelationship.OneToOne ||
						            edge.Item2 == PropertyRelationship.OneToOne &&
						            property.Rel == PropertyRelationship.OneToMany;

						break;
					}
				}

				if (isInverse)
					continue;

				edges.Add(new Tuple<S, PropertyRelationship, S>(schema, property.Rel, dependsOn));
			}

			return edges.Select(x => x.Item3);
		}
	}
}