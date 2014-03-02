using System.Reflection;
using System.Xml.Linq;

namespace Headspring.BulkWriter.DecoratedModel
{
    public class XElementPropertyValueGetter : SimplePropertyValueGetter
    {
        public XElementPropertyValueGetter(PropertyInfo property)
            : base(property)
        {
        }

        public override object Get(object item)
        {
            object value = base.Get(item);

            var element = value as XElement;
            if (null != element)
            {
                value = element.ToString();
            }

            return value;
        }
    }
}