using System;

namespace table_descriptor
{
    public class ColumnNameAttribute : Attribute
    {
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }
}