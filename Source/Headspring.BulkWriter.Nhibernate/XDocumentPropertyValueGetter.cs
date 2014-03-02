using System;
using NHibernate;
using NHibernate.Mapping;

namespace Headspring.BulkWriter.Nhibernate
{
    public class XDocumentPropertyValueGetter : SimplePropertyValueGetter
    {
        public XDocumentPropertyValueGetter(Property property, Type itemType)
            : base(property, itemType)
        {
        }

        public override object Get(object item)
        {
            object value = base.Get(item);

            if (null != value)
            {
                value = NHibernateUtil.XDoc.ToString(value);
            }

            return value;
        }
    }
}