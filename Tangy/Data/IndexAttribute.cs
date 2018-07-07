using System;

namespace Tangy.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IndexAttribute : Attribute
    {
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
    }
}