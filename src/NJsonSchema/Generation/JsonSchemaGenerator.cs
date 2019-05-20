//-----------------------------------------------------------------------
// <copyright file="JsonSchemaGenerator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Annotations;
using NJsonSchema.Converters;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NJsonSchema.Generation
{
    /// <summary>Generates a <see cref="JsonSchema4"/> object for a given type. </summary>
    public class JsonSchemaGenerator
    {
        private static readonly Dictionary<string, string> DataTypeFormats = new Dictionary<string, string>
        {
            {"DateTime", JsonFormatStrings.DateTime},
            {"Date", JsonFormatStrings.Date},
            {"Time", JsonFormatStrings.Time},
            {"EmailAddress", JsonFormatStrings.Email},
            {"PhoneNumber", JsonFormatStrings.Phone},
            {"Url", JsonFormatStrings.Uri}
        };

        /// <summary>Initializes a new instance of the <see cref="JsonSchemaGenerator"/> class.</summary>
        /// <param name="settings">The settings.</param>
        public JsonSchemaGenerator(JsonSchemaGeneratorSettings settings)
        {
            Settings = settings;
        }

        /// <summary>Gets the settings.</summary>
        public JsonSchemaGeneratorSettings Settings { get; }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public async Task<JsonSchema4> GenerateAsync(Type type)
        {
            var schema = new JsonSchema4();
            var schemaResolver = new JsonSchemaResolver(schema, Settings);
            await GenerateAsync(type.ToContextualType(), schema, schemaResolver).ConfigureAwait(false);
            return schema;
        }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <param name="type">The type.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public Task<JsonSchema4> GenerateAsync(Type type, JsonSchemaResolver schemaResolver)
        {
            return GenerateAsync<JsonSchema4>(type, schemaResolver);
        }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <param name="type">The type.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public Task<TSchemaType> GenerateAsync<TSchemaType>(Type type, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            return GenerateAsync<TSchemaType>(type.ToContextualType(), schemaResolver);
        }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <param name="contextualType">The type.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public async Task<JsonSchema4> GenerateAsync(ContextualType contextualType, JsonSchemaResolver schemaResolver)
        {
            return await GenerateAsync<JsonSchema4>(contextualType, schemaResolver).ConfigureAwait(false);
        }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <param name="contextualType">The type.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public async Task<TSchemaType> GenerateAsync<TSchemaType>(ContextualType contextualType, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var schema = new TSchemaType();
            await GenerateAsync(contextualType, schema, schemaResolver).ConfigureAwait(false);
            return schema;
        }

        /// <summary>Generates a <see cref="JsonSchema4" /> object for the given type and adds the mapping to the given resolver.</summary>
        /// <typeparam name="TSchemaType">The type of the schema.</typeparam>
        /// <param name="contextualType">The type.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The schema.</returns>
        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        public virtual async Task GenerateAsync<TSchemaType>(ContextualType contextualType, TSchemaType schema, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var type = contextualType.OriginalType;

            var jsonSchemaTypeAttribute = contextualType.GetAttribute<JsonSchemaTypeAttribute>();
            if (jsonSchemaTypeAttribute != null)
            {
                type = jsonSchemaTypeAttribute.Type;
            }

            ApplyExtensionDataAttributes(contextualType, schema);

            if (await TryHandleSpecialTypesAsync(contextualType, schema, schemaResolver).ConfigureAwait(false))
            {
                await ApplySchemaProcessorsAsync(contextualType, schema, schemaResolver).ConfigureAwait(false);
                return;
            }

            if (schemaResolver.RootObject == schema)
            {
                schema.Title = Settings.SchemaNameGenerator.Generate(type);
            }

            var typeDescription = Settings.ReflectionService.GetDescription(contextualType, Settings);
            if (typeDescription.Type.HasFlag(JsonObjectType.Object))
            {
                if (typeDescription.IsDictionary)
                {
                    typeDescription.ApplyType(schema);
                    await GenerateDictionaryAsync(schema, contextualType, schemaResolver).ConfigureAwait(false);
                }
                else
                {
                    if (schemaResolver.HasSchema(type, false))
                    {
                        schema.Reference = schemaResolver.GetSchema(type, false);
                    }
                    else if (schema.GetType() == typeof(JsonSchema4))
                    {
                        await GenerateObjectAsync(type, typeDescription, schema, schemaResolver).ConfigureAwait(false);
                    }
                    else
                    {
                        schema.Reference = await GenerateAsync(contextualType, schemaResolver).ConfigureAwait(false);
                    }
                }
            }
            else if (typeDescription.IsEnum)
            {
                await GenerateEnum(schema, contextualType, typeDescription, schemaResolver).ConfigureAwait(false);
            }
            else if (typeDescription.Type.HasFlag(JsonObjectType.Array)) // TODO: Add support for tuples?
            {
                await GenerateArray(schema, contextualType, typeDescription, schemaResolver).ConfigureAwait(false);
            }
            else
            {
                typeDescription.ApplyType(schema);
            }

            await ApplySchemaProcessorsAsync(contextualType, schema, schemaResolver).ConfigureAwait(false);
        }

        /// <summary>Generetes a schema directly or referenced for the requested schema type; 
        /// does NOT change nullability.</summary>
        /// <typeparam name="TSchemaType">The resulted schema type which may reference the actual schema.</typeparam>
        /// <param name="contextualType">The type of the schema to generate.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <param name="transformation">An action to transform the resulting schema (e.g. property or parameter) before the type of reference is determined (with $ref or allOf/oneOf).</param>
        /// <returns>The requested schema object.</returns>
        public async Task<TSchemaType> GenerateWithReferenceAsync<TSchemaType>(
            ContextualType contextualType,
            JsonSchemaResolver schemaResolver,
            Func<TSchemaType, JsonSchema4, Task> transformation = null)
            where TSchemaType : JsonSchema4, new()
        {
            return await GenerateWithReferenceAndNullabilityAsync(contextualType, false, schemaResolver, transformation).ConfigureAwait(false);
        }

        /// <summary>Generetes a schema directly or referenced for the requested schema type; 
        /// also adds nullability if required by looking at the type's <see cref="JsonTypeDescription" />.</summary>
        /// <typeparam name="TSchemaType">The resulted schema type which may reference the actual schema.</typeparam>
        /// <param name="contextualType">The type of the schema to generate.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <param name="transformation">An action to transform the resulting schema (e.g. property or parameter) before the type of reference is determined (with $ref or allOf/oneOf).</param>
        /// <returns>The requested schema object.</returns>
        public async Task<TSchemaType> GenerateWithReferenceAndNullabilityAsync<TSchemaType>(
            ContextualType contextualType, JsonSchemaResolver schemaResolver,
            Func<TSchemaType, JsonSchema4, Task> transformation = null)
            where TSchemaType : JsonSchema4, new()
        {
            var typeDescription = Settings.ReflectionService.GetDescription(contextualType, Settings);
            return await GenerateWithReferenceAndNullabilityAsync(contextualType, typeDescription.IsNullable, schemaResolver, transformation).ConfigureAwait(false);
        }

        /// <summary>Generetes a schema directly or referenced for the requested schema type; also adds nullability if required.</summary>
        /// <typeparam name="TSchemaType">The resulted schema type which may reference the actual schema.</typeparam>
        /// <param name="contextualType">The type of the schema to generate.</param>
        /// <param name="isNullable">Specifies whether the property, parameter or requested schema type is nullable.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <param name="transformation">An action to transform the resulting schema (e.g. property or parameter) before the type of reference is determined (with $ref or allOf/oneOf).</param>
        /// <returns>The requested schema object.</returns>
        public virtual async Task<TSchemaType> GenerateWithReferenceAndNullabilityAsync<TSchemaType>(
            ContextualType contextualType, bool isNullable, JsonSchemaResolver schemaResolver,
            Func<TSchemaType, JsonSchema4, Task> transformation = null)
            where TSchemaType : JsonSchema4, new()
        {
            var typeDescription = Settings.ReflectionService.GetDescription(contextualType, Settings);
            var requiresSchemaReference = typeDescription.RequiresSchemaReference(Settings.TypeMappers);

            JsonSchema4 referencedSchema;
            if (!requiresSchemaReference)
            {
                var schema = await GenerateAsync<TSchemaType>(contextualType, schemaResolver).ConfigureAwait(false);
                if (!schema.HasReference)
                {
                    if (transformation != null)
                    {
                        await transformation(schema, schema).ConfigureAwait(false);
                    }

                    if (isNullable)
                    {
                        if (Settings.SchemaType == SchemaType.JsonSchema)
                        {
                            if (schema.Type == JsonObjectType.None)
                            {
                                schema.OneOf.Add(new JsonSchema4 { Type = JsonObjectType.None });
                                schema.OneOf.Add(new JsonSchema4 { Type = JsonObjectType.Null });
                            }
                            else
                            {
                                schema.Type = schema.Type | JsonObjectType.Null;
                            }
                        }
                        else if (Settings.SchemaType == SchemaType.OpenApi3 || Settings.GenerateCustomNullableProperties)
                        {
                            schema.IsNullableRaw = isNullable;
                        }
                    }

                    return schema;
                }
                else // TODO: Is this else needed?
                {
                    referencedSchema = schema.ActualSchema;
                }
            }
            else
            {
                referencedSchema = await GenerateAsync<JsonSchema4>(contextualType, schemaResolver).ConfigureAwait(false);
            }

            var referencingSchema = new TSchemaType();
            if (transformation != null)
            {
                await transformation(referencingSchema, referencedSchema).ConfigureAwait(false);
            }

            if (isNullable)
            {
                if (Settings.SchemaType == SchemaType.JsonSchema)
                {
                    referencingSchema.OneOf.Add(new JsonSchema4 { Type = JsonObjectType.Null });
                }
                else if (Settings.SchemaType == SchemaType.OpenApi3 || Settings.GenerateCustomNullableProperties)
                {
                    referencingSchema.IsNullableRaw = true;
                }
            }

            // See https://github.com/RSuter/NJsonSchema/issues/531
            var useDirectReference = Settings.AllowReferencesWithProperties ||
                !JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(referencingSchema)).Properties().Any(); // TODO: Improve performance

            if (useDirectReference && referencingSchema.OneOf.Count == 0)
            {
                referencingSchema.Reference = referencedSchema.ActualSchema;
            }
            else if (Settings.SchemaType != SchemaType.Swagger2)
            {
                referencingSchema.OneOf.Add(new JsonSchema4
                {
                    Reference = referencedSchema.ActualSchema
                });
            }
            else
            {
                referencingSchema.AllOf.Add(new JsonSchema4
                {
                    Reference = referencedSchema.ActualSchema
                });
            }

            return referencingSchema;
        }

        /// <summary>Gets the converted property name.</summary>
        /// <param name="jsonProperty">The property.</param>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The property name.</returns>
        public virtual string GetPropertyName(Newtonsoft.Json.Serialization.JsonProperty jsonProperty, MemberInfo memberInfo)
        {
            try
            {
                var propertyName = memberInfo != null ? memberInfo
                    .DeclaringType.GetContextualPropertiesAndFields()
                    .First(p => p.Name == memberInfo.Name).GetName() : jsonProperty.PropertyName;

                var contractResolver = Settings.ActualContractResolver as DefaultContractResolver;
                return contractResolver != null
                    ? contractResolver.GetResolvedPropertyName(propertyName)
                    : propertyName;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not get JSON property name of property '" +
                    (memberInfo != null ? memberInfo.Name : "n/a") + "' and type '" +
                    (memberInfo?.DeclaringType != null ? memberInfo.DeclaringType.FullName : "n/a") + "'.", e);
            }
        }

        /// <summary>Generates the properties for the given type and schema.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeDescription">The type description.</param>
        /// <param name="schema">The properties</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The task.</returns>
        protected virtual async Task GenerateObjectAsync(Type type,
            JsonTypeDescription typeDescription, JsonSchema4 schema, JsonSchemaResolver schemaResolver)
        {
            schemaResolver.AddSchema(type, false, schema);

            var rootSchema = schema;
            var actualSchema = await GenerateInheritanceAsync(type, schema, schemaResolver).ConfigureAwait(false);
            if (actualSchema != null)
            {
                schema = actualSchema;
            }
            else
            {
                await GeneratePropertiesAsync(type, schema, schemaResolver).ConfigureAwait(false);
                await ApplyAdditionalPropertiesAsync(type, schema, schemaResolver).ConfigureAwait(false);
            }

            if (!schema.Type.HasFlag(JsonObjectType.Array))
            {
                typeDescription.ApplyType(schema);
            }

            schema.Description = await type.ToCachedType().GetDescriptionAsync().ConfigureAwait(false);

            if (Settings.GetActualGenerateAbstractSchema(type))
            {
                schema.IsAbstract = type.GetTypeInfo().IsAbstract;
            }

            GenerateInheritanceDiscriminator(type, rootSchema, schema);
            await GenerateKnownTypesAsync(type, schemaResolver).ConfigureAwait(false);

            if (Settings.GenerateXmlObjects)
            {
                schema.GenerateXmlObjectForType(type);
            }
        }

        private async Task ApplyAdditionalPropertiesAsync<TSchemaType>(Type type, TSchemaType schema, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var extensionDataProperty = type.GetContextualRuntimeProperties()
                .FirstOrDefault(p => p.GetContextAttribute<JsonExtensionDataAttribute>() != null);

            if (extensionDataProperty != null)
            {
                var genericTypeArguments = extensionDataProperty.GenericArguments;
                var extensionDataPropertyType = genericTypeArguments.Length == 2 ? genericTypeArguments[1] : typeof(object).ToContextualType();

                schema.AdditionalPropertiesSchema = await GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                    extensionDataPropertyType, schemaResolver).ConfigureAwait(false);
            }
            else
            {
                schema.AllowAdditionalProperties = false;
            }
        }

        private async Task ApplySchemaProcessorsAsync(ContextualType contextualType, JsonSchema4 schema, JsonSchemaResolver schemaResolver)
        {
            var context = new SchemaProcessorContext(contextualType.OriginalType, schema, schemaResolver, this, Settings);
            foreach (var processor in Settings.SchemaProcessors)
            {
                await processor.ProcessAsync(context).ConfigureAwait(false);
            }

            var operationProcessorAttribute = contextualType
                .TypeAttributes
                .Where(a => a.GetType().IsAssignableToTypeName(nameof(JsonSchemaProcessorAttribute), TypeNameStyle.Name));

            foreach (dynamic attribute in operationProcessorAttribute)
            {
                var processor = Activator.CreateInstance(attribute.Type, attribute.Parameters);
                await processor.ProcessAsync(context).ConfigureAwait(false);
            }
        }

        private void ApplyExtensionDataAttributes<TSchemaType>(ContextualType contextualType, TSchemaType schema)
            where TSchemaType : JsonSchema4, new()
        {
            // class
            var extensionDataAttributes = contextualType.GetAttributes<JsonSchemaExtensionDataAttribute>().ToArray();
            if (extensionDataAttributes.Any())
            {
                schema.ExtensionData = extensionDataAttributes.ToDictionary(a => a.Key, a => a.Value);
            }
            else
            {
                // property or parameter
                extensionDataAttributes = contextualType.GetAttributes<JsonSchemaExtensionDataAttribute>().ToArray();
                if (extensionDataAttributes.Any())
                {
                    schema.ExtensionData = extensionDataAttributes.ToDictionary(a => a.Key, a => a.Value);
                }
            }
        }

        private async Task<bool> TryHandleSpecialTypesAsync<TSchemaType>(ContextualType contextualType, TSchemaType schema, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var typeMapper = Settings.TypeMappers.FirstOrDefault(m => m.MappedType == contextualType.OriginalType);
            if (typeMapper == null && contextualType.OriginalType.GetTypeInfo().IsGenericType)
            {
                var genericType = contextualType.OriginalType.GetGenericTypeDefinition();
                typeMapper = Settings.TypeMappers.FirstOrDefault(m => m.MappedType == genericType);
            }

            if (typeMapper != null)
            {
                var context = new TypeMapperContext(contextualType.OriginalType, this, schemaResolver, contextualType.ContextAttributes);
                await typeMapper.GenerateSchemaAsync(schema, context).ConfigureAwait(false);
                return true;
            }

            if (contextualType.OriginalType.IsAssignableToTypeName(nameof(JArray), TypeNameStyle.Name) == false &&
                (contextualType.OriginalType.IsAssignableToTypeName(nameof(JToken), TypeNameStyle.Name) == true ||
                 contextualType.OriginalType == typeof(object)))
            {
                return true;
            }

            return false;
        }

        private async Task GenerateArray<TSchemaType>(
            TSchemaType schema, ContextualType contextualType, JsonTypeDescription typeDescription, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
#pragma warning disable 1998

            typeDescription.ApplyType(schema);

            var jsonSchemaAttribute = contextualType.GetTypeAttribute<JsonSchemaAttribute>();
            var itemType = jsonSchemaAttribute?.ArrayItem ?? contextualType.OriginalType.GetEnumerableItemType();
            if (itemType != null)
            {
                var contextualItemType = itemType.ToContextualType();
                var itemIsNullable = contextualType.GetContextAttribute<ItemsCanBeNullAttribute>() != null ||
                                     contextualItemType.Nullability == Nullability.Nullable;

                schema.Item = await GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                    contextualItemType, itemIsNullable, schemaResolver, async (itemSchema, typeSchema) =>
                    {
                        if (Settings.GenerateXmlObjects)
                        {
                            itemSchema.GenerateXmlObjectForItemType(contextualItemType);
                        }
                    }).ConfigureAwait(false);

                if (Settings.GenerateXmlObjects)
                {
                    schema.GenerateXmlObjectForArrayType();
                }
            }
            else
            {
                schema.Item = JsonSchema4.CreateAnySchema();
            }

