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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace HQ.Extensions.Messaging
{
	[Serializable]
	public class UnusedStateMethodsException : Exception
	{
		public UnusedStateMethodsException(ICollection<MethodInfo> stateMethods) : base(
			"State methods were unused (probably a naming error or undefined state):\n" +
			string.Join("\n", stateMethods)) =>
			StateMethods = new ReadOnlyCollection<string>(stateMethods.Select(x => x.Name).ToList());

		protected UnusedStateMethodsException(SerializationInfo info, StreamingContext context)
			: base(info, context) =>
			StateMethods =
				info.GetValue(nameof(StateMethods), typeof(ReadOnlyCollection<string>)) as ReadOnlyCollection<string>;

		public ReadOnlyCollection<string> StateMethods { get; }

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			info.AddValue(nameof(StateMethods), StateMethods);
			base.GetObjectData(info, context);
		}
	}
}