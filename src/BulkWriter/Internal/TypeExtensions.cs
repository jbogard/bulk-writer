using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace BulkWriter.Internal
{
    public static class TypeExtensions
    {
        public static PropertyMapping[] BuildMappings(this Type type) => type.GetRuntimeProperties()
            .Select((pi, i) => new PropertyMapping
            {
                Source = new MappingSource
                {
                    Property = pi,
                    Ordinal = i
                },
                ShouldMap = pi.GetCustomAttribute<NotMappedAttribute>() == null,
                Destination = new MappingDestination
                {
                    ColumnName = pi.GetCustomAttribute<ColumnAttribute>()?.Name ?? pi.Name,
                    ColumnOrdinal = pi.GetCustomAttribute<ColumnAttribute>()?.Order ?? i,
                    ColumnSize = pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0,
                    DataTypeName = pi.GetCustomAttribute<ColumnAttribute>()?.TypeName,
                    IsKey = pi.GetCustomAttribute<KeyAttribute>() != null
                }
            })
            .ToArray();
    }
}