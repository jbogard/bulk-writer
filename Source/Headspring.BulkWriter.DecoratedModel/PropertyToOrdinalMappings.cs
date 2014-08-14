using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Headspring.BulkWriter.DecoratedModel
{
    public class PropertyToOrdinalMappings : IPropertyToOrdinalMappings
    {
        private readonly Type itemType;
        private readonly Dictionary<string, int> nameToOrdinalMap = new Dictionary<string, int>();
        private readonly Dictionary<int, string> ordinalToNameMap = new Dictionary<int, string>();
        private readonly Dictionary<int, IPropertyValueGetter> ordinalToPropertyMap = new Dictionary<int, IPropertyValueGetter>();

        private int currentOrdinal;

        public PropertyToOrdinalMappings(Type itemType)
        {
            this.itemType = itemType;
        }

        public int FieldCount
        {
            get { return (this.currentOrdinal - 1); }
        }

        public int GetOrdinal(string propertyName)
        {
            return this.nameToOrdinalMap[propertyName];
        }

        public string GetName(int ordinal)
        {
            return this.ordinalToNameMap[ordinal];
        }

        public object GetValue(int ordinal, object item)
        {
            IPropertyValueGetter getter = this.ordinalToPropertyMap[ordinal];

            object value = getter.Get(item);
            return value;
        }

        public void Add(PropertyInfo property, out int ordinal)
        {
            if (null == property)
            {
                throw new ArgumentNullException("property");
            }

            this.nameToOrdinalMap[property.Name] = this.currentOrdinal;
            this.ordinalToNameMap[this.currentOrdinal] = property.Name;

            IPropertyValueGetter getter = CreatePropertyValueGetter(property, this.itemType);
            this.ordinalToPropertyMap[this.currentOrdinal] = getter;

            ordinal = this.currentOrdinal;

            this.currentOrdinal++;
        }

        private static IPropertyValueGetter CreatePropertyValueGetter(PropertyInfo property, Type itemType)
        {
            if (property.PropertyType == typeof (XElement))
            {
                return new XElementPropertyValueGetter(property);
            }

            return new SimplePropertyValueGetter(property);
        }
    }
}