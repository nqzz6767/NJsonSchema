﻿//-----------------------------------------------------------------------
// <copyright file="CSharpClassGenerator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Linq;

namespace NJsonSchema.CodeGeneration.CSharp
{
    /// <summary>The CSharp code generator. </summary>
    public class CSharpClassGenerator : GeneratorBase
    {
        private readonly JsonSchema4 _schema;
        private readonly CSharpTypeResolver _resolver;

        /// <summary>Initializes a new instance of the <see cref="CSharpClassGenerator"/> class.</summary>
        /// <param name="schema">The schema.</param>
        public CSharpClassGenerator(JsonSchema4 schema)
            : this(schema, new CSharpTypeResolver())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CSharpClassGenerator"/> class.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="resolver">The resolver.</param>
        public CSharpClassGenerator(JsonSchema4 schema, CSharpTypeResolver resolver)
        {
            _schema = schema;
            _resolver = resolver;
        }

        /// <summary>Gets or sets the namespace.</summary>
        public string Namespace { get; set; }

        /// <summary>Gets the language.</summary>
        protected override string Language
        {
            get { return "CSharp"; }
        }

        /// <summary>Generates the file.</summary>
        /// <returns>The file contents.</returns>
        public string GenerateFile()
        {
            var classes = GenerateClass() + "\n\n" + _resolver.GenerateClasses();

            var template = LoadTemplate("File");
            template.Add("namespace", Namespace);
            template.Add("classes", classes);
            return template.Render();
        }

        /// <summary>Generates the main class.</summary>
        /// <returns></returns>
        public string GenerateClass()
        {
            var properties = _schema.Properties.Values.Select(property => new
            {
                Name = property.Name,
                PropertyName = ConvertToUpperStart(property.Name),
                FieldName = ConvertToLowerStart(property.Name),
                Required = property.IsRequired ? "Required.Always" : "Required.Default",
                Type = _resolver.Resolve(property, property.IsRequired)
            });

            var template = LoadTemplate("Class");
            template.Add("namespace", Namespace);
            template.Add("class", _schema.TypeName);
            template.Add("properties", properties);
            return template.Render();
        }
    }
}
