using System.Reflection;
using Headspring.BulkWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BulkWriterTests
{
    [TestClass]
    public class PropertyInfoExtensionsTests
    {
        [TestMethod]
        public void Can_Get_Correct_ValueType_Property_Value()
        {
            var valueTypeProperty = typeof (MyTestClass).GetProperty("ValueTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(valueTypeProperty);
            var valueTypePropertyValueGetter = valueTypeProperty.GetValueGetter();
            Assert.IsNotNull(valueTypePropertyValueGetter);
            
            var testClass = new MyTestClass();

            object zeroValue = valueTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsInstanceOfType(zeroValue, typeof (int));
            Assert.AreEqual(0, zeroValue);

            testClass.ValueTypeProperty = 418;
            object fourOneEightValue = valueTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsInstanceOfType(fourOneEightValue, typeof(int));
            Assert.AreEqual(418, fourOneEightValue);
        }

        [TestMethod]
        public void Can_Get_Correct_ReferenceType_Property_Value()
        {
            var referenceTypeProperty = typeof(MyTestClass).GetProperty("ReferenceTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(referenceTypeProperty);
            var referenceTypePropertyValueGetter = referenceTypeProperty.GetValueGetter();
            Assert.IsNotNull(referenceTypePropertyValueGetter);

            var testClass = new MyTestClass();

            object nullValue = referenceTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsNull(nullValue);

            testClass.ReferenceTypeProperty = "418";
            object fourOneEightValue = referenceTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsInstanceOfType(fourOneEightValue, typeof(string));
            Assert.AreEqual("418", fourOneEightValue);
        }

        [TestMethod]
        public void Can_Get_Correct_NullableType_PropertyValue()
        {
            var nullableTypeProperty = typeof(MyTestClass).GetProperty("NullableTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(nullableTypeProperty);
            var nullableTypePropertyValueGetter = nullableTypeProperty.GetValueGetter();
            Assert.IsNotNull(nullableTypePropertyValueGetter);

            var testClass = new MyTestClass();

            object nullValue = nullableTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsNull(nullValue);

            testClass.NullableTypeProperty = 418;
            object fourOneEightValue = nullableTypePropertyValueGetter.DynamicInvoke(testClass);
            Assert.IsInstanceOfType(fourOneEightValue, typeof(int?));
            Assert.AreEqual(418, fourOneEightValue);
        }

        [TestMethod]
        public void Value_Getter_Is_Cached()
        {
            var valueTypeProperty = typeof(MyTestClass).GetProperty("ValueTypeProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(valueTypeProperty);
            
            var valueTypePropertyValueGetter1 = valueTypeProperty.GetValueGetter();
            Assert.IsNotNull(valueTypePropertyValueGetter1);

            var valueTypePropertyValueGetter2 = valueTypeProperty.GetValueGetter();
            Assert.IsNotNull(valueTypePropertyValueGetter2);

            Assert.AreSame(valueTypePropertyValueGetter1, valueTypePropertyValueGetter2);
        }

        public class MyTestClass
        {
            public int ValueTypeProperty { get; set; }

            public string ReferenceTypeProperty { get; set; }

            public int? NullableTypeProperty { get; set; }
        }
    }
}
