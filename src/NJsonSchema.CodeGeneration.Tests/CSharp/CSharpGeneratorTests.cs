﻿using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.Tests.Models;
using NJsonSchema.Generation;

namespace NJsonSchema.CodeGeneration.Tests.CSharp
{
    [TestClass]
    public class CSharpGeneratorTests
    {
        [TestMethod]
        public void When_type_is_array_and_items_and_item_is_not_defined_then_any_array_is_generated()
        {
            //// Arrange
            var json = @"{
                'properties': {
                    'emptySchema': { 'type': 'array' }
                }
            }";
            var schema = JsonSchema4.FromJson(json);

            //// Act
            var settings = new CSharpGeneratorSettings() { ClassStyle = CSharpClassStyle.Poco, Namespace = "ns", };
            var generator = new CSharpGenerator(schema, settings);
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains("public ObservableCollection<object> EmptySchema { get; set; }"));
        }

        [TestMethod]
        public void When_all_of_has_multiple_refs_then_the_properties_should_expand_to_single_class()
        {
            //// Arrange
            var json = @"{
                '$schema': 'http://json-schema.org/draft-04/schema#',
                'id': 'http://some.domain.com/foo.json',
                'x-typeName': 'foo',
                'type': 'object',
                'additionalProperties': false,
                'definitions': {
                    'tRef1': {
                        'properties': {
                            'val1': {
                                'type': 'string',
                            }
                        }
                    },
                    'tRef2': {
                        'properties': {
                            'val2': {
                                'type': 'string',
                            }
                        }
                    },
                    'tRef3': {
                        'properties': {
                            'val3': {
                                'type': 'string',
                            }
                        }
                    }
                },
                'properties' : {
                    'tAgg': {
                        'allOf': [
                            {'$ref': '#/definitions/tRef1'},
                            {'$ref': '#/definitions/tRef2'},
                            {'$ref': '#/definitions/tRef3'}
                        ]
                    }
                }
            }";

            //// Act
            var schema = JsonSchema4.FromJson(json);
            var settings = new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco, Namespace = "ns" };
            var generator = new CSharpGenerator(schema, settings);
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains("public partial class TAgg"));
            Assert.IsTrue(output.Contains("public string Val1 { get; set; }"));
            Assert.IsTrue(output.Contains("public string Val2 { get; set; }"));
            Assert.IsTrue(output.Contains("public string Val3 { get; set; }"));
        }

        [TestMethod]
        public void When_more_properties_are_defined_in_allOf_and_type_none_then_all_of_contains_all_properties_in_generated_code()
        {
            //// Arrange
            var json = @"{
                '$schema': 'http://json-schema.org/draft-04/schema#',
                'type': 'object',
                'x-typeName': 'Foo', 
                'properties': { 
                    'prop1' : { 'type' : 'string' } 
                },
                'allOf': [
                    {
                        'properties': { 
                            'prop2' : { 'type' : 'string' } 
                        }
                    }
                ]
            }";

            //// Act
            var schema = JsonSchema4.FromJson(json);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("class Foo"));
            Assert.IsTrue(code.Contains("public string Prop1 { get; set; }"));
            Assert.IsTrue(code.Contains("public string Prop2 { get; set; }"));
            Assert.IsFalse(code.Contains("class Anonymous")); // only one class is generated with both properties
        }

        [TestMethod]
        public void When_allOf_schema_is_object_type_then_it_is_an_inherited_class_in_generated_code()
        {
            //// Arrange
            var json = @"{
                '$schema': 'http://json-schema.org/draft-04/schema#',
                'type': 'object',
                'x-typeName': 'Foo', 
                'properties': { 
                    'prop1' : { 'type' : 'string' } 
                },
                'allOf': [
                    {
                        'type': 'object', 
                        'x-typeName': 'Bar', 
                        'properties': { 
                            'prop2' : { 'type' : 'string' } 
                        }
                    }
                ]
            }";

            //// Act
            var schema = JsonSchema4.FromJson(json);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("class Foo : Bar"));
            Assert.IsTrue(code.Contains("public string Prop1 { get; set; }"));
            Assert.IsTrue(code.Contains("public string Prop2 { get; set; }"));
        }

        class CustomPropertyNameGenerator : IPropertyNameGenerator
        {
            public string Generate(JsonProperty property)
            {
                return "MyCustom" + ConversionUtilities.ConvertToUpperCamelCase(property.Name, true);
            }
        }
        class CustomTypeNameGenerator : ITypeNameGenerator
        {
            public string Generate(JsonSchema4 schema, string typeNameHint)
            {
                return "MyCustomType" + ConversionUtilities.ConvertToUpperCamelCase(schema.TypeNameRaw, true);
            }

        }

        [TestMethod]
        public void When_property_name_is_created_by_custom_fun_then_attribute_is_correct()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Teacher>();
            var schemaData = schema.ToJson();
            var settings = new CSharpGeneratorSettings();

            settings.TypeNameGenerator = new CustomTypeNameGenerator();
            settings.PropertyNameGenerator = new CustomPropertyNameGenerator();
            var generator = new CSharpGenerator(schema, settings);

            //// Act
            var output = generator.GenerateFile();
            Console.WriteLine(output);

            //// Assert
            Assert.IsTrue(output.Contains(@"[JsonProperty(""lastName"""));
            Assert.IsTrue(output.Contains(@"public string MyCustomLastName"));
            Assert.IsTrue(output.Contains(@"public partial class MyCustomTypeTeacher"));
            Assert.IsTrue(output.Contains(@"public partial class MyCustomTypePerson"));
        }

        [TestMethod]
        public void When_schema_contains_ref_to_definition_that_refs_another_definition_then_result_should_contain_correct_target_ref_type()
        {
            //// Arrange
            var schemaJson =
@"{
	'x-typeName': 'foo',
	'type': 'object',
	'definitions': {
		'pRef': {
			'type': 'object',
			'properties': {
				'pRef2': {
					'$ref': '#/definitions/pRef2'
				},
				
			}
		},
		'pRef2': {
			'type': 'string'
		}
	},
	'properties': {
		'pRefs': {
			'type': 'array',
			'items': {
				'$ref': '#/definitions/pRef'
			}
		}
	}
}";

            var schema = JsonSchema4.FromJson(schemaJson);
            var settings = new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco
            };
            var gen = new CSharpGenerator(schema, settings);

            //// Act
            var output = gen.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains("public ObservableCollection<PRef>"));
        }

        [TestMethod]
        public void When_property_has_boolean_default_it_is_reflected_in_the_poco()
        {
            var data = @"{'properties': {
                                'boolWithDefault': {
                                    'type': 'boolean',
                                    'default': false
                                 }
                             }}";

            var schema = JsonSchema4.FromJson(data);
            var settings = new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                Namespace = "ns",
                GenerateDefaultValues = true
            };
            var gen = new CSharpGenerator(schema, settings);
            var output = gen.GenerateFile();

            Assert.IsTrue(output.Contains("public bool BoolWithDefault { get; set; } = false;"));
        }

        [TestMethod]
        public void When_property_has_boolean_default_and_default_value_generation_is_disabled_then_default_value_is_not_generated()
        {
            var data = @"{'properties': {
                                'boolWithDefault': {
                                    'type': 'boolean',
                                    'default': false
                                 }
                             }}";

            var schema = JsonSchema4.FromJson(data);
            var settings = new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                Namespace = "ns",
                GenerateDefaultValues = false
            };
            var gen = new CSharpGenerator(schema, settings);
            var output = gen.GenerateFile();

            Assert.IsTrue(output.Contains("public bool BoolWithDefault { get; set; }"));
            Assert.IsFalse(output.Contains("public bool BoolWithDefault { get; set; } = false;"));
        }

        [TestMethod]
        public void When_namespace_is_set_then_it_should_appear_in_output()
        {
            //// Arrange
            var generator = CreateGenerator();

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains("namespace MyNamespace"));
            Assert.IsTrue(output.Contains("Dictionary<string, int>"));
        }

        [TestMethod]
        public void When_POCO_is_set_then_auto_properties_is_available()
        {
            //// Arrange
            var generator = CreateGenerator();
            generator.Settings.ClassStyle = CSharpClassStyle.Poco;

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains("{ get; set; }"));
        }

        [TestMethod]
        public void When_property_name_does_not_match_property_name_then_attribute_is_correct()
        {
            //// Arrange
            var generator = CreateGenerator();

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"[JsonProperty(""lastName"""));
            Assert.IsTrue(output.Contains(@"public string LastName"));
        }

        [TestMethod]
        public void When_property_is_timespan_than_csharp_timespan_is_used()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Person>();
            var data = schema.ToJson();
            var generator = new CSharpGenerator(schema);

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"public TimeSpan TimeSpan"));
        }

        [TestMethod]
        public void When_allOf_contains_one_schema_then_csharp_inheritance_is_generated()
        {
            //// Arrange
            var generator = CreateGenerator();

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"class Teacher : Person, "));
        }

        [TestMethod]
        public void When_enum_has_description_then_csharp_has_xml_comment()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Teacher>();
            schema.AllOf.First().Properties["Gender"].Description = "EnumDesc.";
            var generator = new CSharpGenerator(schema);

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"/// <summary>EnumDesc.</summary>"));
        }

        [TestMethod]
        public void When_class_has_description_then_csharp_has_xml_comment()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Teacher>();
            schema.Description = "ClassDesc.";
            var generator = new CSharpGenerator(schema);

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"/// <summary>ClassDesc.</summary>"));
        }

        [TestMethod]
        public void When_property_has_description_then_csharp_has_xml_comment()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Teacher>();
            schema.Properties["Class"].Description = "PropertyDesc.";
            var generator = new CSharpGenerator(schema);

            //// Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(output.Contains(@"/// <summary>PropertyDesc.</summary>"));
        }

        [TestMethod]
        public void Can_generate_type_from_string_property_with_byte_format()
        {
            // Arrange
            var schema = JsonSchema4.FromType<File>();
            var generator = new CSharpGenerator(schema);

            // Act
            var output = generator.GenerateFile();

            // Assert
            Assert.IsTrue(output.Contains("public byte[] Content"));
        }

        [TestMethod]
        public void Can_generate_type_from_string_property_with_base64_format()
        {
            // Arrange
            var schema = JsonSchema4.FromType<File>();
            schema.Properties["Content"].Format = "base64";
            var generator = new CSharpGenerator(schema);

            // Act
            var output = generator.GenerateFile();

            // Assert
            Assert.IsTrue(output.Contains("public byte[] Content"));
        }

        [TestMethod]
        public void When_name_contains_dash_then_it_is_converted_to_upper_case()
        {
            //// Arrange
            var schema = new JsonSchema4();
            schema.TypeNameRaw = "MyClass";
            schema.Properties["foo-bar"] = new JsonProperty
            {
                Type = JsonObjectType.String
            };

            var generator = new CSharpGenerator(schema);

            // Act
            var output = generator.GenerateFile();

            // Assert
            Assert.IsTrue(output.Contains(@"[JsonProperty(""foo-bar"", "));
            Assert.IsTrue(output.Contains(@"public string FooBar"));
        }

        [TestMethod]
        public void When_type_name_is_missing_then_anonymous_name_is_generated()
        {
            //// Arrange
            var schema = new JsonSchema4();
            var generator = new CSharpGenerator(schema);

            // Act
            var output = generator.GenerateFile();

            //// Assert
            Assert.IsFalse(output.Contains(@"class  :"));
        }

        private static CSharpGenerator CreateGenerator()
        {
            var schema = JsonSchema4.FromType<Teacher>();
            var schemaData = schema.ToJson();
            var settings = new CSharpGeneratorSettings();
            settings.Namespace = "MyNamespace";
            var generator = new CSharpGenerator(schema, settings);
            return generator;
        }


        private class ObjectTestClass
        {
            public object Foo { get; set; }
        }

        [TestMethod]
        public void When_property_is_object_then_any_type_is_generated()
        {
            //// Arrange

            //// Act
            var schema = JsonSchema4.FromType<ObjectTestClass>();

            //// Assert
            Assert.AreEqual(
@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#""
}", schema.Properties["Foo"].ActualPropertySchema.ToJson());
        }

        [TestMethod]
        public void When_property_is_object_then_object_property_is_generated()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<ObjectTestClass>();

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public object Foo { get; set; }"));
        }

        public enum ConstructionCode
        {
            FIRE_RSTV = 0,
            FRAME = 1,
            JOIST_MAS = 2,
            NON_CBST = 3,
        }

        public class ClassWithDefaultEnumProperty
        {
            [JsonConverter(typeof(StringEnumConverter))]
            [DefaultValue(ConstructionCode.NON_CBST)]
            public ConstructionCode ConstructionCode { get; set; }
        }

        [TestMethod]
        public void When_enum_property_has_default_and_int_serialization_then_correct_csharp_code_generated()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<ClassWithDefaultEnumProperty>();
            var schemaJson = schema.ToJson();

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public ConstructionCode ConstructionCode { get; set; } = ConstructionCode.NON_CBST;"));
        }

        [TestMethod]
        public void When_enum_property_has_default_and_string_serialization_then_correct_csharp_code_generated()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<ClassWithDefaultEnumProperty>(new JsonSchemaGeneratorSettings { DefaultEnumHandling = EnumHandling.String });
            var schemaJson = schema.ToJson();

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public ConstructionCode ConstructionCode { get; set; } = ConstructionCode.NON_CBST;"));
        }

        [TestMethod]
        public void When_enum_type_name_is_missing_then_default_value_is_still_correctly_set()
        {
            //// Arrange
            var schemaJson = @"{
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""ConstructionCode"": {
      ""type"": ""integer"",
      ""x-enumNames"": [
        ""FIRE_RSTV"",
        ""FRAME"",
        ""JOIST_MAS"",
        ""NON_CBST""
      ],
      ""enum"": [
        ""FIRE_RSTV"",
        ""FRAME"",
        ""JOIST_MAS"",
        ""NON_CBST""
      ],
      ""default"": ""JOIST_MAS""
    }
  }
}";
            var schema = JsonSchema4.FromJson(schemaJson);

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public ConstructionCode ConstructionCode { get; set; } = ConstructionCode.JOIST_MAS;"));
        }

        [TestMethod]
        public void When_property_has_same_name_as_class_then_it_is_renamed()
        {
            //// Arrange
            var schemaJson = @"{
  ""type"": ""object"",
  ""x-typeName"": ""Foo"",
  ""properties"": {
    ""Foo"": {
      ""type"": ""string""
    }
  }
}";
            var schema = JsonSchema4.FromJson(schemaJson);

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("[JsonProperty(\"Foo\", Required = Required.DisallowNull"));
            Assert.IsTrue(code.Contains("public string Foo1 { get; set; }"));
        }

        [TestMethod]
        public void When_patternProperties_is_set_with_string_value_type_then_correct_dictionary_is_generated()
        {
            //// Arrange
            var schemaJson = @"{
                ""properties"": {
                    ""dict"": {
                        ""type"": ""object"", 
                        ""additionalProperties"": false,
                        ""patternProperties"": {
                            ""^[a-zA-Z_$][a-zA-Z_$0-9]*$"": {
                                ""type"": ""string""
                            }
                        }
                    }
                }
            }";

            var schema = JsonSchema4.FromJson(schemaJson);

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public Dictionary<string, string> Dict { get; set; } = new Dictionary<string, string>();"));
        }

        [TestMethod]
        public void When_object_has_generic_name_then_it_is_transformed()
        {
            //// Arrange
            var schema = new JsonSchema4();
            schema.TypeNameRaw = "Foo[Bar[Inner]]";
            schema.Type = JsonObjectType.Object;
            schema.Properties["foo"] = new JsonProperty
            {
                Type = JsonObjectType.Number
            };

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public partial class FooOfBarOfInner"));
        }

        [JsonObject(MemberSerialization.OptIn)]
        [GeneratedCode("NJsonSchema", "3.4.6065.33501")]
        public partial class Person2
        {
            [JsonProperty("FirstName", Required = Required.Always)]
            [Required]
            public string FirstName { get; set; }

            [JsonProperty("MiddleName", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
            public string MiddleName { get; set; }

            [JsonProperty("Age", Required = Required.AllowNull)]
            public int? Age { get; set; }
        }

        [TestMethod]
        public void When_property_is_required_then_CSharp_code_is_correct()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<Person2>();
            var schemaJson = schema.ToJson();

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(schemaJson.Contains(
@"  ""required"": [
    ""FirstName"",
    ""Age""
  ],
  ""properties"": {
    ""FirstName"": {
      ""type"": ""string""
    },
    ""MiddleName"": {
      ""type"": ""string""
    },
    ""Age"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    }
  }"));

            Assert.IsTrue(code.Contains(
@"        [JsonProperty(""FirstName"", Required = Required.Always)]
        [Required]
        public string FirstName { get; set; }
    
        [JsonProperty(""MiddleName"", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string MiddleName { get; set; }
    
        [JsonProperty(""Age"", Required = Required.AllowNull)]
        public int? Age { get; set; }"));
        }

        [TestMethod]
        public void When_array_property_is_required_or_not_then_the_code_has_correct_initializer()
        {
            //// Arrange
            var schema = new JsonSchema4
            {
                Properties =
                {
                    { "A", new JsonProperty
                        {
                            Type = JsonObjectType.Array,
                            Item = new JsonSchema4
                            {
                                Type = JsonObjectType.String
                            },
                            IsRequired = true
                        }
                    },
                    { "B", new JsonProperty
                        {
                            Type = JsonObjectType.Array,
                            Item = new JsonSchema4
                            {
                                Type = JsonObjectType.String
                            },
                            IsRequired = false
                        }
                    },
                }
            };

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                NullHandling = NullHandling.Swagger
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public ObservableCollection<string> A { get; set; } = new ObservableCollection<string>();"));
            Assert.IsFalse(code.Contains("public ObservableCollection<string> B { get; set; } = new ObservableCollection<string>();"));
        }

        [TestMethod]
        public void When_dictionary_property_is_required_or_not_then_the_code_has_correct_initializer()
        {
            //// Arrange
            var schema = new JsonSchema4
            {
                Properties =
                {
                    { "A", new JsonProperty
                        {
                            Type = JsonObjectType.Object,
                            AdditionalPropertiesSchema = new JsonSchema4
                            {
                                Type = JsonObjectType.String
                            },
                            IsRequired = true
                        }
                    },
                    { "B", new JsonProperty
                        {
                            Type = JsonObjectType.Object,
                            AdditionalPropertiesSchema = new JsonSchema4
                            {
                                Type = JsonObjectType.String
                            },
                            IsRequired = false
                        }
                    },
                }
            };

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                NullHandling = NullHandling.Swagger
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public Dictionary<string, string> A { get; set; } = new Dictionary<string, string>();"));
            Assert.IsFalse(code.Contains("public Dictionary<string, string> B { get; set; } = new Dictionary<string, string>();"));
        }


        [TestMethod]
        public void When_object_property_is_required_or_not_then_the_code_has_correct_initializer()
        {
            //// Arrange
            var schema = new JsonSchema4
            {
                Properties =
                {
                    { "A", new JsonProperty
                        {
                            Type = JsonObjectType.Object,
                            Properties =
                            {
                                {"A", new JsonProperty
                                    {
                                        Type = JsonObjectType.String
                                    }
                                }
                            },
                            IsRequired = true
                        }
                    },
                    { "B", new JsonProperty
                        {
                            Type = JsonObjectType.Object,
                            Properties =
                            {
                                {"A", new JsonProperty
                                    {
                                        Type = JsonObjectType.String
                                    }
                                }
                            },
                            IsRequired = false
                        }
                    },
                }
            };

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                NullHandling = NullHandling.Swagger
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public A A { get; set; } = new A();"));
            Assert.IsFalse(code.Contains("public B B { get; set; } = new B();"));
        }

        [TestMethod]
        public void When_definition_is_named_Object_then_JObject_is_generated()
        {
            //// Arrange
            var json = 
@"{
	""type"": ""object"", 
	""properties"": {
		""foo"": {
			""$ref"": ""#/definitions/Object""
		}
	}, 
	""definitions"": {
		""Object"": { 
			""type"": ""object"", 
			""properties"": {} 
		}
	}
}";
            var schema = JsonSchema4.FromJson(json);

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                NullHandling = NullHandling.Swagger
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.IsTrue(code.Contains("public JObject Foo { get; set; }"));
        }

        public class ObsClass
        {
            public ObservableCollection<string> Test { get; set; }
        }

        [TestMethod]
        public void When_property_is_ObservableCollection_then_generated_code_uses_the_same_class()
        {
            //// Arrange
            var schema = JsonSchema4.FromType<ObsClass>();
            var settings = new CSharpGeneratorSettings();
            var generator = new CSharpGenerator(schema, settings);

            //// Act
            var output = generator.GenerateFile();
            Console.WriteLine(output);

            //// Assert
            Assert.IsTrue(output.Contains("ObservableCollection<string>"));
        }
    }
}
