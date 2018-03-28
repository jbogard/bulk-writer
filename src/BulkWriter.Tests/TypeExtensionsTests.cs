using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    public class AnnotatedEntity
    {
        public int Id { get; private set; }

        [Column("StringProperty")]
        public string MappedStringProperty { get; set; }

        [NotMapped]
        public decimal NotMappedDecimalProperty { get; private set; }
        
    }

    public class TypeExtensionsTests
    {
        [Fact]
        public void Not_Mapped_Attribute_Should_Not_Map()
        {
            var propertyMappings = typeof(AnnotatedEntity).BuildMappings();
            var notMappedProperty = propertyMappings.SingleOrDefault(x => x.Source.Property.Name == nameof(AnnotatedEntity.NotMappedDecimalProperty));

            Assert.False(notMappedProperty.ShouldMap);
        }

        [Fact]
        public void Column_Attribute_Should_Map_With_Proper_Source_And_Destination()
        {
            var propertyMappings = typeof(AnnotatedEntity).BuildMappings();
            var mappedProperty = propertyMappings.SingleOrDefault(x => x.Source.Property.Name == nameof(AnnotatedEntity.MappedStringProperty));

            Assert.True(mappedProperty.Destination.ColumnName == "StringProperty");
            Assert.True(mappedProperty.ShouldMap);
        }
    }
}