#pragma warning restore 1998
        }

        private async Task GenerateEnum<TSchemaType>(
            TSchemaType schema, ContextualType contextualType, JsonTypeDescription typeDescription, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var type = contextualType.Type;

            var isIntegerEnumeration = typeDescription.Type == JsonObjectType.Integer;
            if (schemaResolver.HasSchema(type, isIntegerEnumeration))
            {
                schema.Reference = schemaResolver.GetSchema(type, isIntegerEnumeration);
            }
            else if (schema.GetType() == typeof(JsonSchema4))
            {
                typeDescription.ApplyType(schema);
                schema.Description = await type.GetXmlSummaryAsync().ConfigureAwait(false);

                LoadEnumerations(type, schema, typeDescription);

                schemaResolver.AddSchema(type, isIntegerEnumeration, schema);
            }
            else
            {
                schema.Reference = await GenerateAsync(contextualType, schemaResolver).ConfigureAwait(false);
            }
        }

        /// <exception cref="InvalidOperationException">Could not find value type of dictionary type.</exception>
        private async Task GenerateDictionaryAsync<TSchemaType>(TSchemaType schema, ContextualType contextualType, JsonSchemaResolver schemaResolver)
            where TSchemaType : JsonSchema4, new()
        {
            var genericTypeArguments = contextualType.GenericArguments;

            var keyType = genericTypeArguments.Length == 2 ? genericTypeArguments[0] : typeof(string).ToContextualType();
            if (keyType.OriginalType.GetTypeInfo().IsEnum)
            {
                schema.DictionaryKey = await GenerateWithReferenceAsync<JsonSchema4>(
                    keyType, schemaResolver).ConfigureAwait(false);
            }

            var valueType = genericTypeArguments.Length == 2 ? genericTypeArguments[1] : typeof(object).ToContextualType();
            if (valueType.OriginalType == typeof(object))
            {
                schema.AdditionalPropertiesSchema = JsonSchema4.CreateAnySchema();
            }
            else
            {
                var valueIsNullable = valueType.GetContextAttribute<ItemsCanBeNullAttribute>() != null ||
                    valueType.OriginalType.Name == "Nullable`1";

                schema.AdditionalPropertiesSchema = await GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                    valueType, valueIsNullable, schemaResolver/*, async (s, r) =>
                    {
                        // TODO: Generate xml for key
                        if (Settings.GenerateXmlObjects)
                        {
                            s.GenerateXmlObjectForItemType(keyType);
                        }
                    }*/).ConfigureAwait(false);
            }

            schema.AllowAdditionalProperties = true;
        }

        private async Task GeneratePropertiesAsync(Type type, JsonSchema4 schema, JsonSchemaResolver schemaResolver)
        {
#if !LEGACY
            var members = type.GetTypeInfo()
                .DeclaredFields
                .Where(f => f.IsPublic && !f.IsStatic)
                .OfType<MemberInfo>()
                .Concat(
                    type.GetTypeInfo().DeclaredProperties
                    .Where(p => (p.GetMethod?.IsPublic == true && p.GetMethod?.IsStatic == false) ||
                                (p.SetMethod?.IsPublic == true && p.SetMethod?.IsStatic == false))
                )
                .ToList();
#else
            var members = type.GetTypeInfo()
                .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.IsPublic && !f.IsStatic)
                .OfType<MemberInfo>()
                .Concat(
                    type.GetTypeInfo()
                    .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => (p.GetGetMethod()?.IsPublic == true && p.GetGetMethod()?.IsStatic == false) ||
                                (p.GetSetMethod()?.IsPublic == true && p.GetSetMethod()?.IsStatic == false))
                )
                .ToList();
