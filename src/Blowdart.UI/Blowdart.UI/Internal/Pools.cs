using System.Buffers;
using System.Reflection;
using Microsoft.Extensions.ObjectPool;

namespace Blowdart.UI.Internal
{
	internal static class Pools
	{
		public static readonly ArrayPool<Assembly> AssemblyPool = ArrayPool<Assembly>.Create();
		public static readonly ObjectPool<UiAction> ActionPool = new DefaultObjectPool<UiAction>(new ActionPoolPolicy());
       
        public static NoContainer AutoResolver { get; set; }
		
        internal class ActionPoolPolicy : IPooledObjectPolicy<UiAction>
        {
            public UiAction Create()
            {
                return new UiAction();
            }

            public bool Return(UiAction action)
            {
                action.Arguments = null;
                action.MethodName = null;
                return true;
            }
        }
    }
}