﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 15.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace NJsonSchema.CodeGeneration.CSharp.Templates
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    internal partial class ClassTemplate : ClassTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            
            #line 2 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.HasDescription){
            
            #line default
            #line hidden
            this.Write("/// <summary>");
            
            #line 3 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ConversionUtilities.ConvertCSharpDocBreaks(Model.Description, 0)));
            
            #line default
            #line hidden
            this.Write("</summary>\r\n");
            
            #line 4 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            
            #line 5 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.HasDiscriminator){
            
            #line default
            #line hidden
            this.Write("[Newtonsoft.Json.JsonConverter(typeof(JsonInheritanceConverter), \"");
            
            #line 6 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.Discriminator));
            
            #line default
            #line hidden
            this.Write("\")]\r\n");
            
            #line 7 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  foreach (var derivedClass in Model.DerivedClasses){
            
            #line default
            #line hidden
            this.Write("[JsonInheritanceAttribute(\"");
            
            #line 8 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(derivedClass.Key));
            
            #line default
            #line hidden
            this.Write("\", typeof(");
            
            #line 8 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(derivedClass.Value));
            
            #line default
            #line hidden
            this.Write("))]\r\n");
            
            #line 9 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 10 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("[System.CodeDom.Compiler.GeneratedCode(\"NJsonSchema\", \"");
            
            #line 11 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(JsonSchema4.ToolchainVersion));
            
            #line default
            #line hidden
            this.Write("\")]\r\n");
            
            #line 12 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.TypeAccessModifier));
            
            #line default
            #line hidden
            this.Write(" partial class ");
            
            #line 12 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.ClassName));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 12 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.InheritanceCode));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n");
            
            #line 14 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.RenderInpc){
            
            #line default
            #line hidden
            
            #line 15 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
