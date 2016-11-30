﻿//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

namespace NJsonSchema.Validation
{
    /// <summary>A validation error. </summary>
    public class ValidationError
    {
        /// <summary>Initializes a new instance of the <see cref="ValidationError"/> class. </summary>
        /// <param name="errorKind">The error kind. </param>
        /// <param name="propertyName">The property name. </param>
        /// <param name="propertyPath">The property path. </param>
        public ValidationError(ValidationErrorKind errorKind, string propertyName, string propertyPath)
        {
            Kind = errorKind;
            Property = propertyName;
            Path = propertyPath != null ? "#/" + propertyPath : "#";
        }

        /// <summary>Gets the error kind. </summary>
        public ValidationErrorKind Kind { get; private set; }

        /// <summary>Gets the property name. </summary>
        public string Property { get; private set; }

        /// <summary>Gets the property path. </summary>
        public string Path { get; private set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Kind, Path);
        }
    }
}
