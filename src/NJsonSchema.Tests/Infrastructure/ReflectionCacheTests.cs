using System.Linq;
using NJsonSchema.Infrastructure;
using Xunit;

namespace NJsonSchema.Tests.Infrastructure
{
    public class ReflectionCacheTests
    {
        public class TestClass
        {
            public string field;

            public static string staticField;

            public string property { get; set; }

            public static string staticProperty { get; set; }
        }

        [Fact]
        public void Static_members_are_not_returned_from_ReflectionCache()
        {
            //// Act
            var propertiesAndFields = ReflectionCache.GetPropertiesAndFields(typeof(TestClass));

            //// Assert
            Assert.True(propertiesAndFields.Any(p => p.MemberInfo.Name == "field"));
            Assert.False(propertiesAndFields.Any(p => p.MemberInfo.Name == "staticField"));
            Assert.True(propertiesAndFields.Any(p => p.MemberInfo.Name == "property"));
            Assert.False(propertiesAndFields.Any(p => p.MemberInfo.Name == "staticProperty"));
        }
   }
}