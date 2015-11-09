using System;

namespace table_descriptor
{
    /// <summary>
    /// Useful for when your database stores based on one key (Id) but your external searches are fetched commonly
    /// from another field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SearchKeyAttribute : Attribute
    {

    }
}