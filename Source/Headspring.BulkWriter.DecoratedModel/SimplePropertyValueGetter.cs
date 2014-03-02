using System;
using System.Reflection;

namespace Headspring.BulkWriter.DecoratedModel
{
    public class SimplePropertyValueGetter : IPropertyValueGetter
    {
        private readonly Delegate getter;

        public SimplePropertyValueGetter(PropertyInfo property)
        {
            this.getter = property.GetValueGetter();
        }

        public virtual object Get(object item)
        {
            object value = getter.DynamicInvoke(item);
            return value;
        }
    }
}