using System;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Mapping;
using NHibernate.Type;

namespace Headspring.BulkWriter.Nhibernate
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

        public void Add(string propertyName, object value, out int ordinal)
        {
            this.nameToOrdinalMap[propertyName] = this.currentOrdinal;
            this.ordinalToNameMap[this.currentOrdinal] = propertyName;
            this.ordinalToPropertyMap[this.currentOrdinal] = new StaticPropertyValueGetter(value);

            ordinal = this.currentOrdinal;

            this.currentOrdinal++;
        }

        public void Add(Property property, Configuration configuration, out int ordinal)
        {
            if (null == property)
            {
                throw new ArgumentNullException("property");
            }

            if (null == configuration)
            {
                throw new ArgumentNullException("configuration");
            }
            
            this.nameToOrdinalMap[property.Name] = this.currentOrdinal;
            this.ordinalToNameMap[this.currentOrdinal] = property.Name;

            IPropertyValueGetter getter = CreatePropertyValueGetter(property, this.itemType, configuration);
            this.ordinalToPropertyMap[this.currentOrdinal] = getter;

            ordinal = this.currentOrdinal;

            this.currentOrdinal++;
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

        public static bool ShouldMap(Property property)
        {
            if (null == property)
            {
                throw new ArgumentNullException("property");
            }
            
            return property.IsUpdateable && ((property.Value is ToOne) || (property.Value is SimpleValue));
        }

        private static IPropertyValueGetter CreatePropertyValueGetter(Property property, Type itemType, Configuration configuration)
        {
            var toOneValue = property.Value as ToOne;
            if (null != toOneValue)
            {
                return new ReferencePropertyValueGetter(property, itemType, configuration);
            }

            if (property.Type is XDocType)
            {
                return new XDocumentPropertyValueGetter(property, itemType);
            }

            return new SimplePropertyValueGetter(property, itemType);
        }
    }
}