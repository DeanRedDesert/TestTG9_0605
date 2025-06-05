//-----------------------------------------------------------------------
// <copyright file = "SchemaLoadException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception which is thrown if one of the schemas cannot be loaded for any reason.
    /// </summary>
    [Serializable]
    public class SchemaLoadException : Exception
    {
        private const string PaytableVersionFormat =
            "No schemas could be loaded from the embedded assembly resources.\nPaytable Version: {0}";
        private const string PaytableVersionWithMessageFormat =
            "No schemas could be loaded from the embedded assembly resources.\nPaytable Version: {0}\nMessage: {1}";
        private const string SchemaNameFormat =
            "A schema could not be loaded from the embedded assembly resources.\nPaytable Version: {0}\nSchema Name: {1}";
        private const string SchemaNameWithMessageFormat =
            "A schema could not be loaded from the embedded assembly resources.\nPaytable Version: {0}\nSchema Name: {1}\nMessage: {2}";

        /// <summary>
        /// Gets the paytable version that the schemas were being loaded for.
        /// </summary>
        public PaytableVersion PaytableVersion { get; private set; }

        /// <summary>
        /// Gets the name of the schema that could not be loaded.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoadException"/> class with the given error message.
        /// </summary>
        /// <param name="paytableVersion">The paytable version that the schemas belong to.</param>
        /// <param name="message">A message further explaining the error.</param>
        public SchemaLoadException(PaytableVersion paytableVersion, string message) 
            : base(string.Format(CultureInfo.InvariantCulture, PaytableVersionWithMessageFormat, paytableVersion, message))
        {
            PaytableVersion = paytableVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoadException"/> class with the given error message
        /// and inner exception.
        /// </summary>
        /// <param name="paytableVersion">The paytable version that the schemas belong to.</param>
        /// <param name="innerException">The exception that was caught while loading a schema.</param>
        public SchemaLoadException(PaytableVersion paytableVersion, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, PaytableVersionFormat, paytableVersion), innerException)
        {
            PaytableVersion = paytableVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoadException"/> class with the given message and
        /// schema name.
        /// </summary>
        /// <param name="paytableVersion">The paytable version that the schemas belong to.</param>
        /// <param name="schemaName">The name of the schema that could not be loaded.</param>
        /// <param name="message">A message further explaining the error.</param>
        public SchemaLoadException(PaytableVersion paytableVersion, string schemaName, string message) 
            : base(string.Format(CultureInfo.InvariantCulture, SchemaNameWithMessageFormat, paytableVersion, schemaName, message))
        {
            PaytableVersion = paytableVersion;
            SchemaName = schemaName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoadException"/> class with the given message,
        /// schema name, and inner exception.
        /// </summary>
        /// <param name="paytableVersion">The paytable version that the schemas belong to.</param>
        /// <param name="schemaName">The name of the schema that could not be loaded.</param>
        /// <param name="innerException">The exception that was caught while loading a schema.</param>
        public SchemaLoadException(PaytableVersion paytableVersion, string schemaName, Exception innerException)
            : base(
                string.Format(CultureInfo.InvariantCulture, SchemaNameFormat, paytableVersion, schemaName),
                innerException)
        {
            PaytableVersion = paytableVersion;
            SchemaName = schemaName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaLoadException"/> class from the given
        /// <see cref="SerializationInfo"/> and <see cref="StreamingContext"/>.
        /// </summary>
        /// <param name="serializationInfo">
        /// The <see cref="SerializationInfo"/> containing serialized object information.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> providing additional information about the serialized stream.
        /// </param>
        protected SchemaLoadException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