foreach(var property in Model.Properties){
            
            #line default
            #line hidden
            this.Write("    private ");
            
            #line 16 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.Type));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 16 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.FieldName));
            
            #line default
            #line hidden
            
            #line 16 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.HasDefaultValue){
            
            #line default
            #line hidden
            this.Write(" = ");
            
            #line 16 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.DefaultValue));
            
            #line default
            #line hidden
            
            #line 16 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write(";\r\n");
            
            #line 17 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 19 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            
            #line 20 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
foreach(var property in Model.Properties){
            
            #line default
            #line hidden
            
            #line 21 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.HasDescription){
            
            #line default
            #line hidden
            this.Write("    /// <summary>");
            
            #line 22 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ConversionUtilities.ConvertCSharpDocBreaks(property.Description, 1)));
            
            #line default
            #line hidden
            this.Write("</summary>\r\n");
            
            #line 23 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            this.Write("    [Newtonsoft.Json.JsonProperty(\"");
            
            #line 24 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.Name));
            
            #line default
            #line hidden
            this.Write("\", Required = ");
            
            #line 24 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.JsonPropertyRequiredCode));
            
            #line default
            #line hidden
            
            #line 24 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.IsStringEnumArray){
            
            #line default
            #line hidden
            this.Write(", ItemConverterType = typeof(Newtonsoft.Json.Converters.StringEnumConverter)");
            
            #line 24 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write(")]\r\n");
            
            #line 25 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.RenderRequiredAttribute){
            
            #line default
            #line hidden
            this.Write("    [System.ComponentModel.DataAnnotations.Required]\r\n");
            
            #line 27 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 28 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.RenderRangeAttribute){
            
            #line default
            #line hidden
            this.Write("    [System.ComponentModel.DataAnnotations.Range(");
            
            #line 29 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.RangeMinimumValue));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 29 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.RangeMaximumValue));
            
            #line default
            #line hidden
            this.Write(")]\r\n");
            
            #line 30 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 31 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.RenderStringLengthAttribute){
            
            #line default
            #line hidden
            this.Write("    [System.ComponentModel.DataAnnotations.StringLength(");
            
            #line 32 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.StringLengthMaximumValue));
            
            #line default
            #line hidden
            
            #line 32 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.StringLengthMinimumValue > 0){ 
            
            #line default
            #line hidden
            this.Write(", MinimumLength = ");
            
            #line 32 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.StringLengthMinimumValue));
            
            #line default
            #line hidden
            
            #line 32 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write(")]\r\n");
            
            #line 33 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 34 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.RenderRegularExpressionAttribute){
            
            #line default
            #line hidden
            this.Write("    [System.ComponentModel.DataAnnotations.RegularExpression(@\"");
            
            #line 35 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.RegularExpressionValue));
            
            #line default
            #line hidden
            this.Write("\")]\r\n");
            
            #line 36 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 37 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.IsStringEnum){
            
            #line default
            #line hidden
            this.Write("    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumCo" +
                    "nverter))]\r\n");
            
            #line 39 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            
            #line 40 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(property.IsDate){
            
            #line default
            #line hidden
            this.Write("    [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]\r\n");
            
            #line 42 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }
            
            #line default
            #line hidden
            this.Write("    public ");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.Type));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.PropertyName));
            
            #line default
            #line hidden
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  if(!Model.RenderInpc){
            
            #line default
            #line hidden
            this.Write(" { get; ");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.HasSetter){
            
            #line default
            #line hidden
            this.Write("set; ");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("}");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.HasDefaultValue){
            
            #line default
            #line hidden
            this.Write(" = ");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.DefaultValue));
            
            #line default
            #line hidden
            this.Write(";");
            
            #line 43 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 45 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
  }else{
            
            #line default
            #line hidden
            this.Write("\r\n    {\r\n        get { return ");
            
            #line 48 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.FieldName));
            
            #line default
            #line hidden
            this.Write("; }\r\n");
            
            #line 49 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(property.HasSetter){
            
            #line default
            #line hidden
            this.Write("        set \r\n        {\r\n            if (");
            
            #line 52 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.FieldName));
            
            #line default
            #line hidden
            this.Write(" != value)\r\n            {\r\n                ");
            
            #line 54 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(property.FieldName));
            
            #line default
            #line hidden
            this.Write(" = value; \r\n                RaisePropertyChanged();\r\n            }\r\n        }\r\n");
            
            #line 58 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("    }\r\n");
            
            #line 60 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 62 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            
            #line 63 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.HasAdditionalPropertiesType){
            
            #line default
            #line hidden
            this.Write("    private System.Collections.Generic.IDictionary<string, ");
            
            #line 64 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.AdditionalPropertiesType));
            
            #line default
            #line hidden
            this.Write("> _additionalProperties = new System.Collections.Generic.Dictionary<string, ");
            
            #line 64 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.AdditionalPropertiesType));
            
            #line default
            #line hidden
            this.Write(">();\r\n\r\n    [Newtonsoft.Json.JsonExtensionData]\r\n    public System.Collections.Ge" +
                    "neric.IDictionary<string, ");
            
            #line 67 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.AdditionalPropertiesType));
            
            #line default
            #line hidden
            this.Write("> AdditionalProperties\r\n    {\r\n        get { return _additionalProperties; }\r\n   " +
                    "     set { _additionalProperties = value; }\r\n    }\r\n\r\n");
            
            #line 73 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            
            #line 74 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.RenderInpc){
            
            #line default
            #line hidden
            this.Write("    public event System.ComponentModel.PropertyChangedEventHandler PropertyChange" +
                    "d;\r\n\r\n");
            
            #line 77 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("    public string ToJson() \r\n    {\r\n        return Newtonsoft.Json.JsonConvert.Se" +
                    "rializeObject(this");
            
            #line 80 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.JsonSerializerParameterCode));
            
            #line default
            #line hidden
            this.Write(");\r\n    }\r\n    \r\n    public static ");
            
            #line 83 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.ClassName));
            
            #line default
            #line hidden
            this.Write(" FromJson(string data)\r\n    {\r\n        return Newtonsoft.Json.JsonConvert.Deseria" +
                    "lizeObject<");
            
            #line 85 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.ClassName));
            
            #line default
            #line hidden
            this.Write(">(data");
            
            #line 85 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.JsonSerializerParameterCode));
            
            #line default
            #line hidden
            this.Write(");\r\n    }\r\n");
            
            #line 87 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
if(Model.RenderInpc){
            
            #line default
            #line hidden
            this.Write(@"
    protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) 
            handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
");
            
            #line 95 "C:\Data\Projects\NJsonSchema\src\NJsonSchema.CodeGeneration.CSharp\Templates\ClassTemplate.tt"
}
            
            #line default
            #line hidden
            this.Write("}");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    internal class ClassTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
