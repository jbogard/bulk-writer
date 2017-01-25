using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using BulkWriter.Properties;

namespace BulkWriter
{
    internal static class AutoDiscover
    {
        public static string TableName<TResult>(bool quoted)
        {
            var pluralizationService = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

            string name = typeof (TResult).Name;
            if (!pluralizationService.IsPlural(name))
            {
                name = pluralizationService.Pluralize(name);
            }

            return quoted ? string.Format(CultureInfo.InvariantCulture, "[{0}]", name) : name;
        }

        public static void Mappings(string connectionString, string destinationTableName, IEnumerable<PropertyMapping> propertyMappings)
        {
            if (null == propertyMappings)
            {
                throw new ArgumentNullException("propertyMappings");
            }

            var sortedSchemaRow = SchemaReader.GetSortedSchemaRows(connectionString, destinationTableName);

            foreach (var mapping in propertyMappings)
            {
                if (mapping.ShouldMap)
                {
                    if (!mapping.Destination.IsPropertySet(MappingProperty.ColumnName))
                    {
                        mapping.Destination.ColumnName = mapping.Source.Property.Name;
                    }

                    var matchingSchemaRow = sortedSchemaRow.FirstOrDefault(x => string.Equals(x.ColumnName, mapping.Destination.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (null == matchingSchemaRow)
                    {
                        throw new InvalidOperationException(string.Format(Resources.Culture, Resources.AutoDiscover_Mappings_MappingDoesNotMatchDbColumn, mapping.Source.Property.Name, mapping.Destination.ColumnName));
                    }

                    if (!mapping.Destination.IsPropertySet(MappingProperty.ColumnOrdinal))
                    {
                        mapping.Destination.ColumnOrdinal = matchingSchemaRow.ColumnOrdinal;
                    }

                    if (!mapping.Destination.IsPropertySet(MappingProperty.ColumnSize))
                    {
                        mapping.Destination.ColumnSize = matchingSchemaRow.Size;
                    }

                    if (!mapping.Destination.IsPropertySet(MappingProperty.DataTypeName))
                    {
                        mapping.Destination.DataTypeName = matchingSchemaRow.DataTypeName;
                    }

                    if (!mapping.Destination.IsPropertySet(MappingProperty.IsKey))
                    {
                        mapping.Destination.IsKey = matchingSchemaRow.IsKey;
                    }
                }
            }
        }
    }
}