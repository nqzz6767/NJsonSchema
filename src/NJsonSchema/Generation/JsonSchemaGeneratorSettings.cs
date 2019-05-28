//-----------------------------------------------------------------------
// <copyright file="JsonSchemaGeneratorSettings.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Annotations;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema.Infrastructure;
using System.Linq;
using Namotion.Reflection;

#pragma warning disable CS0618 // Type or member is obsolete
namespace NJsonSchema.Generation
{
    /// <summary>The JSON Schema generator settings.</summary>
    public class JsonSchemaGeneratorSettings
    {
        private Dictionary<string, JsonContract> _cachedContracts = new Dictionary<string, JsonContract>();

        private EnumHandling _defaultEnumHandling;
        private PropertyNameHandling _defaultPropertyNameHandling;
        private JsonSerializerSettings _serializerSettings;
        private IContractResolver _contractResolver;

        /// <summary>Initializes a new instance of the <see cref="JsonSchemaGeneratorSettings"/> class.</summary>
        public JsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.Default;
            SchemaType = SchemaType.JsonSchema;
            GenerateAbstractSchemas = true;

            // Obsolete, use SerializerSettings instead
            DefaultEnumHandling = EnumHandling.Integer;
            DefaultPropertyNameHandling = PropertyNameHandling.Default;
            ContractResolver = null;

            TypeNameGenerator = new DefaultTypeNameGenerator();
            SchemaNameGenerator = new DefaultSchemaNameGenerator();
            ReflectionService = new DefaultReflectionService();

            ExcludedTypeNames = new string[0];
        }

