﻿//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NJsonSchema.Validation
{
    /// <summary>A validation error. </summary>
    public class ValidationError
    {
        /// <summary>Initializes a new instance of the <see cref="ValidationError"/> class. </summary>
        /// <param name="errorKind">The error kind. </param>
        /// <param name="propertyName">The property name. </param>
        /// <param name="propertyPath">The property path. </param>
        /// <param name="token">The token that failed to validate. </param>
        public ValidationError(ValidationErrorKind errorKind, string propertyName, string propertyPath, JToken token)
        {
            Kind = errorKind;
            Property = propertyName;
            Path = propertyPath != null ? "#/" + propertyPath : "#";

            var lineInfo = token as IJsonLineInfo;
            _hasLineInfo = lineInfo != null && lineInfo.HasLineInfo();
            if (HasLineInfo())
            {
                LineNumber = lineInfo.LineNumber;
                LinePosition = lineInfo.LinePosition;
            }
            else
            {
                LineNumber = 0;
                LinePosition = 0;
            }
        }

        /// <summary>Gets the error kind. </summary>
        public ValidationErrorKind Kind { get; private set; }

        /// <summary>Gets the property name. </summary>
        public string Property { get; private set; }

        /// <summary>Gets the property path. </summary>
        public string Path { get; private set; }

        private readonly bool _hasLineInfo;

        /// <summary>Indicates whether or not the error contains line information.</summary>
        public bool HasLineInfo()
        {
            return _hasLineInfo;
        }

        /// <summary>Gets the line number the validation failed on. </summary>
        public int LineNumber { get; private set; }

        /// <summary>Gets the line number the validation failed on. </summary>
        public int LinePosition { get; private set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Kind, Path);
        }
    }
}
