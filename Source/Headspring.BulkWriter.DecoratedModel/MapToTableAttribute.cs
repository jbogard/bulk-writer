using System;

namespace Headspring.BulkWriter.DecoratedModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class MapToTableAttribute : Attribute
    {
        private readonly string name;

        public MapToTableAttribute(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }
}