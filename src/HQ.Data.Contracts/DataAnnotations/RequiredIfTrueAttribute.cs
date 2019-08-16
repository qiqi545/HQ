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

using System.ComponentModel.DataAnnotations;
using TypeKitchen;

namespace HQ.Data.Contracts.DataAnnotations
{
	public sealed class RequiredIfTrueAttribute : ValidationAttribute
	{
		private readonly string _propertyOrFieldName;

		public RequiredIfTrueAttribute(string propertyOrFieldName)
		{
			_propertyOrFieldName = propertyOrFieldName;
			ErrorMessage = $"{FormatErrorMessage(propertyOrFieldName)} is true, therefore, {{0}} is required.";
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var accessor = ReadAccessor.Create(validationContext.ObjectInstance, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, out var members);
			if (!members.TryGetValue(_propertyOrFieldName, out _))
				return ValidationResult.Success; // user error

			if(!accessor.TryGetValue(validationContext.ObjectInstance, _propertyOrFieldName, out var propertyOrField))
				return ValidationResult.Success; // user error

			if (!(propertyOrField is bool flag))
				return ValidationResult.Success; // user error

			if(!flag)
				return ValidationResult.Success; // not required

			var attribute = new RequiredAttribute();
			return !attribute.IsValid(value) ? Invalid(validationContext) : ValidationResult.Success;
		}

		private ValidationResult Invalid(ValidationContext validationContext)
		{
			var errorMessage = FormatErrorMessage(validationContext.DisplayName);

			return new ValidationResult(errorMessage, new[] { validationContext.MemberName });
		}
	}
}