using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Headspring.BulkWriter.DecoratedModel
{
    internal static class PropertyInfoExtensions
    {
        private static readonly Dictionary<PropertyInfo, Delegate> CachedGetters = new Dictionary<PropertyInfo, Delegate>();

        public static Delegate GetValueGetter(this PropertyInfo propertyInfo)
        {
            Delegate getter;
            if (!CachedGetters.TryGetValue(propertyInfo, out getter))
            {
                Debug.Assert(propertyInfo.DeclaringType != null, "propertyInfo.DeclaringType != null");
                ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
                MemberExpression property = Expression.Property(instance, propertyInfo);
                UnaryExpression convert = Expression.TypeAs(property, typeof (object));

                CachedGetters[propertyInfo] = getter = Expression.Lambda(convert, instance).Compile();
            }

            return getter;
        }
    }
}