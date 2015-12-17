using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NJsonSchema.Tests.Generation
{
    [TestClass]
    public class AttributeGenerationTests
    {
        [TestMethod]
        public void When_minLength_and_maxLength_attribute_are_available_then_minLength_and_maxLenght_are_set()
        {
            //// Arrange

            //// Act
            var schema = JsonSchema4.FromType<AttributeTestClass>();
            var property = schema.Properties["Items"];

            //// Assert
            Assert.AreEqual(3, property.MinLength);
            Assert.AreEqual(5, property.MaxLength);
        }

        [TestMethod]
        public void When_display_attribute_is_available_then_name_and_description_are_read()
        {
            //// Arrange


            //// Act
            var schema = JsonSchema4.FromType<AttributeTestClass>();
            var property = schema.Properties["Display"];

            //// Assert
            Assert.AreEqual("Foo", property.Title);
            Assert.AreEqual("Bar", property.Description);
        }

        [TestMethod]
        public void When_description_attribute_is_available_then_description_are_read()
        {
            //// Arrange


            //// Act
            var schema = JsonSchema4.FromType<AttributeTestClass>();
            var property = schema.Properties["Description"];

            //// Assert
            Assert.AreEqual("Abc", property.Description);
        }

        public class AttributeTestClass
        {
            [MinLength(3)]
            [MaxLength(5)]
            public string[] Items { get; set; }

            [Display(Name = "Foo", Description = "Bar")]
            public string Display { get; set; }

            [System.ComponentModel.Description("Abc")]
            public string Description { get; set; }
        }
    }
}