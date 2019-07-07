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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using TypeKitchen;

namespace HQ.UI.Web.SemanticUi
{
	public static class DisplayPropertiesExtensions
	{
		public static DisplayProperties GetDisplayProperties(this AccessorMember field)
		{
			var properties = new DisplayProperties();

			properties.IsRequired = field.HasAttribute<RequiredAttribute>();

			properties.IsReadOnly = field.TryGetAttribute(out ReadOnlyAttribute readOnly)
				? readOnly.IsReadOnly
				: !field.CanWrite;

			properties.IsDisabled = field.TryGetAttribute(out EditableAttribute editable)
				? editable.AllowEdit
				: !field.CanRead;

			properties.IsHidden = field.TryGetAttribute(out BrowsableAttribute browsable) &&
			                      !browsable.Browsable;

			properties.IsVisible = !field.HasAttribute<IgnoreDataMemberAttribute>() &&
			                       !field.HasAttribute<NonSerializedAttribute>() &&
			                       !field.HasAttribute<NotMappedAttribute>();

			if (field.TryGetAttribute(out DataTypeAttribute dataType))
			{
				properties.Type = dataType.GetDataTypeName();
				properties.Format = dataType.DisplayFormat.DataFormatString;
			}

			if (field.TryGetAttribute(out DisplayAttribute display))
			{
				properties.Description = display.GetDescription();
				//properties.Order = display.GetOrder();
				//properties.Section = display.GetGroupName();
				properties.Label = display.GetName();
				properties.Placeholder = display.GetPrompt();

				var shortName = display.GetShortName();
				if (shortName != null && shortName != properties.Label)
					properties.Annotation = display.GetShortName();
			}

			if (properties.Label == null) properties.Label = field.Name;

			if (field.TryGetAttribute(out DisplayNameAttribute displayName)) properties.Label = displayName.DisplayName;

			return properties;
		}
	}
}