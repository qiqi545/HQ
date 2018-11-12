namespace table_descriptor
{
    public class PropertyToColumn
    {
        public PropertyAccessor Property { get; }
        public string ColumnName { get; set; }
        public bool IsComputed { get; set; }
        public bool IsKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsSearchKey { get; set; }
        
        public PropertyToColumn(PropertyAccessor typedPropertyDescriptor)
        {
            Property = typedPropertyDescriptor;
            ColumnName = Property.Name;
            Property.Name = Property.Name;
        }
    }
}