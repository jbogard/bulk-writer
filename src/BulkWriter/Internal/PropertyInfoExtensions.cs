using System;
using System.Collections.Generic;
using System.Reflection;
using BulkWriter.Properties;
using static System.Linq.Expressions.Expression;

namespace BulkWriter.Internal
{
    internal static class PropertyInfoExtensions
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
                    var instance = Parameter(typeof(object), "instance");
                    var convertedInstance = Convert(instance, propertyInfo.DeclaringType);
                    var propertyCall = Property(convertedInstance, propertyInfo);
                    var convertedPropertyValue = Convert(propertyCall, typeof(object));

                    var lambda = Lambda<GetPropertyValueHandler>(convertedPropertyValue, instance);
                    var compiled = lambda.Compile();

                    CachedGetters[propertyInfo] = getter = compiled;
                }
            }

            return getter;
        }
    }
}