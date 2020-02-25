using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace BulkWriter.Internal
{
    internal static class TypeExtensions
    {
        public static PropertyMapping[] BuildMappings(this Type type) =>
           type.GetRuntimeProperties()
             .Select((pi, i) =>
             {
                 var destinationParam = new DestinationParams(pi, i);

                 return new PropertyMapping
                 {
                     Source = new MappingSource
                     {
                         Property = pi,
                         Ordinal = i
                     },
                     ShouldMap = pi.GetCustomAttribute<NotMappedAttribute>() == null,
                     Destination = new MappingDestination
                     {
                         ColumnName = destinationParam.ColumnName,
                         ColumnOrdinal = destinationParam.ColumnOrdinal,
                         ColumnSize = destinationParam.ColumnSize,
                         DataTypeName = destinationParam.DataTypeName,
                         IsKey = destinationParam.IsKey
                     }
                 };
             })
             .ToArray();

        internal class DestinationParams
        {
            public DestinationParams(MemberInfo pi, int index)
            {
                ColumnOrdinal = index;
                ColumnName = pi.Name;
                ColumnSize = pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0;
                IsKey = pi.GetCustomAttribute<KeyAttribute>() != null;

                var columnAttribute = pi.GetCustomAttribute<ColumnAttribute>();

                if (columnAttribute != null)
                {
                    ColumnOrdinal = columnAttribute.Order > -1 ? columnAttribute.Order : ColumnOrdinal;
                    ColumnName = !string.IsNullOrWhiteSpace(columnAttribute.Name) ? columnAttribute.Name : ColumnName;
                    DataTypeName = columnAttribute.TypeName;
                }
            }

            public string ColumnName { get; }
            public int ColumnOrdinal { get; }
            public int ColumnSize { get; }
            public string DataTypeName { get; }
            public bool IsKey { get; }
        }
    }
}