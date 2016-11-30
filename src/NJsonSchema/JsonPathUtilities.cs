//-----------------------------------------------------------------------
// <copyright file="JsonPathUtilities.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Infrastructure;

namespace NJsonSchema
{
    /// <summary>Utilities to work with JSON paths.</summary>
    public static class JsonPathUtilities
    {
        private static readonly Lazy<CamelCasePropertyNamesContractResolver> CamelCaseResolverLazy = new Lazy<CamelCasePropertyNamesContractResolver>();

        /// <summary>Gets the name of the property for JSON serialization.</summary>
        /// <returns>The name.</returns>
        /// <exception cref="NotSupportedException">The PropertyNameHandling is not supported.</exception>
        public static string GetPropertyName(MemberInfo property, PropertyNameHandling propertyNameHandling)
        {
            var propertyName = ReflectionCache.GetPropertiesAndFields(property.DeclaringType)
                .First(p => p.MemberInfo.Name == property.Name).GetName();
            switch (propertyNameHandling)
            {
                case PropertyNameHandling.Default:
                    return propertyName;

                case PropertyNameHandling.CamelCase:
                    return CamelCaseResolverLazy.Value.GetResolvedPropertyName(propertyName);

                default:
                    throw new NotSupportedException($"The PropertyNameHandling '{propertyNameHandling}' is not supported.");
            }
        }

        /// <summary>Gets the JSON path of the given object.</summary>
        /// <param name="root">The root object.</param>
        /// <param name="searchedObject">The object to search.</param>
        /// <param name="schemaResolver">The schema resolver.</param>
        /// <returns>The path or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not find the JSON path of a child object.</exception>
        public static string GetJsonPath(object root, object searchedObject, JsonSchemaResolver schemaResolver = null)
        {
            var path = GetJsonPath(root, searchedObject, "#", new HashSet<object>());
            if (path == null)
            {
                var searchedSchema = searchedObject as JsonSchema4;
                if (schemaResolver != null && searchedSchema != null)
                {
                    schemaResolver.AppendSchema(searchedSchema, null);
                    return GetJsonPath(root, searchedObject, schemaResolver);
                }
                else
                    throw new InvalidOperationException("Could not find the JSON path of a child object.");

            }
            return path;
        }

        private static string GetJsonPath(object obj, object searchedObject, string basePath, HashSet<object> checkedObjects)
        {
            if (obj == null || obj is string || checkedObjects.Contains(obj))
                return null;

            if (obj == searchedObject)
                return basePath;

            checkedObjects.Add(obj);

            if (obj is IDictionary)
            {
                foreach (var key in ((IDictionary)obj).Keys)
                {
                    var path = GetJsonPath(((IDictionary)obj)[key], searchedObject, basePath + "/" + key, checkedObjects);
                    if (path != null)
                        return path;
                }
            }
            else if (obj is IEnumerable)
            {
                var i = 0;
                foreach (var item in (IEnumerable)obj)
                {
                    var path = GetJsonPath(item, searchedObject, basePath + "/" + i, checkedObjects);
                    if (path != null)
                        return path;
                    i++;
                }
            }
            else
            {
                foreach (var member in ReflectionCache.GetPropertiesAndFields(obj.GetType()).Where(p => p.CustomAttributes.JsonIgnoreAttribute == null))
                {
                    var value = member.GetValue(obj);
                    if (value != null)
                    {
                        var pathSegment = member.GetName();
                        var path = GetJsonPath(value, searchedObject, basePath + "/" + pathSegment, checkedObjects);
                        if (path != null)
                            return path;
                    }
                }
            }

            return null;
        }
    }
}