#endif

            var contextualMembers = members.Select(m => m.ToContextualMember());
            var contract = Settings.ResolveContract(type);

            var allowedProperties = GetTypeProperties(type);
            var objectContract = contract as JsonObjectContract;
            if (objectContract != null && allowedProperties == null)
            {
                foreach (var jsonProperty in objectContract.Properties.Where(p => p.DeclaringType == type))
                {
                    bool shouldSerialize;
                    try
                    {
                        shouldSerialize = jsonProperty.ShouldSerialize?.Invoke(null) != false;
                    }
                    catch
                    {
                        shouldSerialize = true;
                    }

                    if (shouldSerialize)
                    {
                        var memberInfo = contextualMembers.FirstOrDefault(p => p.Name == jsonProperty.UnderlyingName);
                        var propertyInfo = memberInfo as ContextualPropertyInfo;

                        if (Settings.GenerateAbstractProperties || propertyInfo == null || propertyInfo.PropertyInfo.DeclaringType.GetTypeInfo().IsInterface ||
#if !LEGACY
                            (propertyInfo.PropertyInfo.GetMethod?.IsAbstract != true && propertyInfo.PropertyInfo.SetMethod?.IsAbstract != true))
#else
                            (propertyInfo.PropertyInfo.GetGetMethod()?.IsAbstract != true && propertyInfo.PropertyInfo.GetSetMethod()?.IsAbstract != true))
#endif
                        {
                            await LoadPropertyOrFieldAsync(jsonProperty, memberInfo, type, schema, schemaResolver).ConfigureAwait(false);
                        }
                    }
                }
            }
            else
            {
                // TODO: Remove this hacky code (used to support serialization of exceptions and restore the old behavior [pre 9.x])
                foreach (var memberInfo in contextualMembers.Where(m => allowedProperties == null || allowedProperties.Contains(m.Name)))
                {
                    var attribute = memberInfo.GetContextAttribute<JsonPropertyAttribute>();
                    var memberType = (memberInfo as ContextualPropertyInfo)?.PropertyInfo.PropertyType ??
                                     (memberInfo as ContextualFieldInfo)?.FieldInfo.FieldType;

                    var property = new Newtonsoft.Json.Serialization.JsonProperty
                    {
                        AttributeProvider = new ReflectionAttributeProvider(memberInfo),
                        PropertyType = memberType,
                        Ignored = IsPropertyIgnored(memberInfo, type)
                    };

                    if (attribute != null)
                    {
                        property.PropertyName = attribute.PropertyName ?? memberInfo.Name;
                        property.Required = attribute.Required;
                        property.DefaultValueHandling = attribute.DefaultValueHandling;
                        property.TypeNameHandling = attribute.TypeNameHandling;
                        property.NullValueHandling = attribute.NullValueHandling;
                        property.TypeNameHandling = attribute.TypeNameHandling;
                    }
                    else
                    {
                        property.PropertyName = memberInfo.Name;
                    }

                    await LoadPropertyOrFieldAsync(property, memberInfo, type, schema, schemaResolver).ConfigureAwait(false);
                }
            }
        }

        /// <summary>Gets the properties of the given type or null to take all properties.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The property names or null for all.</returns>
        protected virtual string[] GetTypeProperties(Type type)
        {
            if (type == typeof(Exception))
            {
                return new[] { "InnerException", "Message", "Source", "StackTrace" };
            }

            return null;
        }

        private async Task GenerateKnownTypesAsync(Type type, JsonSchemaResolver schemaResolver)
        {
            var attributes = type.GetTypeInfo()
                .GetCustomAttributes(Settings.GetActualFlattenInheritanceHierarchy(type));

            if (Settings.GenerateKnownTypes)
            {
                var knownTypeAttributes = attributes
                   // Known types of inherited classes will be generated later (in GenerateInheritanceAsync)
                   .Where(a => a.GetType().IsAssignableToTypeName("KnownTypeAttribute", TypeNameStyle.Name))
                   .OfType<Attribute>();

                foreach (dynamic attribute in knownTypeAttributes)
                {
                    if (attribute.Type != null)
                        await AddKnownTypeAsync(attribute.Type, schemaResolver).ConfigureAwait(false);
                    else if (attribute.MethodName != null)
                    {
                        var methodInfo = type.GetRuntimeMethod((string)attribute.MethodName, new Type[0]);
                        if (methodInfo != null)
                        {
                            var knownTypes = methodInfo.Invoke(null, null) as IEnumerable<Type>;
                            if (knownTypes != null)
                            {
                                foreach (var knownType in knownTypes)
                                    await AddKnownTypeAsync(knownType, schemaResolver).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                        throw new ArgumentException($"A KnownType attribute on {type.FullName} does not specify a type or a method name.", nameof(type));
                }
            }

            foreach (var jsonConverterAttribute in attributes
                .Where(a => a.GetType().IsAssignableToTypeName("JsonInheritanceAttribute", TypeNameStyle.Name)))
            {
                var knownType = ObjectExtensions.TryGetPropertyValue<Type>(
                    jsonConverterAttribute, "Type", null);

                if (knownType != null)
                {
                    await AddKnownTypeAsync(knownType, schemaResolver).ConfigureAwait(false);
                }
            }
        }

        private async Task AddKnownTypeAsync(Type type, JsonSchemaResolver schemaResolver)
        {
            var typeDescription = Settings.ReflectionService.GetDescription(type.ToContextualType(), Settings);
            var isIntegerEnum = typeDescription.Type == JsonObjectType.Integer;

            if (!schemaResolver.HasSchema(type, isIntegerEnum))
            {
                await GenerateAsync(type, schemaResolver).ConfigureAwait(false);
            }
        }

        private async Task<JsonSchema4> GenerateInheritanceAsync(Type type, JsonSchema4 schema, JsonSchemaResolver schemaResolver)
        {
            var baseType = type.GetTypeInfo().BaseType;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(ValueType))
            {
                if (baseType.GetTypeInfo().GetCustomAttributes(false).TryGetAssignableToTypeName("JsonSchemaIgnoreAttribute", TypeNameStyle.Name) == null &&
                    baseType.GetTypeInfo().GetCustomAttributes(false).TryGetAssignableToTypeName("SwaggerIgnoreAttribute", TypeNameStyle.Name) == null &&
                    Settings.ExcludedTypeNames?.Contains(baseType.FullName) != true)
                {
                    if (Settings.GetActualFlattenInheritanceHierarchy(type))
                    {
                        var typeDescription = Settings.ReflectionService.GetDescription(baseType.ToContextualType(), Settings);
                        if (!typeDescription.IsDictionary && !type.IsArray)
                        {
                            await GeneratePropertiesAsync(baseType, schema, schemaResolver).ConfigureAwait(false);
                            var actualSchema = await GenerateInheritanceAsync(baseType, schema, schemaResolver).ConfigureAwait(false);

                            GenerateInheritanceDiscriminator(baseType, schema, actualSchema ?? schema);
                        }
                    }
                    else
                    {
                        var actualSchema = new JsonSchema4();

                        await GeneratePropertiesAsync(type, actualSchema, schemaResolver).ConfigureAwait(false);
                        await ApplyAdditionalPropertiesAsync(type, actualSchema, schemaResolver).ConfigureAwait(false);

                        var baseTypeInfo = Settings.ReflectionService.GetDescription(baseType.ToContextualType(), Settings);
                        var requiresSchemaReference = baseTypeInfo.RequiresSchemaReference(Settings.TypeMappers);

                        if (actualSchema.Properties.Any() || requiresSchemaReference)
                        {
                            // Use allOf inheritance only if the schema is an object with properties 
                            // (not empty class which just inherits from array or dictionary)

                            var baseSchema = await GenerateAsync(baseType, schemaResolver).ConfigureAwait(false);
                            if (requiresSchemaReference)
                            {
                                if (schemaResolver.RootObject != baseSchema.ActualSchema)
                                {
                                    schemaResolver.AppendSchema(baseSchema.ActualSchema, Settings.SchemaNameGenerator.Generate(baseType));
                                }

                                schema.AllOf.Add(new JsonSchema4
                                {
                                    Reference = baseSchema.ActualSchema
                                });
                            }
                            else
                            {
                                schema.AllOf.Add(baseSchema);
                            }

                            // First schema is the (referenced) base schema, second is the type schema itself
                            schema.AllOf.Add(actualSchema);
                            return actualSchema;
                        }
                        else
                        {
                            // Array and dictionary inheritance are not expressed with allOf but inline
                            await GenerateAsync(baseType.ToContextualType(), schema, schemaResolver).ConfigureAwait(false);
                            return schema;
                        }
                    }
                }
            }

            if (Settings.GetActualFlattenInheritanceHierarchy(type) && Settings.GenerateAbstractProperties)
            {
#if !LEGACY
                foreach (var i in type.GetTypeInfo().ImplementedInterfaces)
#else
                foreach (var i in type.GetTypeInfo().GetInterfaces())
#endif
                {
                    var typeDescription = Settings.ReflectionService.GetDescription(i.ToContextualType(), Settings);
                    if (!typeDescription.IsDictionary && !type.IsArray &&
                        !typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(i.GetTypeInfo()))
                    {
                        await GeneratePropertiesAsync(i, schema, schemaResolver).ConfigureAwait(false);
                        var actualSchema = await GenerateInheritanceAsync(i, schema, schemaResolver).ConfigureAwait(false);

                        GenerateInheritanceDiscriminator(i, schema, actualSchema ?? schema);
                    }
                }
            }

            return null;
        }

        private void GenerateInheritanceDiscriminator(Type type, JsonSchema4 schema, JsonSchema4 typeSchema)
        {
            if (!Settings.GetActualFlattenInheritanceHierarchy(type))
            {
                var discriminatorConverter = TryGetInheritanceDiscriminatorConverter(type);
                if (discriminatorConverter != null)
                {
                    var discriminatorName = TryGetInheritanceDiscriminatorName(discriminatorConverter);

                    // Existing property can be discriminator only if it has String type  
                    if (typeSchema.Properties.TryGetValue(discriminatorName, out JsonProperty existingProperty) &&
                        (existingProperty.Type & JsonObjectType.String) == 0)
                    {
                        throw new InvalidOperationException("The JSON discriminator property '" + discriminatorName + "' must be a string property on type '" + type.FullName + "' (it is recommended to not implement the discriminator property at all).");
                    }

                    var discriminator = new OpenApiDiscriminator
                    {
                        JsonInheritanceConverter = discriminatorConverter,
                        PropertyName = discriminatorName
                    };

                    typeSchema.DiscriminatorObject = discriminator;
                    typeSchema.Properties[discriminatorName] = new JsonProperty
                    {
                        Type = JsonObjectType.String,
                        IsRequired = true
                    };
                }
                else
                {
                    var baseDiscriminator = schema.ResponsibleDiscriminatorObject ?? schema.ActualTypeSchema.ResponsibleDiscriminatorObject;
                    baseDiscriminator?.AddMapping(type, schema);
                }
            }
        }

        private object TryGetInheritanceDiscriminatorConverter(Type type)
        {
            var typeAttributes = type.GetTypeInfo().GetCustomAttributes(false).OfType<Attribute>();

            dynamic jsonConverterAttribute = typeAttributes.TryGetAssignableToTypeName(nameof(JsonConverterAttribute), TypeNameStyle.Name);
            if (jsonConverterAttribute != null)
            {
                var converterType = (Type)jsonConverterAttribute.ConverterType;
                if (converterType.IsAssignableToTypeName(nameof(JsonInheritanceConverter), TypeNameStyle.Name))
                {
                    return jsonConverterAttribute.ConverterParameters != null && jsonConverterAttribute.ConverterParameters.Length > 0 ?
                        Activator.CreateInstance(jsonConverterAttribute.ConverterType, jsonConverterAttribute.ConverterParameters) :
                        Activator.CreateInstance(jsonConverterAttribute.ConverterType);
                }
            }

            return null;
        }

        private string TryGetInheritanceDiscriminatorName(dynamic jsonInheritanceConverter)
        {
            if (ObjectExtensions.HasProperty(jsonInheritanceConverter, nameof(JsonInheritanceConverter.DiscriminatorName)))
            {
                return jsonInheritanceConverter.DiscriminatorName;
            }

            return JsonInheritanceConverter.DefaultDiscriminatorName;
        }

        private void LoadEnumerations(Type type, JsonSchema4 schema, JsonTypeDescription typeDescription)
        {
            schema.Type = typeDescription.Type;
            schema.Enumeration.Clear();
            schema.EnumerationNames.Clear();
            schema.IsFlagEnumerable = type.ToCachedType().GetTypeAttribute<FlagsAttribute>() != null;

            var underlyingType = Enum.GetUnderlyingType(type);

            var converters = Settings.ActualSerializerSettings.Converters.ToList();
            if (!converters.OfType<StringEnumConverter>().Any())
                converters.Add(new StringEnumConverter());

            foreach (var enumName in Enum.GetNames(type))
            {
                if (typeDescription.Type == JsonObjectType.Integer)
                {
                    var value = Convert.ChangeType(Enum.Parse(type, enumName), underlyingType);
                    schema.Enumeration.Add(value);
                }
                else
                {
                    // EnumMember only checked if StringEnumConverter is used
                    var attributes = type.GetTypeInfo().GetDeclaredField(enumName).GetCustomAttributes();
                    dynamic enumMemberAttribute = attributes.TryGetAssignableToTypeName("System.Runtime.Serialization.EnumMemberAttribute");
                    if (enumMemberAttribute != null && !string.IsNullOrEmpty(enumMemberAttribute.Value))
                        schema.Enumeration.Add((string)enumMemberAttribute.Value);
                    else
                    {
                        var value = Enum.Parse(type, enumName);
                        var json = JsonConvert.SerializeObject(value, Formatting.None, converters.ToArray());
                        schema.Enumeration.Add(JsonConvert.DeserializeObject<string>(json));
                    }
                }

                schema.EnumerationNames.Add(enumName);
            }

            if (typeDescription.Type == JsonObjectType.Integer && Settings.GenerateEnumMappingDescription)
            {
                schema.Description = (schema.Description + "\n\n" +
                    string.Join("\n", schema.Enumeration.Select((e, i) => e + " = " + schema.EnumerationNames[i]))).Trim();
            }
        }

        private async Task LoadPropertyOrFieldAsync(Newtonsoft.Json.Serialization.JsonProperty jsonProperty, ContextualMemberInfo memberInfo, Type parentType, JsonSchema4 parentSchema, JsonSchemaResolver schemaResolver)
        {
            var propertyTypeDescription = Settings.ReflectionService.GetDescription(memberInfo, Settings);
            if (jsonProperty.Ignored == false && IsPropertyIgnoredBySettings(memberInfo) == false)
            {
                var propertyName = GetPropertyName(jsonProperty, memberInfo.MemberInfo);
                if (parentSchema.Properties.ContainsKey(propertyName))
                    throw new InvalidOperationException("The JSON property '" + propertyName + "' is defined multiple times on type '" + parentType.FullName + "'.");

                var requiredAttribute = memberInfo.ContextAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.RequiredAttribute");

                var hasJsonNetAttributeRequired = jsonProperty.Required == Required.Always || jsonProperty.Required == Required.AllowNull;
                var isDataContractMemberRequired = GetDataMemberAttribute(memberInfo, parentType)?.IsRequired == true;

                var hasRequiredAttribute = requiredAttribute != null;
                if (hasRequiredAttribute || isDataContractMemberRequired || hasJsonNetAttributeRequired)
                {
                    parentSchema.RequiredProperties.Add(propertyName);
                }

                var isNullable = propertyTypeDescription.IsNullable &&
                    hasRequiredAttribute == false &&
                    (bool)isDataContractMemberRequired == false &&
                    (jsonProperty.Required == Required.Default || jsonProperty.Required == Required.AllowNull);

                Func<JsonProperty, JsonSchema4, Task> TransformSchema = async (propertySchema, typeSchema) =>
                {
                    if (Settings.GenerateXmlObjects)
                    {
                        propertySchema.GenerateXmlObjectForProperty(memberInfo, propertyName);
                    }

                    if (hasRequiredAttribute &&
                        propertyTypeDescription.Type == JsonObjectType.String &&
                        requiredAttribute.TryGetPropertyValue("AllowEmptyStrings", false) == false)
                    {
                        propertySchema.MinLength = 1;
                    }

                    if (!isNullable && Settings.SchemaType == SchemaType.Swagger2)
                    {
                        if (!parentSchema.RequiredProperties.Contains(propertyName))
                            parentSchema.RequiredProperties.Add(propertyName);
                    }

                    dynamic readOnlyAttribute = memberInfo.ContextAttributes.TryGetAssignableToTypeName("System.ComponentModel.ReadOnlyAttribute");
                    if (readOnlyAttribute != null)
                    {
                        propertySchema.IsReadOnly = readOnlyAttribute.IsReadOnly;
                    }

                    if (propertySchema.Description == null)
                    {
                        propertySchema.Description = await memberInfo.GetDescriptionAsync().ConfigureAwait(false);
                    }

                    propertySchema.Default = ConvertDefaultValue(jsonProperty);

                    ApplyDataAnnotations(propertySchema, propertyTypeDescription, memberInfo.ContextAttributes);
                };

                var referencingProperty = await GenerateWithReferenceAndNullabilityAsync(
                    memberInfo, isNullable, schemaResolver, TransformSchema).ConfigureAwait(false);

                parentSchema.Properties.Add(propertyName, referencingProperty);
            }
        }

        private bool IsPropertyIgnored(ContextualMemberInfo property, Type parentType)
        {
            if (property.GetContextAttribute<JsonIgnoreAttribute>() != null)
            {
                return true;
            }

            if (property.GetContextAttribute<JsonPropertyAttribute>() == null &&
                HasDataContractAttribute(parentType) &&
                GetDataMemberAttribute(property, parentType) == null)
            {
                return true;
            }

            return IsPropertyIgnoredBySettings(property);
        }

        private bool IsPropertyIgnoredBySettings(ContextualMemberInfo property)
        {
            if (Settings.IgnoreObsoleteProperties && property.GetContextAttribute<ObsoleteAttribute>() != null)
            {
                return true;
            }

            return false;
        }

        private dynamic GetDataMemberAttribute(ContextualMemberInfo property, Type parentType)
        {
            if (!HasDataContractAttribute(parentType))
            {
                return null;
            }

            return property.ContextAttributes.TryGetAssignableToTypeName("DataMemberAttribute", TypeNameStyle.Name);
        }

        private bool HasDataContractAttribute(Type parentType)
        {
            return parentType.ToCachedType().TypeAttributes
                .TryGetAssignableToTypeName("DataContractAttribute", TypeNameStyle.Name) != null;
        }

        /// <summary>Applies the property annotations to the JSON property.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="typeDescription">The property type description.</param>
        /// <param name="parentAttributes">The attributes.</param>
        public virtual void ApplyDataAnnotations(JsonSchema4 schema, JsonTypeDescription typeDescription, IEnumerable<Attribute> parentAttributes)
        {
            // TODO: Refactor out

            dynamic displayAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.DisplayAttribute");
            if (displayAttribute != null && displayAttribute.Name != null)
            {
                schema.Title = displayAttribute.Name;
            }

            dynamic defaultValueAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DefaultValueAttribute");
            if (defaultValueAttribute != null)
            {
                if (typeDescription.IsEnum &&
                    typeDescription.Type.HasFlag(JsonObjectType.String))
                {
                    schema.Default = defaultValueAttribute.Value?.ToString();
                }
                else
                {
                    schema.Default = defaultValueAttribute.Value;
                }
            }

            dynamic regexAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.RegularExpressionAttribute");
            if (regexAttribute != null)
            {
                if (typeDescription.IsDictionary)
                {
                    schema.AdditionalPropertiesSchema.Pattern = regexAttribute.Pattern;
                }
                else
                {
                    schema.Pattern = regexAttribute.Pattern;
                }
            }

            if (typeDescription.Type == JsonObjectType.Number ||
                typeDescription.Type == JsonObjectType.Integer)
            {
                ApplyRangeAttribute(schema, parentAttributes);

                var multipleOfAttribute = parentAttributes.OfType<MultipleOfAttribute>().SingleOrDefault();
                if (multipleOfAttribute != null)
                {
                    schema.MultipleOf = multipleOfAttribute.MultipleOf;
                }
            }

            dynamic minLengthAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.MinLengthAttribute");
            if (minLengthAttribute != null && minLengthAttribute.Length != null)
            {
                if (typeDescription.Type == JsonObjectType.String)
                {
                    schema.MinLength = minLengthAttribute.Length;
                }
                else if (typeDescription.Type == JsonObjectType.Array)
                {
                    schema.MinItems = minLengthAttribute.Length;
                }
            }

            dynamic maxLengthAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.MaxLengthAttribute");
            if (maxLengthAttribute != null && maxLengthAttribute.Length != null)
            {
                if (typeDescription.Type == JsonObjectType.String)
                {
                    schema.MaxLength = maxLengthAttribute.Length;
                }
                else if (typeDescription.Type == JsonObjectType.Array)
                {
                    schema.MaxItems = maxLengthAttribute.Length;
                }
            }

            dynamic stringLengthAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.StringLengthAttribute");
            if (stringLengthAttribute != null)
            {
                if (typeDescription.Type == JsonObjectType.String)
                {
                    schema.MinLength = stringLengthAttribute.MinimumLength;
                    schema.MaxLength = stringLengthAttribute.MaximumLength;
                }
            }

            dynamic dataTypeAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.DataTypeAttribute");
            if (dataTypeAttribute != null)
            {
                var dataType = dataTypeAttribute.DataType.ToString();
                if (DataTypeFormats.ContainsKey(dataType))
                {
                    schema.Format = DataTypeFormats[dataType];
                }
            }
        }

        private void ApplyRangeAttribute(JsonSchema4 schema, IEnumerable<Attribute> parentAttributes)
        {
            dynamic rangeAttribute = parentAttributes.TryGetAssignableToTypeName("System.ComponentModel.DataAnnotations.RangeAttribute");
            if (rangeAttribute != null)
            {
                if (rangeAttribute.Minimum != null)
                {
                    if (rangeAttribute.OperandType == typeof(double))
                    {
                        var minimum = (double)Convert.ChangeType(rangeAttribute.Minimum, typeof(double));
                        if (minimum > double.MinValue)
                        {
                            schema.Minimum = (decimal)minimum;
                        }
                    }
                    else
                    {
                        var minimum = (decimal)Convert.ChangeType(rangeAttribute.Minimum, typeof(decimal));
                        if (minimum > decimal.MinValue)
                        {
                            schema.Minimum = minimum;
                        }
                    }
                }

                if (rangeAttribute.Maximum != null)
                {
                    if (rangeAttribute.OperandType == typeof(double))
                    {
                        var maximum = (double)Convert.ChangeType(rangeAttribute.Maximum, typeof(double));
                        if (maximum < double.MaxValue)
                        {
                            schema.Maximum = (decimal)maximum;
                        }
                    }
                    else
                    {
                        var maximum = (decimal)Convert.ChangeType(rangeAttribute.Maximum, typeof(decimal));
                        if (maximum < decimal.MaxValue)
                        {
                            schema.Maximum = maximum;
                        }
                    }
                }
            }
        }

        private object ConvertDefaultValue(Newtonsoft.Json.Serialization.JsonProperty property)
        {
            if (property.DefaultValue != null && property.DefaultValue.GetType().GetTypeInfo().IsEnum)
            {
                var hasStringEnumConverter = typeof(StringEnumConverter).GetTypeInfo().IsAssignableFrom(property.Converter?.GetType().GetTypeInfo());
                if (hasStringEnumConverter)
                {
                    return property.DefaultValue.ToString();
                }
                else
                {
                    return (int)property.DefaultValue;
                }
            }
            else
            {
                return property.DefaultValue;
            }
        }
    }
}
