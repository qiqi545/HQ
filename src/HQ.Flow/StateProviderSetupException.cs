using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace HQ.Flow
{
	[Serializable]
	public class StateProviderSetupException : Exception
	{
		private ReadOnlyCollection<MethodInfo> StateMethods { get; }

		public StateProviderSetupException(string message, IEnumerable<MethodInfo> stateMethods) : base(message)
		{
			StateMethods = new ReadOnlyCollection<MethodInfo>(stateMethods.ToList());
		}

		protected StateProviderSetupException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			StateMethods = info.GetValue(nameof(StateMethods), typeof(ReadOnlyCollection<MethodInfo>)) as ReadOnlyCollection<MethodInfo>;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException(nameof(info));
			info.AddValue(nameof(StateMethods), StateMethods);
			base.GetObjectData(info, context);
		}
	}
}