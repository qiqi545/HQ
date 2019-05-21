// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using TypeKitchen;

namespace Blowdart.UI.Web.SemanticUI
{
	public struct DisplayProperties
	{
		public bool IsReadOnly { get; set; }
		public bool IsVisible { get; set; }
		public bool IsHidden { get; set; }
		public bool IsRequired { get; set; }

		public string Label { get; set; }
		public string Placeholder { get; set; }
		public string Description { get; set; }
	}

	public static class DisplayPropertiesExtensions
	{
		public static DisplayProperties GetDisplayProperties(this AccessorMember field)
		{
			var properties = new DisplayProperties();

			properties.IsRequired = field.HasAttribute<RequiredAttribute>();

			properties.IsReadOnly = field.TryGetAttribute(out ReadOnlyAttribute readOnly)
				? readOnly.IsReadOnly
				: !field.CanWrite;

			properties.IsHidden = field.TryGetAttribute(out BrowsableAttribute browsable) && 
			                      !browsable.Browsable;

			properties.IsVisible = !field.HasAttribute<IgnoreDataMemberAttribute>() && 
			                       !field.HasAttribute<NonSerializedAttribute>();
			
			if (field.TryGetAttribute(out DisplayAttribute display))
			{
				properties.Description = display.Description;
				//properties.Order = display.Order;
				//properties.Section = display.GroupName;
				properties.Label = display.Name;
				properties.Placeholder = display.Prompt;
			}
			else
			{
				properties.Label = field.Name;
			}

			return properties;
		}
	}
}