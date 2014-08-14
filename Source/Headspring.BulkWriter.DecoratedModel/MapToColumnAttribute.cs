using System;

namespace Headspring.BulkWriter.DecoratedModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MapToColumnAttribute : Attribute
    {
        private readonly string name;
        private readonly int ordinal;

        public MapToColumnAttribute(string name, int ordinal)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            this.name = name;
            this.ordinal = ordinal;
        }

        public string Name
        {
            get { return this.name; }
        }

        public int Ordinal
        {
            get { return this.ordinal; }
        }

        public bool InsertIdentity { get; set; }
    }
}