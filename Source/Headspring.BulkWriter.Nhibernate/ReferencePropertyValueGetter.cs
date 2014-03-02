using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NHibernate.Properties;

namespace Headspring.BulkWriter.Nhibernate
{
    public class ReferencePropertyValueGetter : SimplePropertyValueGetter
    {
        private readonly Configuration configuration;

        public ReferencePropertyValueGetter(Property property, Type itemType, Configuration configuration)
            : base(property, itemType)
        {
            this.configuration = configuration;
        }

        public override object Get(object item)
        {
            object value = base.Get(item);

            if (null != value)
            {
                Type type = NHibernateUtil.GetClass(value);
                PersistentClass classMapping = this.configuration.GetClassMapping(type);
                Property identifierProperty = classMapping.IdentifierProperty;
                IGetter getter = identifierProperty.GetGetter(type);
                value = getter.Get(value);
            }

            return value;
        }
    }
}