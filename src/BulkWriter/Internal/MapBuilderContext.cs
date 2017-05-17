using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class MapBuilderContext<TResult> : IMapBuilderContext<TResult>
    {
        private readonly Dictionary<PropertyInfo, PropertyMapping> mappings = new Dictionary<PropertyInfo, PropertyMapping>();

        private string destinationTableName;

        public IMapBuilderContext<TResult> DestinationTable(string tableName)
        {
            if (null == tableName)
            {
                throw new ArgumentNullException("tableName");
            }

            if (0 == tableName.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ToDestinationTable_InvalidTableName, "tableName");
            }

            this.destinationTableName = tableName;

            return this;
        }

        public IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector)
        {
            return this.MapProperty(propertySelector, null);
        }

        public IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector, Action<IMapBuilderContextMap> configure)
        {
            var property = ExtractPropertyInfo(propertySelector);
            var mapping = new PropertyMapping(new MappingSource(property, this.mappings.Count));
            
            if (null != configure)
            {
                var map = new MapBuilderContextMap(mapping);
                configure(map);
            }

            this.mappings[property] = mapping;

            return this;
        }

        public IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            return this.MapProperty(propertyInfo, null);
        }

        public IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo, Action<IMapBuilderContextMap> configure)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            var mapping = new PropertyMapping(new MappingSource(propertyInfo, this.mappings.Count));

            if (null != configure)
            {
                var map = new MapBuilderContextMap(mapping);
                configure(map);
            }

            this.mappings[propertyInfo] = mapping;

            return this;
        }

        public IMapping<TResult> Build()
        {
            return new Mapping<TResult>(this.destinationTableName, this.mappings.Values);
        }

        /// <summary>
        /// Implemented to facilitate testing. Not intended to be used in your code.
        /// </summary>
        /// <returns>A collection of property mappings this instance is managing.</returns>
        public IEnumerable<PropertyMapping> GetPropertyMappings()
        {
            return this.mappings.Values;
        } 

        private static PropertyInfo ExtractPropertyInfo<TMember>(Expression<Func<TResult, TMember>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (null == memberExpression)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ExtractPropertyInfo_InvalidPropertySelector, "propertySelector");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (null == property)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ExtractPropertyInfo_InvalidPropertySelector, "propertySelector");
            }

            return NormalizePropertyInfo(property);
        }

        private static PropertyInfo NormalizePropertyInfo(PropertyInfo property)
        {
            if (null == property)
            {
                throw new ArgumentNullException("property");
            }

            return typeof(TResult).GetProperty(property.Name);
        }
    }
}