        /// <summary>Gets or sets the default reference type null handling when no nullability information is available (if NotNullAttribute and CanBeNullAttribute are missing, default: Null).</summary>
        public ReferenceTypeNullHandling DefaultReferenceTypeNullHandling { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate abstract properties (i.e. interface and abstract properties. Properties may defined multiple times in a inheritance hierarchy, default: false).</summary>
        public bool GenerateAbstractProperties { get; set; }

        /// <summary>Gets or sets a value indicating whether to flatten the inheritance hierarchy instead of using allOf to describe inheritance (default: false).</summary>
        public bool FlattenInheritanceHierarchy { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate the x-abstract flag on schemas (default: true).</summary>
        public bool GenerateAbstractSchemas { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate schemas for types in <see cref="KnownTypeAttribute"/> attributes (default: true).</summary>
        public bool GenerateKnownTypes { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether to generate xmlObject representation for definitions (default: false).</summary>
        public bool GenerateXmlObjects { get; set; } = false;

        /// <summary>Gets or sets a value indicating whether to ignore properties with the <see cref="ObsoleteAttribute"/>.</summary>
        public bool IgnoreObsoleteProperties { get; set; }

        /// <summary>Gets or sets a value indicating whether to use $ref references even if additional properties are 
        /// defined on the object (otherwise allOf/oneOf with $ref is used, default: false).</summary>
        public bool AllowReferencesWithProperties { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate a description with number to enum name mappings (for integer enums only, default: false).</summary>
        public bool GenerateEnumMappingDescription { get; set; }

        /// <summary>Gets or sets the schema type to generate (default: JsonSchema).</summary>
        public SchemaType SchemaType { get; set; }

        /// <summary>Gets or sets the serializer settings.</summary>
        /// <remarks><see cref="DefaultPropertyNameHandling"/>, <see cref="DefaultEnumHandling"/> and <see cref="ContractResolver"/> will be ignored.</remarks>
        [JsonIgnore]
        public JsonSerializerSettings SerializerSettings
        {
            get => _serializerSettings; set
            {
                _serializerSettings = value;
                UpdateActualContractResolverAndSerializerSettings();
            }
        }

        /// <summary>Gets or sets the excluded type names (same as <see cref="JsonSchemaIgnoreAttribute"/>).</summary>
        public string[] ExcludedTypeNames { get; set; }

        /// <summary>Gets or sets the type name generator.</summary>
        [JsonIgnore]
        public ITypeNameGenerator TypeNameGenerator { get; set; }

        /// <summary>Gets or sets the schema name generator.</summary>
        [JsonIgnore]
        public ISchemaNameGenerator SchemaNameGenerator { get; set; }

        /// <summary>Gets or sets the reflection service.</summary>
        [JsonIgnore]
        public IReflectionService ReflectionService { get; set; }

        /// <summary>Gets or sets the type mappings.</summary>
        [JsonIgnore]
        public ICollection<ITypeMapper> TypeMappers { get; set; } = new Collection<ITypeMapper>();

        /// <summary>Gets or sets the schema processors.</summary>
        [JsonIgnore]
        public ICollection<ISchemaProcessor> SchemaProcessors { get; } = new Collection<ISchemaProcessor>();

        /// <summary>Gets or sets a value indicating whether to generate x-nullable properties (Swagger 2 only).</summary>
        public bool GenerateCustomNullableProperties { get; set; }

        /// <summary>Gets or sets the contract resolver.</summary>
        /// <remarks><see cref="DefaultPropertyNameHandling"/> will be ignored.</remarks>
        [JsonIgnore]
        [Obsolete("Use SerializerSettings directly instead. In NSwag.AspNetCore the property is set automatically.")]
        public IContractResolver ContractResolver
        {
            get => _contractResolver; set
            {
                _contractResolver = value;
                UpdateActualContractResolverAndSerializerSettings();
            }
        }

        /// <summary>Gets or sets the default property name handling (default: Default).</summary>
        [Obsolete("Use SerializerSettings directly instead. In NSwag.AspNetCore the property is set automatically.")]
        public PropertyNameHandling DefaultPropertyNameHandling
        {
            get => _defaultPropertyNameHandling; set
            {
                _defaultPropertyNameHandling = value;
                UpdateActualContractResolverAndSerializerSettings();
            }
        }

        /// <summary>Gets or sets the default enum handling (default: Integer).</summary>
        [Obsolete("Use SerializerSettings directly instead. In NSwag.AspNetCore the property is set automatically.")]
        public EnumHandling DefaultEnumHandling
        {
            get => _defaultEnumHandling; set
            {
                _defaultEnumHandling = value;
                UpdateActualSerializerSettings();
            }
        }

        /// <summary>Gets the contract resolver.</summary>
        /// <returns>The contract resolver.</returns>
        /// <exception cref="InvalidOperationException">A setting is misconfigured.</exception>
        [JsonIgnore]
        public IContractResolver ActualContractResolver { get; internal set; }

        /// <summary>Gets the serializer settings.</summary>
        /// <exception cref="InvalidOperationException">A setting is misconfigured.</exception>
        [JsonIgnore]
        public JsonSerializerSettings ActualSerializerSettings { get; internal set; }

        /// <summary>Gets the contract for the given type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The contract.</returns>
        public JsonContract ResolveContract(Type type)
        {
            var key = type.FullName;
            if (key == null)
            {
                return null;
            }

            if (!_cachedContracts.ContainsKey(key))
            {
                lock (_cachedContracts)
                {
                    if (!_cachedContracts.ContainsKey(key))
                    {
                        _cachedContracts[key] = !type.GetTypeInfo().IsGenericTypeDefinition ?
                            ActualContractResolver.ResolveContract(type) :
                            null;
                    }
                }
            }

            return _cachedContracts[key];
        }

        /// <summary>Gets the actual computed <see cref="GenerateAbstractSchemas"/> setting based on the global setting and the JsonSchemaAbstractAttribute attribute.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The result.</returns>
        public bool GetActualGenerateAbstractSchema(Type type)
        {
            var attribute = type.GetTypeInfo().GetCustomAttributes(false)
                .FirstAssignableToTypeNameOrDefault("JsonSchemaAbstractAttribute", TypeNameStyle.Name);

            return (GenerateAbstractSchemas && attribute == null) || attribute?.TryGetPropertyValue("IsAbstract", true) == true;
        }

        /// <summary>Gets the actual computed <see cref="FlattenInheritanceHierarchy"/> setting based on the global setting and the JsonSchemaFlattenAttribute attribute.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The result.</returns>
        public bool GetActualFlattenInheritanceHierarchy(Type type)
        {
            var attribute = type.GetTypeInfo().GetCustomAttributes(false)
                .FirstAssignableToTypeNameOrDefault("JsonSchemaFlattenAttribute", TypeNameStyle.Name);

            return (FlattenInheritanceHierarchy && attribute == null) || attribute?.TryGetPropertyValue("Flatten", true) == true;
        }

        private void UpdateActualContractResolverAndSerializerSettings()
        {
            _cachedContracts = new Dictionary<string, JsonContract>();

            if (SerializerSettings != null)
            {
                if (DefaultPropertyNameHandling != PropertyNameHandling.Default)
                {
                    throw new InvalidOperationException("The setting DefaultPropertyNameHandling cannot be used when ContractResolver or SerializerSettings is set.");
                }

                if (ContractResolver != null)
                {
                    throw new InvalidOperationException("The setting ContractResolver cannot be used when SerializerSettings is set.");
                }

                ActualContractResolver = SerializerSettings.ContractResolver;
            }
            else if (ContractResolver != null)
            {
                if (DefaultPropertyNameHandling != PropertyNameHandling.Default)
                {
                    throw new InvalidOperationException("The setting DefaultPropertyNameHandling cannot be used when ContractResolver or SerializerSettings is set.");
                }

                ActualContractResolver = ContractResolver;
            }
            else if (DefaultPropertyNameHandling == PropertyNameHandling.CamelCase)
            {
                ActualContractResolver = new CamelCasePropertyNamesContractResolver();
            }
            else if (DefaultPropertyNameHandling == PropertyNameHandling.SnakeCase)
            {
                ActualContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
            }
            else
            {
                ActualContractResolver = new DefaultContractResolver();
            }

            UpdateActualSerializerSettings();
        }

        private void UpdateActualSerializerSettings()
        {
            if (SerializerSettings != null)
            {
                if (DefaultPropertyNameHandling != PropertyNameHandling.Default)
                {
                    throw new InvalidOperationException("The setting DefaultPropertyNameHandling cannot be used when ContractResolver or SerializerSettings is set.");
                }

                if (ContractResolver != null)
                {
                    throw new InvalidOperationException("The setting ContractResolver cannot be used when SerializerSettings is set.");
                }

                if (DefaultEnumHandling != EnumHandling.Integer)
                {
                    throw new InvalidOperationException("The setting DefaultEnumHandling cannot be used when SerializerSettings is set.");
                }

                ActualSerializerSettings = SerializerSettings;
            }
            else
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = ActualContractResolver;

                if (DefaultEnumHandling == EnumHandling.String)
                {
                    settings.Converters.Add(new StringEnumConverter());
                }
                else if (DefaultEnumHandling == EnumHandling.CamelCaseString)
                {
                    settings.Converters.Add(new StringEnumConverter(true));
                }

                ActualSerializerSettings = settings;
            }
        }
    }
}