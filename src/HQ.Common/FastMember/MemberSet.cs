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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HQ.Common.FastMember
{
    /// <summary>
    ///     Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IList<Member>
    {
        private readonly Member[] members;

        internal MemberSet(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            members = type.GetProperties(PublicInstance).Cast<MemberInfo>().Concat(type.GetFields(PublicInstance))
                .OrderBy(x => x.Name)
                .Select(member => new Member(member)).ToArray();
        }

        /// <summary>
        ///     Get a member by index
        /// </summary>
        public Member this[int index] => members[index];

        /// <summary>
        ///     Return a sequence of all defined members
        /// </summary>
        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in members) yield return member;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     The number of members defined for this type
        /// </summary>
        public int Count => members.Length;

        Member IList<Member>.this[int index]
        {
            get => members[index];
            set => throw new NotSupportedException();
        }

        bool ICollection<Member>.Remove(Member item)
        {
            throw new NotSupportedException();
        }

        void ICollection<Member>.Add(Member item)
        {
            throw new NotSupportedException();
        }

        void ICollection<Member>.Clear()
        {
            throw new NotSupportedException();
        }

        void IList<Member>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList<Member>.Insert(int index, Member item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<Member>.Contains(Member item)
        {
            return members.Contains(item);
        }

        void ICollection<Member>.CopyTo(Member[] array, int arrayIndex)
        {
            members.CopyTo(array, arrayIndex);
        }

        bool ICollection<Member>.IsReadOnly => true;

        int IList<Member>.IndexOf(Member member)
        {
            return Array.IndexOf(members, member);
        }
    }

    /// <summary>
    ///     Represents an abstracted view of an individual member defined for a type
    /// </summary>
    public sealed class Member
    {
        private readonly MemberInfo member;

        internal Member(MemberInfo member)
        {
            this.member = member;
        }

        /// <summary>
        ///     The name of this member
        /// </summary>
        public string Name => member.Name;

        /// <summary>
        ///     The type of value stored in this member
        /// </summary>
        public Type Type
        {
            get
            {
                if (member is FieldInfo) return ((FieldInfo) member).FieldType;
                if (member is PropertyInfo) return ((PropertyInfo) member).PropertyType;
                throw new NotSupportedException(member.GetType().Name);
            }
        }

        /// <summary>
        ///     Property Can Write
        /// </summary>
        public bool CanWrite
        {
            get
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo) member).CanWrite;
                    default: throw new NotSupportedException(member.MemberType.ToString());
                }
            }
        }

        /// <summary>
        ///     Property Can Read
        /// </summary>
        public bool CanRead
        {
            get
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo) member).CanRead;
                    default: throw new NotSupportedException(member.MemberType.ToString());
                }
            }
        }

        /// <summary>
        ///     Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            return Attribute.IsDefined(member, attributeType);
        }

        /// <summary>
        ///     Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute(Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttribute(member, attributeType, inherit);
        }
    }
}
