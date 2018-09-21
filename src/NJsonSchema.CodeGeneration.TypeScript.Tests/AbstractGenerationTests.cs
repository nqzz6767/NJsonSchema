﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using Xunit;

namespace NJsonSchema.CodeGeneration.TypeScript.Tests
{
    public class AbstractGenerationTests
    {
        public abstract class AbstractClass : BaseClass
        {
            public string Foo { get; set; }
        }

        [Fact]
        public async Task When_class_is_abstract_then_is_abstract_TypeScript_keyword_is_generated()
        {
            /// Arrange
            var schema = await JsonSchema4.FromTypeAsync<AbstractClass>();

            /// Act
            var generator = new TypeScriptGenerator(schema, new TypeScriptGeneratorSettings { TypeScriptVersion = 2.0m });
            var code = generator.GenerateFile("AbstractClass");

            /// Assert
            Assert.Contains("export abstract class AbstractClass", code);
        }

        public class ContainerClass
        {
            [Required]
            public AbstractClass Foo { get; set; }
        }

        [Fact]
        public async Task When_property_is_required_and_abstract_then_it_is_not_instantiated()
        {
            /// Arrange
            var schema = await JsonSchema4.FromTypeAsync<ContainerClass>();

            /// Act
            var generator = new TypeScriptGenerator(schema, new TypeScriptGeneratorSettings { TypeScriptVersion = 2.0m });
            var code = generator.GenerateFile("ContainerClass");

            /// Assert
            Assert.Contains("this.foo = data[\"Foo\"] ? AbstractClass.fromJS(data[\"Foo\"]) : <any>undefined;", code);
        }

        [KnownType(typeof(SuperClass))]
        [KnownType(typeof(AbstractClass))]
        [JsonConverter(typeof(JsonInheritanceConverter))]
        public class BaseClass
        {

        }

        public class SuperClass : AbstractClass
        {

        }

        [Fact]
        public async Task When_abstract_class_is_in_inheritance_hierarchy_then_it_is_newer_instantiated()
        {
            /// Arrange
            var schema = await JsonSchema4.FromTypeAsync<AbstractClass>();

            /// Act
            var generator = new TypeScriptGenerator(schema, new TypeScriptGeneratorSettings { TypeScriptVersion = 2.0m });
            var code = generator.GenerateFile("AbstractClass");

            /// Assert
            Assert.DoesNotContain("new AbstractClass();", code);
        }
    }
}
