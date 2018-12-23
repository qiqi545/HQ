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
using System.Dynamic;

namespace HQ.Common.FastMember
{
    /// <summary>
    ///     Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        ///     Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }

        /// <summary>
        ///     The object represented by this instance
        /// </summary>
        public abstract object Target { get; }

        /// <summary>
        ///     Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return Target.Equals(obj);
        }

        /// <summary>
        ///     Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }

        /// <summary>
        ///     Use the target's definition of a string representation
        /// </summary>
        public override string ToString()
        {
            return Target.ToString();
        }

        /// <summary>
        ///     Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target)
        {
            return Create(target, false);
        }

        /// <summary>
        ///     Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target, bool allowNonPublicAccessors)
        {
            if (target == null) throw new ArgumentNullException("target");
            var dlr = target as IDynamicMetaObjectProvider;
            if (dlr != null) return new DynamicWrapper(dlr); // use the DLR
            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType(), allowNonPublicAccessors));
        }

        private sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly TypeAccessor accessor;

            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                Target = target;
                this.accessor = accessor;
            }

            public override object this[string name]
            {
                get => accessor[Target, name];
                set => accessor[Target, name] = value;
            }

            public override object Target { get; }
        }

        private sealed class DynamicWrapper : ObjectAccessor
        {
            private readonly IDynamicMetaObjectProvider target;

            public DynamicWrapper(IDynamicMetaObjectProvider target)
            {
                this.target = target;
            }

            public override object Target => target;

            public override object this[string name]
            {
                get => CallSiteCache.GetValue(name, target);
                set => CallSiteCache.SetValue(name, target, value);
            }
        }
    }
}
