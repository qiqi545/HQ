using System;

namespace table_descriptor
{
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }
}