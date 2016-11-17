//-----------------------------------------------------------------------
// <copyright file="ClassTemplateModel.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration.Models;

namespace NJsonSchema.CodeGeneration.TypeScript.Models
{
    /// <summary>The TypeScript class template model.</summary>
    public class ClassTemplateModel : ClassTemplateModelBase
    {
        private readonly TypeScriptGeneratorSettings _settings;
        private readonly string _typeName;
        private readonly JsonSchema4 _schema;
        private readonly object _rootObject;
        private readonly TypeScriptTypeResolver _resolver;

        /// <summary>Initializes a new instance of the <see cref="ClassTemplateModel" /> class.</summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="discriminatorName">The name to compare the discriminator against.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="rootObject">The root object.</param>
        public ClassTemplateModel(string typeName, string discriminatorName, TypeScriptGeneratorSettings settings, TypeScriptTypeResolver resolver, JsonSchema4 schema, object rootObject)
        {
            _typeName = typeName;
            _settings = settings;
            _schema = schema;
            _rootObject = rootObject;
            _resolver = resolver;

            DiscriminatorName = discriminatorName;
        }

        /// <summary>Gets the class name (the postfix 'Base' is used when using an extension class).</summary>
        public string Class => _settings.ExtendedClasses?.Contains(_typeName) == true ? _typeName + "Base" : _typeName;

        /// <summary>Gets the actual class name (i.e. the derived class when using an extension class).</summary>
        public override string ActualClass => _typeName;

        /// <summary>Gets the derived class names.</summary>
        public List<string> DerivedClassNames => _schema.GetDerivedSchemas(_rootObject, _resolver)
            .Where(s => s.Value.Inherits(_schema))
            .Select(s => s.Key)
            .ToList();

        /// <summary>Gets the name for the discriminator check.</summary>
        public string DiscriminatorName { get; }

        /// <summary>Gets a value indicating whether the class has a discriminator property.</summary>
        public bool HasDiscriminator => !string.IsNullOrEmpty(_schema.Discriminator);
        
        /// <summary>Gets a value indicating whether the class or an inherited class has a discriminator property.</summary>
        public bool HasBaseDiscriminator => !string.IsNullOrEmpty(_schema.BaseDiscriminator);
       
        /// <summary>Gets the class discriminator property name (may be defined in a inherited class).</summary>
        public string BaseDiscriminator => _schema.BaseDiscriminator;

        /// <summary>Gets a value indicating whether the class has description.</summary>
        public bool HasDescription => !(_schema is JsonProperty) && !string.IsNullOrEmpty(_schema.Description);

        /// <summary>Gets the description.</summary>
        public string Description => ConversionUtilities.RemoveLineBreaks(_schema.Description);

        /// <summary>Gets a value indicating whether this class has a parent class.</summary>
        public bool HasInheritance => _schema.InheritedSchemas.Count >= 1;

        /// <summary>Gets the inheritance code.</summary>
        public string Inheritance => HasInheritance ? " extends " + BaseClass : string.Empty;

        /// <summary>Gets the base class name.</summary>
        public string BaseClass => HasInheritance ? _resolver.Resolve(_schema.InheritedSchemas.First(), true, string.Empty) : null;

        /// <summary>Gets the property models.</summary>
        public List<PropertyModel> Properties => _schema.ActualProperties.Values
            .Where(v => v.IsInheritanceDiscriminator == false)
            .Select(property => new PropertyModel(this, property, _typeName, _resolver, _settings)).ToList();
    }
}