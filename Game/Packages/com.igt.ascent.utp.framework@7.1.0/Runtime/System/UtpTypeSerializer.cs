// -----------------------------------------------------------------------
// <copyright file = "UtpTypeSerializer.cs" company = "IGT">
//     Copyright (c) 2016-2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    /// <summary>
    /// Class to define the UtpTypeSerializer.
    /// </summary>
    public static class UtpTypeSerializer
    {
        /// <summary>
        /// Dictionary of custom types.
        /// </summary>
        private static Dictionary<string, Type> customTypes;

        /// <summary>
        /// Parses a ModuleCommand return definition and converts any custom types to a serialized type.
        /// </summary>
        /// <param name="returnDefinition">Return definition string.</param>
        /// <param name="inputType">Input type flag.</param>
        /// <returns>Custom type serialized definition.</returns>
        public static string GetTypeDefinition(string returnDefinition, bool inputType = false)
        {
            if(returnDefinition == null)
            {
                throw new ArgumentNullException("returnDefinition");
            }

            if(customTypes == null)
            {
                customTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

                //  Reflect to get the type reference for UtpController (older versions of UtpController don't use a namespace, newer versions do)
                var utpControllerType = Type.GetType("IGT.Game.Utp.Framework.UtpController, Assembly-CSharp") ?? Type.GetType("UtpController, Assembly-CSharp");

                //  Get types from the UtpController assembly if it was found, otherwise use the executing assembly
                var assemblyTypes = utpControllerType != null ? Assembly.GetAssembly(utpControllerType).GetTypes() : Assembly.GetExecutingAssembly().GetTypes();
                foreach(var t in assemblyTypes)
                {
                    customTypes[t.Name] = t;
                }
            }

            var types = new List<string>();

            var matchString = inputType
                ? @"(?<Type>[\w-\[\]]+)(?<Parameter>)"
                : @"(?<Type>[\w-\[\]]+) (?<Parameter>[\w-]+)";

            var returnTypeRegex = new Regex(matchString, RegexOptions.IgnoreCase);

            Match paramMatch = returnTypeRegex.Match(returnDefinition);

            if(paramMatch.Success)
            {
                while(paramMatch.Success)
                {
                    var paramType = paramMatch.Groups["Type"].Value;
                    var paramName = paramMatch.Groups["Parameter"].Value;

                    Type type;
                    types.Add(customTypes.TryGetValue(paramType.Replace("[]", ""), out type)
                        ? GetSerializedType(type, paramName, paramType.EndsWith("[]"))
                        : string.Format("{0} {1}", paramType, paramName));

                    paramMatch = paramMatch.NextMatch();
                }

                var typeDefinition = "";

                for(int i = 0; i < types.Count; i++)
                {
                    typeDefinition += types[i].Trim();
                    if(i < types.Count - 1)
                    {
                        typeDefinition += ", ";
                    }
                }

                return typeDefinition;
            }

            // If we fail to parse it, just return what the module creator specified
            return returnDefinition;
        }

        /// <summary>
        /// Gets a serialized type definition for use with UTP ModuleCommand's.
        /// </summary>
        /// <param name="type">The type to serialize.</param>
        /// <param name="paramName">The parameter name it will be stored in.</param>
        /// <param name="isArrayOverride">If set the type definition will be forced to be an array.</param>
        /// <returns>Serialized type definition.</returns>
        private static string GetSerializedType(Type type, string paramName, bool isArrayOverride = false)
        {
            var typeString = "";
            if(type == null)
            {
                typeString = "void";
            }
            else if(IsSimpleObject(type))
            {
                typeString = type.Name;
            }
            else
            {
                var arrayType = type.IsArray || isArrayOverride;
                if(type.IsArray)
                {
                    type = type.GetElementType();
                }
                if(type != null)
                {
                    typeString = string.Format("Object({0}){{{1}}}{2}",
                        type.Name.Replace("[]", ""),
                        SerializeType(type),
                        arrayType ? "[]" : "");
                }
            }

            return string.Format("{0} {1}", typeString, paramName);
        }

        /// <summary>
        /// Returns a string representation of the type.
        /// </summary>
        /// <param name="type">The type to serialize.</param>
        /// <param name="depth">How far deep into the object it is.</param>
        /// <param name="knownTypes">List of previously generated types.</param>
        /// <returns>The serialized type.</returns>
        private static string SerializeType(Type type, int depth = 0, List<string> knownTypes = null)
        {
            if(depth > 5)
            {
                return "void";
            }

            var objectProps = new Dictionary<string, string>();

            if(knownTypes == null)
            {
                knownTypes = new List<string>();
            }

            var baseType = type.Name.Replace("[]", "");

            if(!knownTypes.Contains(baseType))
            {
                knownTypes.Add(baseType);
            }
            else
            {
                return type.Name;
            }

            var props = type.GetProperties()
                    .Select(p => new { p.Name, Type = p.PropertyType })
                    .Concat(type.GetFields()
                        .Select(p => new { p.Name, Type = p.FieldType }));

            foreach (var prop in props)
            {
                var pType = Nullable.GetUnderlyingType(prop.Type) ?? prop.Type;
                string pTypeString;
                var isArray = pType.IsArray;
                var isList = pType.Name.StartsWith("List");
                var isEnum = pType.IsEnum;

                // Get base type
                if(isArray)
                {
                    pType = pType.GetElementType();
                }

                if(isList)
                {
                    if(pType != null) pType = pType.GetGenericArguments().First();
                }

                if(pType == null)
                {
                    return "Type serialization failed: property type was null";
                }

                bool isComplexObj = !IsSimpleObject(pType);

                if (isEnum)
                {
                    var enumVals = new Dictionary<string, string>();
                    foreach (var enumVal in Enum.GetValues(pType))
                    {
                        var underlyingValue = Convert.ChangeType(enumVal, Enum.GetUnderlyingType(enumVal.GetType()));
                        if(underlyingValue != null && !enumVals.ContainsKey(underlyingValue.ToString()))
                        {
                            enumVals.Add(underlyingValue.ToString(), enumVal.ToString());
                        }
                    }
                    pTypeString = DictToString(enumVals, "{1}={0}", ", ");
                }
                else
                    pTypeString = isComplexObj ? SerializeType(pType, ++depth, knownTypes) : pType.Name;

                string prepend = "";

                if(isList)
                {
                    prepend += "List(";
                }

                if(isEnum)
                {
                    prepend += "Enum(" + pType.Name + "){";
                }

                else if(isComplexObj)
                {
                    prepend += "Object(" + pType.Name + "){";
                }

                string append = "";

                if(isEnum || isComplexObj)
                {
                    append += "}";
                }

                if(isList)
                {
                    append += ")";
                }

                if(pType.Name == type.Name)
                {
                    objectProps.Add(prop.Name, (isList ? "List(" : "") + type.Name + (isList ? ")" : "") + (isArray ? "[]" : ""));
                }
                else
                {
                    objectProps.Add(prop.Name,
                        string.Format("{0}{1}{2}{3}",
                            prepend,            // prepend - Object{, Enum{, or List(
                            pTypeString,        // type
                            append,             // append - } or )
                            isArray ? "[]" : "" // array
                            ));
                }
            }

            return DictToString(objectProps, "{1} {0}", ", ");
        }

        /// <summary>
        /// Write types definition.
        /// </summary>
        private static readonly Type[] writeTypes =
        {
            typeof(string), typeof(DateTime), typeof(Enum), 
            typeof(decimal), typeof(Guid)
        };

        /// <summary>
        /// Determine if simple type.
        /// </summary>
        /// <param name="type">Type of object.</param>
        /// <returns>Type.</returns>
        public static bool IsSimpleObject(Type type)
        {
            return type.IsPrimitive || writeTypes.Contains(type);
        }

        /// <summary>
        /// Convert dictionary to string.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <typeparam name="TV">TV.</typeparam>
        /// <param name="items">Items.</param>
        /// <param name="format">Format.</param>
        /// <param name="separator">Separator.</param>
        /// <returns></returns>
        private static string DictToString<T, TV>(IEnumerable<KeyValuePair<T, TV>> items, string format, string separator)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            var itemString = new StringBuilder();

            var keyValuePairs = items as KeyValuePair<T, TV>[] ?? items.ToArray();

            for (int i = 0; i < keyValuePairs.Length; i++)
            {
                itemString.AppendFormat(format, keyValuePairs.ElementAt(i).Key, keyValuePairs.ElementAt(i).Value);
                if (i != keyValuePairs.Length - 1)
                    itemString.Append(separator);
            }

            return itemString.ToString();
        }
        
        /// <summary>
        /// Serializes an object to an XML string for communication through UTP
        /// </summary>
        /// <param name="toSerialize">The object to serialize</param>
        /// <returns>String representation of the object</returns>
        public static string SerializeObject(object toSerialize)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(toSerialize.GetType());

                using(var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    var result = Regex.Replace(textWriter.ToString(), @" xmlns:xs\w=\""[\w\:\/\.\-]+\""", "", RegexOptions.IgnoreCase);
                    return Regex.Replace(result, @"\<\?xml.+\?\>\s*", "", RegexOptions.IgnoreCase);
                }
            }
            catch(Exception)
            {
                return "Failed to serialize data";
            }
        }

        /// <summary>
        /// Deserializes an xml string to an object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="toDeserialize">XML string of the object</param>
        /// <returns>Deserialized object</returns>
        public static object DeserializeObject<T>(string toDeserialize)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var textReader = new StringReader(toDeserialize);
                return xmlSerializer.Deserialize(textReader);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Failed to deserialize xml to type '{0}':\r\n{1}", typeof(T), toDeserialize);
                return null;
            }
        }
    }
}