//-----------------------------------------------------------------------
// <copyright file="DataConversionGenerator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using NJsonSchema.CodeGeneration.TypeScript.Templates;

namespace NJsonSchema.CodeGeneration.TypeScript
{
    /// <summary>Generates data conversion code.</summary>
    public class DataConversionGenerator
    {
        /// <summary>Generates the code to convert a data object to the target class instances.</summary>
        /// <returns>The generated code.</returns>
        public static string RenderConvertToJavaScriptCode(DataConversionParameters parameters)
        {
            return new ConvertToJavaScriptTemplate(CreateModel(parameters)).Render();
        }

        /// <summary>Generates the code to convert a data object to the target class instances.</summary>
        /// <returns>The generated code.</returns>
        public static string RenderConvertToClassCode(DataConversionParameters parameters)
        {
            return new ConvertToClassTemplate(CreateModel(parameters)).Render();
        }

        private static object CreateModel(DataConversionParameters parameters)
        {
            var type = parameters.Resolver.Resolve(parameters.Schema, parameters.IsPropertyNullable, parameters.TypeNameHint);
            var defaultValueGenerator = new TypeScriptDefaultValueGenerator(parameters.Resolver);
            return new
            {
                Variable = parameters.Variable,
                Value = parameters.Value,

                HasDefaultValue = defaultValueGenerator.GetDefaultValue(parameters.Schema, parameters.IsPropertyNullable, type, parameters.TypeNameHint) != null,
                DefaultValue = defaultValueGenerator.GetDefaultValue(parameters.Schema, parameters.IsPropertyNullable, type, parameters.TypeNameHint),

                Type = type,

                IsNewableObject = IsNewableObject(parameters.Schema),
                IsDate = IsDate(parameters.Schema.Format, parameters.Settings.DateTimeType),

                IsDictionary = parameters.Schema.IsDictionary,
                DictionaryValueType = parameters.Resolver.TryResolve(parameters.Schema.AdditionalPropertiesSchema, parameters.TypeNameHint) ?? "any",
                IsDictionaryValueNewableObject = parameters.Schema.AdditionalPropertiesSchema != null && IsNewableObject(parameters.Schema.AdditionalPropertiesSchema),
                IsDictionaryValueDate = IsDate(parameters.Schema.AdditionalPropertiesSchema?.ActualSchema?.Format, parameters.Settings.DateTimeType),

                IsArray = parameters.Schema.Type.HasFlag(JsonObjectType.Array),
                ArrayItemType = parameters.Resolver.TryResolve(parameters.Schema.Item, parameters.TypeNameHint) ?? "any",
                IsArrayItemNewableObject = parameters.Schema.Item != null && IsNewableObject(parameters.Schema.Item),
                IsArrayItemDate = IsDate(parameters.Schema.Item?.Format, parameters.Settings.DateTimeType),

                StringToDateCode = parameters.Settings.DateTimeType == TypeScriptDateTimeType.Date ? "new Date" : "moment",
                DateToStringCode = "toISOString()"
            };
        }

        private static bool IsDate(string format, TypeScriptDateTimeType type)
        {
            // TODO: Make this more generic (see TypeScriptTypeResolver.ResolveString)
            if (type == TypeScriptDateTimeType.Date)
            {
                if (format == JsonFormatStrings.Date)
                    return true;

                if (format == JsonFormatStrings.DateTime)
                    return true;

                if (format == JsonFormatStrings.Time)
                    return false;

                if (format == JsonFormatStrings.TimeSpan)
                    return false;
            }
            else if (type == TypeScriptDateTimeType.MomentJS)
            {
                if (format == JsonFormatStrings.Date)
                    return true;

                if (format == JsonFormatStrings.DateTime)
                    return true;

                if (format == JsonFormatStrings.Time)
                    return true;

                if (format == JsonFormatStrings.TimeSpan)
                    return true;
            }
            return false;
        }

        private static bool IsNewableObject(JsonSchema4 schema)
        {
            schema = schema.ActualSchema;
            return schema.Type.HasFlag(JsonObjectType.Object) && !schema.IsAnyType && !schema.IsDictionary;
        }
    }
}