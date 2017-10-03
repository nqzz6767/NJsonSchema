﻿//-----------------------------------------------------------------------
// <copyright file="ClassTemplateModel.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration.Models;

namespace NJsonSchema.CodeGeneration.CSharp.Models
{
    /// <summary>The CSharp class template model.</summary>
    public class ClassTemplateModel : ClassTemplateModelBase
    {
        private readonly CSharpTypeResolver _resolver;
        private readonly JsonSchema4 _schema;
        private readonly CSharpGeneratorSettings _settings;

        /// <summary>Initializes a new instance of the <see cref="ClassTemplateModel"/> class.</summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="rootObject">The root object.</param>
        public ClassTemplateModel(string typeName, CSharpGeneratorSettings settings, 
            CSharpTypeResolver resolver, JsonSchema4 schema, object rootObject)
            : base(resolver, schema, rootObject)
        {
            _resolver = resolver;
            _schema = schema;
            _settings = settings;

            Class = typeName;
            Properties = _schema.ActualProperties.Values
                .Where(p => !p.IsInheritanceDiscriminator)
                .Select(property => new PropertyModel(this, property, _resolver, _settings))
                .ToList();
        }

        /// <summary>Gets or sets the class name.</summary>
        public override string Class { get; }

        /// <summary>Gets the namespace.</summary>
        public string Namespace => _settings.Namespace;

        /// <summary>Gets a value indicating whether an additional properties type is available.</summary>
        public bool HasAdditionalPropertiesType => _schema.AdditionalPropertiesSchema != null;

        /// <summary>Gets the type of the additional properties.</summary>
        public string AdditionalPropertiesType => _resolver.Resolve(
            _schema.AdditionalPropertiesSchema,
            _schema.AdditionalPropertiesSchema.IsNullable(_settings.SchemaType),
            string.Empty);

        /// <summary>Gets the property models.</summary>
        public IEnumerable<PropertyModel> Properties { get; }

        /// <summary>Gets a value indicating whether the class has description.</summary>
        public bool HasDescription => !(_schema is JsonProperty) && !string.IsNullOrEmpty(_schema.Description);

        /// <summary>Gets the description.</summary>
        public string Description => _schema.Description;

        /// <summary>Gets a value indicating whether the class style is INPC.</summary>
        /// <value><c>true</c> if inpc; otherwise, <c>false</c>.</value>
        public bool Inpc => _settings.ClassStyle == CSharpClassStyle.Inpc;

        /// <summary>Gets a value indicating whether the class has discriminator property.</summary>
        public bool HasDiscriminator => !string.IsNullOrEmpty(_schema.Discriminator);

        /// <summary>Gets the discriminator property name.</summary>
        public string Discriminator => _schema.Discriminator;

        /// <summary>Gets a value indicating whether the class has a parent class.</summary>
        public bool HasInheritance => _schema.InheritedSchema != null;

        /// <summary>Gets the base class name.</summary>
        public string BaseClass => HasInheritance ? _resolver.Resolve(_schema.InheritedSchema, false, string.Empty) : null;

        /// <summary>Gets the JSON serializer parameter code.</summary>
        public string JsonSerializerParameterCode => CSharpJsonSerializerGenerator.GenerateJsonSerializerParameterCode(_settings, null);

        /// <summary>Gets the inheritance code.</summary>
        public string Inheritance
        {
            get
            {
                if (HasInheritance)
                    return ": " + BaseClass + (_settings.ClassStyle == CSharpClassStyle.Inpc ? ", System.ComponentModel.INotifyPropertyChanged" : "");
                else
                    return _settings.ClassStyle == CSharpClassStyle.Inpc ? ": System.ComponentModel.INotifyPropertyChanged" : "";
            }
        }
    }
}