using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace BulkWriter
{
    public interface IMapBuilderContext<TResult>
    {
        IMapBuilderContext<TResult> DestinationTable(string tableName);

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "As designed. Need an expression tree.")]
        IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector);

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "As designed. Need an expression tree.")]
        IMapBuilderContext<TResult> MapProperty<TMember>(Expression<Func<TResult, TMember>> propertySelector, Action<IMapBuilderContextMap> configure);

        IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo);

        IMapBuilderContext<TResult> MapProperty(PropertyInfo propertyInfo, Action<IMapBuilderContextMap> configure);

        IMapping<TResult> Build();
    }
}