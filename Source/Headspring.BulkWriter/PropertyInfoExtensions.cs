using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Headspring.BulkWriter.Properties;

namespace Headspring.BulkWriter
{
    public delegate object GetPropertyValueHandler(object instance);

    internal static class PropertyInfoExtensions
    {
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<PropertyInfo, GetPropertyValueHandler> CachedGetters = new Dictionary<PropertyInfo, GetPropertyValueHandler>();

        public static GetPropertyValueHandler GetValueGetter(this PropertyInfo propertyInfo)
        {
            if (null == propertyInfo)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            if (null == propertyInfo.DeclaringType)
            {
                throw new ArgumentException(Resources.PropertyInfoExtensions_PropertyNotDeclaredOnType, "propertyInfo");
            }

            GetPropertyValueHandler getter;

            ReaderWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (!CachedGetters.TryGetValue(propertyInfo, out getter))
                {
                    ReaderWriterLock.EnterWriteLock();

                    try
                    {
                        var instance = Expression.Parameter(typeof(object), "instance");
                        var convertedInstance = Expression.Convert(instance, propertyInfo.DeclaringType);
                        var propertyCall = Expression.Property(convertedInstance, propertyInfo);
                        var convertedPropertyValue = Expression.Convert(propertyCall, typeof(object));

                        var lambda = Expression.Lambda<GetPropertyValueHandler>(convertedPropertyValue, instance);
                        var compiled = lambda.Compile();

                        CachedGetters[propertyInfo] = getter = compiled;
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }

            return getter;
        }
    }
}