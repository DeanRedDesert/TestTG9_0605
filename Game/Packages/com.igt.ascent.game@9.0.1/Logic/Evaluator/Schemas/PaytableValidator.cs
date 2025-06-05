//-----------------------------------------------------------------------
// <copyright file = "PaytableValidator.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Ascent.Build.EmbeddedResources;
    using Logging;

    /// <summary>
    /// Provides validation of paytables for a particular version of the paytable schema.
    /// </summary>
    public class PaytableValidator
    {
        /// <summary>
        /// The regex format pattern, which must be formatted with a <see cref="PaytableVersion"/> to create the correct
        /// pattern for that version's XSD files.
        /// </summary>
        private const string XsdPatternFormat = @".*\.{0}\..+\.xsd$";

        private readonly XmlSchemaSet schemaSet;

        /// <summary>
        /// Gets the <see cref="PaytableVersion"/> for this validator.
        /// </summary>
        public PaytableVersion Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableValidator"/> with the given 
        /// <see cref="XmlSchemaSet"/>.
        /// </summary>
        /// <param name="version">The <see cref="PaytableVersion"/> for the provided schema set.</param>
        /// <param name="schemaSet">A set of XML schemas that will be used in conjunction to validate paytables.</param>
        private PaytableValidator(PaytableVersion version, XmlSchemaSet schemaSet)
        {
            Version = version;
            this.schemaSet = schemaSet;
            if(!schemaSet.IsCompiled)
            {
                schemaSet.Compile();
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PaytableValidator"/> class that can be used to validate
        /// documents with the given version.
        /// </summary>
        /// <param name="version">The <see cref="PaytableVersion"/> to use for validation.</param>
        /// <returns>A new <see cref="PaytableValidator"/> object with the schema set for the specified version.</returns>
        public static PaytableValidator Create(PaytableVersion version)
        {
            var schemaSet = LoadSchemasForVersion(version);
            return new PaytableValidator(version, schemaSet);
        }

        /// <summary>
        /// Validates the given paytable document and reports any validation errors or warnings through
        /// the given <see cref="ValidationEventHandler"/>.
        /// </summary>
        /// <param name="paytableDocument">The <see cref="XDocument"/> to validate.</param>
        /// <param name="validationEventHandler">
        /// The <see cref="ValidationEventHandler"/> that will be called if there are any validation errors
        /// or warnings.
        /// </param>
        public void Validate(XDocument paytableDocument, ValidationEventHandler validationEventHandler)
        {
            try
            {
                var readerSettings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = schemaSet
                };
                readerSettings.ValidationEventHandler += validationEventHandler;
                using(var reader = XmlReader.Create(paytableDocument.CreateReader(), readerSettings))
                {
                    while(reader.Read())
                    {
                    }
                }
            }
            catch(InvalidCastException exception)
            {
                // Mono is throwing an InvalidCastException when validating, but .NET does not.
                // TODO: Add support for decimal values to XmlAtomicType in Mono.
                Log.WriteWarning(exception.ToString());
            }
        }

        /// <summary>
        /// Loads the embedded schema files for the given version into a schema set.
        /// </summary>
        /// <param name="version">The <see cref="PaytableVersion"/> specifying which schemas to load.</param>
        /// <returns>A <see cref="XmlSchemaSet"/> containing the schemas for the specified paytable version.</returns>
        /// <exception cref="SchemaLoadException">
        /// Thrown if any schema for the given version could not be loaded for any reason.
        /// </exception>
        private static XmlSchemaSet LoadSchemasForVersion(PaytableVersion version)
        {
            var assembly = ResourcesAssembly.GetCurrent();
            var schemaSet = new XmlSchemaSet();
            var schemasToLoad = GetSchemaNamesForVersion(version, assembly);
            
            foreach(var schemaName in schemasToLoad)
            {
                try
                {
                    using(var stream = assembly.GetManifestResourceStream(schemaName))
                    {
                        if(stream == null)
                        {
                            throw new SchemaLoadException(version, schemaName, "The manifest resource stream was null.");
                        }

                        var schema = XmlSchema.Read(stream, null);
                        schemaSet.Add(schema);
                    }
                }
                catch(Exception exception)
                {
                    throw new SchemaLoadException(version, schemaName, exception);
                }
            }

            return schemaSet;
        }

        /// <summary>
        /// Gets the list of schema names that make up the given version.
        /// </summary>
        /// <param name="version">The paytable version to get the schemas for.</param>
        /// <param name="assembly">The assembly that contains the schemas.</param>
        /// <returns>An enumerable collection containing the full resource names of the schemas.</returns>
        /// <remarks>
        /// The schemas are partitioned into separate folders for versions (e.g. Ver1, Ver2, etc.) Ver1 contains all of
        /// the schemas for version 1, but Ver2 only contains the schemas that changed from version 1, and so on. The
        /// names of the schema files do not change between versions. This means that to obtain the set of schemas for
        /// version 2, you would take all of the schemas from the Ver1 folder, then take the schemas from Ver2, replacing
        /// any identically named schemas from the Ver1 set and adding any new schemas.
        /// 
        /// This method reduces the required footprint needed to store all versions. The primary limitation is that it
        /// is impossible to remove a schema using this method.
        /// </remarks>
        /// <exception cref="SchemaLoadException">
        /// Thrown if no schema files are contained in any of the required folders.
        /// </exception>
        private static IEnumerable<string> GetSchemaNamesForVersion(PaytableVersion version, Assembly assembly)
        {
            var schemasToLoad = new Dictionary<string, string>();

            foreach(var versionValue in Enum.GetValues(typeof(PaytableVersion)).OfType<PaytableVersion>())
            {
                var regexPattern = string.Format(CultureInfo.InvariantCulture, XsdPatternFormat, versionValue);
                var schemaNames = assembly.GetMatchingResourceNames(regexPattern).ToList();

                if(!schemaNames.Any())
                {
                    throw new SchemaLoadException(
                        versionValue,
                        "Could not find any embedded schema files.");
                }

                foreach(var schemaName in schemaNames)
                {
                    var key = GetFileName(schemaName);
                    schemasToLoad[key] = schemaName;
                }

                if(version == versionValue)
                {
                    break;
                }
            }
            return schemasToLoad.Values;
        }

        /// <summary>
        /// Gets the original file name from the given resource name.
        /// </summary>
        /// <param name="resourcePath">A '.'-separated resource path.</param>
        /// <returns>A string containing the file name portion of the resource path.</returns>
        private static string GetFileName(string resourcePath)
        {
            var parts = resourcePath.Split('.');
            return parts.Length < 2
                ? resourcePath
                : $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
        }
    }
}
