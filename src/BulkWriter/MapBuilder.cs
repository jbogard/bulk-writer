using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BulkWriter
{
    public static class MapBuilder
    {
        public static IMapping<TResult> BuildAllProperties<TResult>()
        {
            return MapAllProperties<TResult>().Build();
        }

        public static IMapBuilderContext<TResult> MapNoProperties<TResult>()
        {
            return new MapBuilderContext<TResult>();
        }

        public static IMapBuilderContext<TResult> MapAllProperties<TResult>()
        {
            var context = new MapBuilderContext<TResult>();

            var properties = typeof (TResult).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(IsAutoMappableProperty);
            foreach (var property in properties)
            {
                context.MapProperty(property);
            }

            return context;
        }

        private static bool IsAutoMappableProperty(PropertyInfo property)
        {
            return property.CanRead && (0 == property.GetIndexParameters().Length) && !IsCollectionType(property.PropertyType);
        }

        private static bool IsCollectionType(Type type)
        {
            // string implements IEnumerable, but for our purposes we don't consider it a collection.
            if (type == typeof (string))
            {
                return false;
            }

            var interfaces = type.GetInterfaces().Where(x => (x == typeof (IEnumerable)) || (x.IsGenericType && (x.GetGenericTypeDefinition() == typeof (IEnumerable<>))));
            return interfaces.Any();
        }
    }
}