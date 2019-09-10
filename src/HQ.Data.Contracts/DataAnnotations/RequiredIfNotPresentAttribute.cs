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

using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using TypeKitchen;

namespace HQ.Data.Contracts.DataAnnotations
{
	public sealed class RequiredIfNotPresentAttribute : DelegatedValidationAttribute
	{
		public RequiredIfNotPresentAttribute(string propertyOrFieldName, bool allowEmptyStrings = false) : base(
			ResolveDelegateType(propertyOrFieldName, allowEmptyStrings, Options))
		{
			AllowEmptyStrings = allowEmptyStrings;

			ErrorMessage = $"{FormatErrorMessage(propertyOrFieldName)} is not present, therefore, {{0}} is required.";
		}

		public bool AllowEmptyStrings { get; }

		private static MethodInfo ResolveDelegateType(string propertyOrFieldName, bool allowEmptyStrings, ScriptOptions options)
		{
			var handler = Snippet.CreateMethod("public static bool Validate(object value)" +
			                                   $"{{ " +
			                                   $"   var accessor = ReadAccessor.Create(value); " +
			                                   $"   if(accessor.TryGetValue(value, \"{propertyOrFieldName}\", out var propertyOrField))" +
			                                   $"   {{ " +
			                                   $"       var attribute = new RequiredAttribute {{ AllowEmptyStrings = {(allowEmptyStrings ? "true" : "false")} }};" +
			                                   $"       var present = attribute.IsValid(propertyOrField); " +
			                                   $"       return present ? attribute.IsValid(value) : true;" +
			                                   $"   }}" +
			                                   $"   return false;" +
			                                   $"}}", options);
			return handler;
		}
	}
}