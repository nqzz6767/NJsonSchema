﻿using NJsonSchema.Generation;
using Xunit;

namespace NJsonSchema.Tests.Generation
{
    public class ShouldSerializeTests
    {
        public class Test
        {
            public string Name { get; set; }

            public bool ShouldSerializeName()
            {
                return !string.IsNullOrEmpty(Name);
            }
        }

        [Fact]
        public void When_ShouldSerialize_method_exists_then_schema_is_generated()
        {
            //// Arrange
            var schema = JsonSchemaGenerator.FromType<Test>();

            //// Act


            //// Assert
            Assert.NotNull(schema);
        }
    }
}
