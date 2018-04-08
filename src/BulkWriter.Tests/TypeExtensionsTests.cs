using System.ComponentModel.DataAnnotations;
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

    [Table("MyTable")]
    public class MyTestClass
    {
        [Column("CustomId")]
        [Key]
        public int Id { get; set; }

        [Column("CustomName")]
        public string Name { get; set; }

        [NotMapped]
        public string IgnoredColumn { get; set; }
    }
    
    public class TypeExtensionsTests
    {
        [Fact]
        public void Verify_Build_Mappings_Annotations()
        {
            var mappings = typeof(MyTestClass).BuildMappings();

            Assert.Equal(3, mappings.Length);

            var idMap = mappings.Single(m => m.Source.Property.Name == nameof(MyTestClass.Id));
            Assert.Equal("CustomId", idMap.Destination.ColumnName);
            Assert.Equal(0, idMap.Destination.ColumnOrdinal);
            Assert.True(idMap.Destination.IsKey);
            Assert.Equal(0, idMap.Source.Ordinal);
            Assert.Equal(nameof(MyTestClass.Id), idMap.Source.Property.Name);
            Assert.True(idMap.ShouldMap);

            var nameMap = mappings.Single(m => m.Source.Property.Name == nameof(MyTestClass.Name));
            Assert.Equal("CustomName", nameMap.Destination.ColumnName);
            Assert.Equal(1, nameMap.Destination.ColumnOrdinal);
            Assert.False(nameMap.Destination.IsKey);
            Assert.Equal(1, nameMap.Source.Ordinal);
            Assert.Equal(nameof(MyTestClass.Name), nameMap.Source.Property.Name);
            Assert.True(nameMap.ShouldMap);

            var ignoreColumnMap = mappings.Single(m => m.Source.Property.Name == nameof(MyTestClass.IgnoredColumn));
            Assert.Equal(nameof(MyTestClass.IgnoredColumn), ignoreColumnMap.Destination.ColumnName);
            Assert.Equal(2, ignoreColumnMap.Destination.ColumnOrdinal);
            Assert.False(ignoreColumnMap.Destination.IsKey);
            Assert.Equal(2, ignoreColumnMap.Source.Ordinal);
            Assert.Equal(nameof(MyTestClass.IgnoredColumn), ignoreColumnMap.Source.Property.Name);
            Assert.False(ignoreColumnMap.ShouldMap);
        } 

        [Fact]
        public void Not_Mapped_Attribute_Should_Not_Map()
        {
            var propertyMappings = typeof(AnnotatedEntity).BuildMappings();
            var notMappedProperty = propertyMappings.Single(x => x.Source.Property.Name == nameof(AnnotatedEntity.NotMappedDecimalProperty));

            Assert.False(notMappedProperty.ShouldMap);
        }

        [Fact]
        public void Column_Attribute_Should_Map_With_Proper_Source_And_Destination()
        {
            var propertyMappings = typeof(AnnotatedEntity).BuildMappings();
            var mappedProperty = propertyMappings.Single(x => x.Source.Property.Name == nameof(AnnotatedEntity.MappedStringProperty));

            Assert.True(mappedProperty.Destination.ColumnName == "StringProperty");
            Assert.True(mappedProperty.ShouldMap);
        }
    }
}