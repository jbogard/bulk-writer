using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BulkWriter
{
    public interface IMapBuilderContext<TResult>
    {
        IMapBuilderContext<TResult> DestinationTable(string tableName);

        IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector);

        IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector, Action<IMapBuilderContextMap> configure);

        IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo);

        IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo, Action<IMapBuilderContextMap> configure);

        IMapping<TResult> Build();
    }
}