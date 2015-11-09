using System;

namespace table_descriptor
{
    public class KeyAttribute : Attribute
    {
        public bool Identity { get; private set; }

        public KeyAttribute(bool identity)
        {
            Identity = identity;
        }
    }
}