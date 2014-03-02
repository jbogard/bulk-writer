using System;
using NHibernate.Mapping;
using NHibernate.Properties;

namespace Headspring.BulkWriter.Nhibernate
{
    public class SimplePropertyValueGetter : IPropertyValueGetter
    {
        private readonly Type itemType;
        private readonly Property property;

        public SimplePropertyValueGetter(Property property, Type itemType)
        {
            this.property = property;
            this.itemType = itemType;
        }

        protected Property Property
        {
            get { return this.property; }
        }

        protected Type ItemType
        {
            get { return this.itemType; }
        }

        public virtual object Get(object item)
        {
            IGetter getter = this.property.GetGetter(this.itemType);

            object value = getter.Get(item);
            return value;
        }
    }
}