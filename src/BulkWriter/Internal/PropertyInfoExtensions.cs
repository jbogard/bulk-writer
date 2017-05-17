using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BulkWriter.Properties;

namespace BulkWriter.Internal
{
    public static class PropertyInfoExtensions
    {
        private static readonly Dictionary<PropertyInfo, GetPropertyValueHandler> CachedGetters = new Dictionary<PropertyInfo, GetPropertyValueHandler>();

        public static GetPropertyValueHandler GetValueGetter(this PropertyInfo propertyInfo)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (null == propertyInfo.DeclaringType)
            {
                throw new ArgumentException(Resources.PropertyInfoExtensions_PropertyNotDeclaredOnType, nameof(propertyInfo));
            }

            GetPropertyValueHandler getter;

            lock (CachedGetters)
            {
                if (!CachedGetters.TryGetValue(propertyInfo, out getter))
                {
                    var instance = Expression.Parameter(typeof(object), "instance");
                    var convertedInstance = Expression.Convert(instance, propertyInfo.DeclaringType);
                    var propertyCall = Expression.Property(convertedInstance, propertyInfo);
                    var convertedPropertyValue = Expression.Convert(propertyCall, typeof(object));

                    var lambda = Expression.Lambda<GetPropertyValueHandler>(convertedPropertyValue, instance);
                    var compiled = lambda.Compile();

                    CachedGetters[propertyInfo] = getter = compiled;
                }
            }

            return getter;
        }
    }
}