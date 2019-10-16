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
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using TypeKitchen;

namespace HQ.Data.Contracts.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public abstract class DelegatedValidationAttribute : ValidationAttribute
	{
		protected static ScriptOptions Options;

		static DelegatedValidationAttribute()
		{
			var builder = Snippet.GetBuilder();
			Options = builder
				.Add<ReflectionTypeResolver>()
				.Add<DelegatedValidationAttribute>()
				.Add<RequiredAttribute>()
				.Build();
		}

		protected DelegatedValidationAttribute(MethodInfo handler) => Condition ??= (Func<object, bool>) Delegate.CreateDelegate(typeof(Func<object, bool>), null, handler);

		public Func<object, bool> Condition { get; set; }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var result = ValidationResult.Success;

			return Condition == null ? result :
				Condition(validationContext.ObjectInstance) ? result : Invalid(validationContext);
		}

		private ValidationResult Invalid(ValidationContext validationContext)
		{
			var errorMessage = FormatErrorMessage(validationContext.DisplayName);

			return new ValidationResult(errorMessage, new[] {validationContext.MemberName});
		}
	}
}