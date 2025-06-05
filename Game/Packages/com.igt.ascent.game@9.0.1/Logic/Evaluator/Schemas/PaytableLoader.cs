//-----------------------------------------------------------------------
// <copyright file = "PaytableLoader.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Logging;

    /// <summary>
    /// The paytable loader will load a paytable file to produce a <see cref="Paytable"/> object. If a
    /// file contains an older version of the paytable format, this class will attempt to migrate it to
    /// the currently supported format.
    /// </summary>
    public class PaytableLoader
    {
        /// <summary>
        /// Gets the paytable version that is used to create instances of the <see cref="Paytable"/> class.
        /// </summary>
        public const PaytableVersion TipVersion = PaytableVersion.Ver6;

        /// <summary>
        /// A cache of successfully validatated paytables and the version that they validated against. The key is the
        /// key that was used when loading the paytable, usually the path to the paytable file.
        /// </summary>
        private readonly IDictionary<string, PaytableVersion> validationResults =
            new Dictionary<string, PaytableVersion>();

        /// <summary>
        /// The <see cref="PaytableValidator"/> objects used to validate paytable files, ordered by ascending version.
        /// </summary>
        private readonly IList<PaytableValidator> validators = new List<PaytableValidator>();

        /// <summary>
        /// A dictionary of lists of validation errors and warnings, keyed by the <see cref="PaytableVersion"/>
        /// of the <see cref="PaytableValidator"/> that they were produced with.
        /// </summary>
        private readonly IDictionary<PaytableVersion, IList<PaytableErrorInfo>> validationEvents =
            new Dictionary<PaytableVersion, IList<PaytableErrorInfo>>();

        /// <summary>
        /// The <see cref="XmlSerializer"/> used to deserialize paytable documents into <see cref="Paytable"/> objects.
        /// </summary>
        private readonly XmlSerializer paytableSerializer = new XmlSerializer(typeof(Paytable));

        /// <summary>
        /// Gets an enumerable collection of all the <see cref="PaytableVersion"/> that had validation errors while
        /// loading the last paytable.
        /// </summary>
        public IEnumerable<PaytableVersion> VersionsWithErrors
        {
            get { return validationEvents.Where(kvp => kvp.Value.Any()).Select(kvp => kvp.Key); }
        }

        /// <summary>
        /// Gets the Singleton instance.
        /// </summary>
        public static PaytableLoader Instance { get; private set; }

        /// <summary>
        /// This static constructor initializes the singleton instance.
        /// </summary>
        static PaytableLoader()
        {
            Instance = new PaytableLoader();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoader"/> class.
        /// </summary>
        private PaytableLoader()
        {
            /* Take everything up to and including the tip version. This is a precaution, in case a new version is 
             * added without updating the tip version. */
            var supportedVersions =
                Enum.GetValues(typeof(PaytableVersion)).OfType<PaytableVersion>().TakeWhile(x => x <= TipVersion);

            foreach(var version in supportedVersions)
            {
                var validator = PaytableValidator.Create(version);
                validators.Add(validator);
                validationEvents.Add(validator.Version, new List<PaytableErrorInfo>());
            }
        }

        /// <summary>
        /// Gets the validation errors produced while loading the last paytable for the given 
        /// <see cref="PaytableVersion"/>.
        /// </summary>
        /// <param name="paytableVersion">
        /// The <see cref="PaytableVersion"/>, which indicates which schemas were used for validation, to get the 
        /// errors for.
        /// </param>
        /// <returns>
        /// A list of <see cref="ValidationEventArgs"/>, or <b>null</b> if the paytable was not validated against the 
        /// given version.
        /// </returns>
        public IList<PaytableErrorInfo> GetErrorsForVersion(PaytableVersion paytableVersion)
        {
            return validationEvents.ContainsKey(paytableVersion) ? validationEvents[paytableVersion] : null;
        }

        /// <summary>
        /// Loads a <see cref="Paytable"/> object from the given <see cref="XDocument"/>.
        /// </summary>
        /// <param name="paytableDocument">The <see cref="XDocument"/> to load from.</param>
        /// <param name="key">A unique key for the paytable document (can be the file path).</param>
        /// <returns>The loaded <see cref="Paytable"/> object.</returns>
        /// <exception cref="PaytableLoadException">
        /// Thrown if the paytable could not be validated against any of the supported versions.
        /// </exception>
        public Paytable Load(XDocument paytableDocument, string key)
        {
            var result = LoadInternal(paytableDocument, key);
            return result.LoadedPaytable;
        }

        /// <summary>
        /// Resets the validation cache, which clears all previous validation results.
        /// </summary>
        public void ResetValidationCache()
        {
            validationResults.Clear();
        }

        /// <summary>
        /// Gets the paytable version that the document validates against. Uses a cache to avoid validating the same
        /// document multiple times, which is why the key is needed.
        /// </summary>
        /// <param name="paytableDocument">The paytable to validate.</param>
        /// <param name="key">A unique key for the paytable document (can be the file path).</param>
        /// <returns>Either the valid version or <b>null</b> if no version validates the document.</returns>
        private PaytableVersion? GetValidVersion(XDocument paytableDocument, string key)
        {
            PaytableValidator result;

            if(validationResults.ContainsKey(key))
            {
                result =
                    validators.Reverse().First(validator => validator.Version == validationResults[key]);
            }
            else
            {
                result = validators.Reverse()
                    .FirstOrDefault(paytableValidator => Validate(paytableDocument, paytableValidator));

                if(result != null)
                {
                    validationResults[key] = result.Version;
                }
            }

            if(result != null)
            {
                return result.Version;
            }
            return null;
        }

        /// <summary>
        /// Loads the given paytable document and returns a struct with the loaded paytable and its version.
        /// </summary>
        /// <param name="paytableDocument">The <see cref="XDocument"/> to load.</param>
        /// <param name="key">A unique key for the paytable document (can be the file path).</param>
        /// <returns>A struct with the loaded paytable and its version.</returns>
        private PaytableLoadResult LoadInternal(XDocument paytableDocument, string key)
        {
            ClearValidationErrors();

            var fromVersion = GetValidVersion(paytableDocument, key);

            if(fromVersion == null)
            {
                var message = BuildErrorMessage();
                throw new PaytableLoadException(key, message);
            }

            if(fromVersion != TipVersion)
            {
                paytableDocument = Transform(paytableDocument, fromVersion.Value);
            }

            Paytable paytable;
            using(var reader = paytableDocument.CreateReader())
            {
                paytable = paytableSerializer.Deserialize(reader) as Paytable;
            }
            return new PaytableLoadResult
                {
                    LoadedPaytable = paytable,
                    LoadedVersion = fromVersion.Value
                };
        }

        /// <summary>
        /// Transforms the given paytable document from the indicated version to the tip version.
        /// </summary>
        /// <param name="paytableDocument">The paytable document to transform.</param>
        /// <param name="fromVersion">The paytable version to transform from.</param>
        /// <returns>An <see cref="XDocument"/> containing the transformed paytable.</returns>
        private static XDocument Transform(XDocument paytableDocument, PaytableVersion fromVersion)
        {
            var transformer = new PaytableTransformer(fromVersion, TipVersion);

            Log.WriteWarning(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Older paytable format detected. Transforming from {0} to {1}.",
                    transformer.FromVersion,
                    transformer.ToVersion));

            paytableDocument = transformer.Transform(paytableDocument);
            return paytableDocument;
        }

        /// <summary>
        /// Builds an error message that includes all of the validation events from the last load operation.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing a formatted error message with all of the validation events.
        /// </returns>
        private string BuildErrorMessage()
        {
            var errorStringBuilder = new StringBuilder();

            foreach(var errorInfo in GetErrorsForVersion(TipVersion))
            {
                // First report the error message and the version that had the problem.
                errorStringBuilder.AppendFormat(
                    "Paytable Load {0}: {1} Version: {2}",
                    errorInfo.ValidationEvent.Severity,
                    errorInfo.ValidationEvent.Message,
                    TipVersion);

                // We've specified that we want line info in the loaded XDocument, so this should be a formality.
                if(errorInfo.HasLineInfo)
                {
                    errorStringBuilder.AppendFormat(
                        " Line: {0} Column: {1}", errorInfo.LineNumber, errorInfo.LinePosition);
                }

                // Add a newline.
                errorStringBuilder.AppendLine();
            }
            return errorStringBuilder.ToString();
        }

        /// <summary>
        /// Clears all validation errors.
        /// </summary>
        private void ClearValidationErrors()
        {
            foreach(var list in validationEvents.Values)
            {
                list.Clear();
            }
        }

        /// <summary>
        /// Validates the given <see cref="XDocument"/> with the given <see cref="PaytableValidator"/> and indicates
        /// if it is valid or not.
        /// </summary>
        /// <param name="document">The <see cref="XDocument"/> to validate.</param>
        /// <param name="validator">The <see cref="PaytableValidator"/> to validate the document with.</param>
        /// <returns>
        /// A <see cref="bool"/> value which is <b>true</b> if the document is a valid paytable document according to
        /// the given validator.
        /// </returns>
        private bool Validate(XDocument document, PaytableValidator validator)
        {
            validator.Validate(
                document,
                (sender, args) =>
                validationEvents[validator.Version].Add(new PaytableErrorInfo(sender as IXmlLineInfo, args)));
            return !validationEvents[validator.Version].Any();
        }

        /// <summary>
        /// Describes an error that was found while loading the paytable.
        /// </summary>
        /// <remarks>
        /// This class is essentially a Decorator for a <see cref="ValidationEventArgs"/> object. While that object
        /// contains a place for line information, it is never provided when validating an <see cref="XDocument"/>
        /// that has already been loaded. Instead, it is available as the sender parameter in a <see cref="ValidationEventHandler"/>.
        /// The sender must be cast to an <see cref="IXmlLineInfo"/> object.
        /// </remarks>
        public class PaytableErrorInfo
        {
            /// <summary>
            /// Gets a <see cref="bool"/> that is <b>true</b> if this object has line information.
            /// </summary>
            public bool HasLineInfo { get; private set; }

            /// <summary>
            /// Gets the line number where the error occurred.
            /// </summary>
            public int LineNumber { get; private set; }

            /// <summary>
            /// Gets the line position where the error occurred.
            /// </summary>
            public int LinePosition { get; private set; }

            /// <summary>
            /// Gets the <see cref="ValidationEventArgs"/> describing the error.
            /// </summary>
            public ValidationEventArgs ValidationEvent { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PaytableErrorInfo"/> class with the given <see cref="IXmlLineInfo"/>
            /// and <see cref="ValidationEventArgs"/> objects.
            /// </summary>
            /// <param name="lineInfo">
            /// A <see cref="IXmlLineInfo"/> object which indicates if line info is present and what it is if so.
            /// </param>
            /// <param name="validationEvent">
            /// The <see cref="ValidationEventArgs"/> object that was received at the same time as the 
            /// <see cref="IXmlLineInfo"/> object.
            /// </param>
            public PaytableErrorInfo(IXmlLineInfo lineInfo, ValidationEventArgs validationEvent)
            {
                HasLineInfo = lineInfo != null && lineInfo.HasLineInfo();
                LineNumber = lineInfo != null ? lineInfo.LineNumber : 0;
                LinePosition = lineInfo != null ? lineInfo.LinePosition : 0;
                ValidationEvent = validationEvent;
            }
        }

        /// <summary>
        /// This struct holds a loaded paytable and its loaded version. It is primarily in place so that the metrics can track
        /// overall load times for separate paytable versions.
        /// </summary>
        private struct PaytableLoadResult
        {
            /// <summary>
            /// The paytable that was loaded.
            /// </summary>
            public Paytable LoadedPaytable;

            /// <summary>
            /// The version of the loaded paytable.
            /// </summary>
            public PaytableVersion LoadedVersion;
        }
    }
}
