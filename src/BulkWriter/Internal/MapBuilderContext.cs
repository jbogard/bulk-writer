using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public class MapBuilderContext<TResult> : IMapBuilderContext<TResult>
    {
        private readonly Dictionary<PropertyInfo, PropertyMapping> _mappings = new Dictionary<PropertyInfo, PropertyMapping>();

        private string _destinationTableName;

        public IMapBuilderContext<TResult> DestinationTable(string tableName)
        {
            if (null == tableName)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (0 == tableName.Length)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ToDestinationTable_InvalidTableName, nameof(tableName));
            }

            _destinationTableName = tableName;

            return this;
        }

        public IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector) => MapProperty(propertySelector, null);

        public IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector, Action<IMapBuilderContextMap> configure)
        {
            var property = ExtractPropertyInfo(propertySelector);
            var mapping = new PropertyMapping(new MappingSource(property, _mappings.Count));
            
            if (null != configure)
            {
                var map = new MapBuilderContextMap(mapping);
                configure(map);
            }

            _mappings[property] = mapping;

            return this;
        }

        public IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            return MapProperty(propertyInfo, null);
        }

        public IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo, Action<IMapBuilderContextMap> configure)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            var mapping = new PropertyMapping(new MappingSource(propertyInfo, _mappings.Count));

            if (null != configure)
            {
                var map = new MapBuilderContextMap(mapping);
                configure(map);
            }

            _mappings[propertyInfo] = mapping;

            return this;
        }

        public IMapping<TResult> Build() => new Mapping<TResult>(_destinationTableName, _mappings.Values);

        /// <summary>
        /// Implemented to facilitate testing. Not intended to be used in your code.
        /// </summary>
        /// <returns>A collection of property mappings this instance is managing.</returns>
        public IEnumerable<PropertyMapping> GetPropertyMappings() => _mappings.Values;

        private static PropertyInfo ExtractPropertyInfo<TMember>(Expression<Func<TResult, TMember>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (null == memberExpression)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ExtractPropertyInfo_InvalidPropertySelector, nameof(propertySelector));
            }

            var property = memberExpression.Member as PropertyInfo;
            if (null == property)
            {
                throw new ArgumentException(Resources.MapBuilderContext_ExtractPropertyInfo_InvalidPropertySelector, nameof(propertySelector));
            }

            return NormalizePropertyInfo(property);
        }

        private static PropertyInfo NormalizePropertyInfo(PropertyInfo property)
        {
            if (null == property)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return typeof(TResult).GetProperty(property.Name);
        }
    }
}