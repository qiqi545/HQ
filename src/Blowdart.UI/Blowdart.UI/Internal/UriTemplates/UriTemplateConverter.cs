// Copyright (c) Tavis Software Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See THIRD_PARTY_NOTICES.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Blowdart.UI.Internal.UriTemplates
{
    /// <inheritdoc />
    /// <summary>
    /// Converts to <see cref="T:Blowdart.UI.Web.Internal.UriTemplates.UriTemplate" /> instances from other representations.
    /// </summary>
    public sealed class UriTemplateConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            if (value == null) { return null; }

            var template = value as string;
            if (template != null)
            {
                if (template.Length == 0)
                {
                    // For TypeConverter purposes, an empty string is "no value."
                    return null;
                }

                return new UriTemplate(template);
            }

            throw (NotSupportedException)GetConvertFromException(value);
        }
    }
}
