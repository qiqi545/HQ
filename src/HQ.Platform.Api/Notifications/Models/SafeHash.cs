﻿#region LICENSE

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

using System.Dynamic;
using DotLiquid;

namespace HQ.Platform.Api.Notifications.Models
{
	/// <summary>
	///     Allows a dynamic hash to access values that don't exist
	/// </summary>
	internal class SafeHash : DynamicObject
	{
		private readonly Hash _child;

		public SafeHash(Hash child) => _child = child;

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (_child.ContainsKey(binder.Name)) _child[binder.Name] = value;
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = _child.ContainsKey(binder.Name) ? _child[binder.Name] : null;
			return true;
		}
	}
}