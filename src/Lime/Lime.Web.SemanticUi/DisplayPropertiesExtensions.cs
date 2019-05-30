// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using TypeKitchen;

namespace Lime.Web.SemanticUi
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