#region LICENSE

// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.

#endregion

using System;
using System.Collections.Generic;
using System.Security.Claims;
using GraphQLParser;
using GraphQLParser.AST;
using HQ.Common;
using HQ.Data.Contracts.AspNetCore.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace HQ.Platform.Api.Runtime.GraphQl
{
	public class GraphQlQueryContextProvider : IQueryContextProvider
	{
        public GraphQlQueryContextProvider()
        {
            SupportedMediaTypes = new List<MediaTypeHeaderValue>
            {
                MediaTypeHeaderValue.Parse(Constants.MediaTypes.GraphQl)
            };
        }

		public IEnumerable<QueryContext> Parse(HttpContext source)
		{
			return BuildHandleData(new Source(source.Request.Path));
		}

        public IEnumerable<QueryContext> Parse(ClaimsPrincipal user, string source)
        {
            return BuildHandleData(new Source(source));
        }

        private static IEnumerable<QueryContext> BuildHandleData(ISource source)
		{
			var parser = new Parser(new Lexer());
			var document = parser.Parse(source);
			var queries = new List<QueryContext>();

			foreach (var definition in document.Definitions)
			{
				if (definition is GraphQLOperationDefinition operation)
					switch (operation.Operation)
					{
						case OperationType.Query:
						{
							switch (operation.SelectionSet.Kind)
							{
								case ASTNodeKind.Name:
									break;
								case ASTNodeKind.Document:
									break;
								case ASTNodeKind.OperationDefinition:
									break;
								case ASTNodeKind.VariableDefinition:
									break;
								case ASTNodeKind.Variable:
									break;
								case ASTNodeKind.SelectionSet:
								{
                                    /*
									var fields = operation.SelectionSet.Selections.Cast<GraphQLFieldSelection>();
									foreach (var field in fields)
									{
										Debug.Assert(field.Kind == ASTNodeKind.Field);
										var q = new QueryContext {Type = _schemas.GetTypeByName(field.Name.Value)};
										var f = field.SelectionSet.Selections.Cast<GraphQLFieldSelection>()
											.Select(x => x.Name.Value);
										q.Fields = new FieldOptions();
										q.Fields.Fields.AddRange(f);
										queries.Add(q);

										if (q.Type != null)
											q.Handle = typeof(IObjectSaveRepository).GetMethod("GetAsync");
									}
                                    */

									break;
								}
								case ASTNodeKind.Field:
									break;
								case ASTNodeKind.Argument:
									break;
								case ASTNodeKind.FragmentSpread:
									break;
								case ASTNodeKind.InlineFragment:
									break;
								case ASTNodeKind.FragmentDefinition:
									break;
								case ASTNodeKind.IntValue:
									break;
								case ASTNodeKind.FloatValue:
									break;
								case ASTNodeKind.StringValue:
									break;
								case ASTNodeKind.BooleanValue:
									break;
								case ASTNodeKind.EnumValue:
									break;
								case ASTNodeKind.ListValue:
									break;
								case ASTNodeKind.ObjectValue:
									break;
								case ASTNodeKind.ObjectField:
									break;
								case ASTNodeKind.Directive:
									break;
								case ASTNodeKind.NamedType:
									break;
								case ASTNodeKind.ListType:
									break;
								case ASTNodeKind.NonNullType:
									break;
								case ASTNodeKind.NullValue:
									break;
								case ASTNodeKind.SchemaDefinition:
									break;
								case ASTNodeKind.OperationTypeDefinition:
									break;
								case ASTNodeKind.ScalarTypeDefinition:
									break;
								case ASTNodeKind.ObjectTypeDefinition:
									break;
								case ASTNodeKind.FieldDefinition:
									break;
								case ASTNodeKind.InputValueDefinition:
									break;
								case ASTNodeKind.InterfaceTypeDefinition:
									break;
								case ASTNodeKind.UnionTypeDefinition:
									break;
								case ASTNodeKind.EnumTypeDefinition:
									break;
								case ASTNodeKind.EnumValueDefinition:
									break;
								case ASTNodeKind.InputObjectTypeDefinition:
									break;
								case ASTNodeKind.TypeExtensionDefinition:
									break;
								case ASTNodeKind.DirectiveDefinition:
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}

							break;
						}
						case OperationType.Mutation:
							break;
						case OperationType.Subscription:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

				if (definition is GraphQLFragmentDefinition fragment)
				{
				}
			}

			return queries;
		}

        public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes { get; }

        public IEnumerable<QueryContext> Parse(Type type, HttpContext source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QueryContext> Parse(Type type, ClaimsPrincipal user, string source)
        {
            throw new NotImplementedException();
        }
	}
}
