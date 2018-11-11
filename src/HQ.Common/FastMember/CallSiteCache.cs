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
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace HQ.Common.FastMember
{
    internal static class CallSiteCache
    {
        private static readonly Hashtable getters = new Hashtable(), setters = new Hashtable();

        internal static object GetValue(string name, object target)
        {
            var callSite = (CallSite<Func<CallSite, object, object>>) getters[name];
            if (callSite == null)
            {
                var newSite = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None,
                    name, typeof(CallSiteCache),
                    new[] {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)}));
                lock (getters)
                {
                    callSite = (CallSite<Func<CallSite, object, object>>) getters[name];
                    if (callSite == null) getters[name] = callSite = newSite;
                }
            }

            return callSite.Target(callSite, target);
        }

        internal static void SetValue(string name, object target, object value)
        {
            var callSite = (CallSite<Func<CallSite, object, object, object>>) setters[name];
            if (callSite == null)
            {
                var newSite = CallSite<Func<CallSite, object, object, object>>.Create(
                    Binder.SetMember(CSharpBinderFlags.None, name, typeof(CallSiteCache),
                        new[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                        }));
                lock (setters)
                {
                    callSite = (CallSite<Func<CallSite, object, object, object>>) setters[name];
                    if (callSite == null) setters[name] = callSite = newSite;
                }
            }

            callSite.Target(callSite, target, value);
        }
    }
}
