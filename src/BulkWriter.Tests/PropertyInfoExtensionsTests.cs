using System;
using System.Reflection;
using BulkWriter.Internal;
using Xunit;

namespace BulkWriter.Tests
{
    
    public class PropertyInfoExtensionsTests
    {
        [Fact]
        public void Can_Get_Correct_ValueType_Property_Value()
        {
            var valueTypeProperty = typeof (MyTestClass).GetProperty("ValueTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(valueTypeProperty);
            var valueTypePropertyValueGetter = valueTypeProperty.GetValueGetter();
            Assert.NotNull(valueTypePropertyValueGetter);
            
            var testClass = new MyTestClass();

            var zeroValue = valueTypePropertyValueGetter(testClass);
            Assert.IsType(typeof (int), zeroValue);
            Assert.Equal(0, zeroValue);

            testClass.ValueTypeProperty = 418;
            var fourOneEightValue = valueTypePropertyValueGetter(testClass);
            Assert.IsType(typeof(int), fourOneEightValue);
            Assert.Equal(418, fourOneEightValue);
        }

        [Fact]
        public void Can_Get_Correct_ReferenceType_Property_Value()
        {
            var referenceTypeProperty = typeof(MyTestClass).GetProperty("ReferenceTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(referenceTypeProperty);
            var referenceTypePropertyValueGetter = referenceTypeProperty.GetValueGetter();
            Assert.NotNull(referenceTypePropertyValueGetter);

            var testClass = new MyTestClass();

            var nullValue = referenceTypePropertyValueGetter(testClass);
            Assert.Null(nullValue);

            testClass.ReferenceTypeProperty = "418";
            var fourOneEightValue = referenceTypePropertyValueGetter(testClass);
            Assert.IsType(typeof(string), fourOneEightValue);
            Assert.Equal("418", fourOneEightValue);
        }

        [Fact(Skip="I hate nullables")]
        public void Can_Get_Correct_NullableType_PropertyValue()
        {
            var nullableTypeProperty = typeof(MyTestClass).GetProperty("NullableTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nullableTypeProperty);
            var nullableTypePropertyValueGetter = nullableTypeProperty.GetValueGetter();
            Assert.NotNull(nullableTypePropertyValueGetter);

            var testClass = new MyTestClass();

            var nullValue = nullableTypePropertyValueGetter(testClass);
            Assert.Null(nullValue);

            testClass.NullableTypeProperty = 418;
            var fourOneEightValue = nullableTypePropertyValueGetter(testClass);
            Assert.Equal(typeof(int?), Nullable.GetUnderlyingType(fourOneEightValue.GetType()));
            Assert.Equal(418, fourOneEightValue);
        }

        [Fact]
        public void Value_Getter_Is_Cached()
        {
            var valueTypeProperty = typeof(MyTestClass).GetProperty("ValueTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(valueTypeProperty);
            
            var valueTypePropertyValueGetter1 = valueTypeProperty.GetValueGetter();
            Assert.NotNull(valueTypePropertyValueGetter1);

            var valueTypePropertyValueGetter2 = valueTypeProperty.GetValueGetter();
            Assert.NotNull(valueTypePropertyValueGetter2);

            Assert.Same(valueTypePropertyValueGetter1, valueTypePropertyValueGetter2);
        }

        public class MyTestClass
        {
            public int ValueTypeProperty { get; set; }

            public string ReferenceTypeProperty { get; set; }

            public int? NullableTypeProperty { get; set; }
        }
    }
}
