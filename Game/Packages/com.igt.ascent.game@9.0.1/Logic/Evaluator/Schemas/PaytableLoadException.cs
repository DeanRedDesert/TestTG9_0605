//-----------------------------------------------------------------------
// <copyright file = "PaytableLoadException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception which is thrown if a paytable cannot be loaded successfully.
    /// </summary>
    [Serializable]
    public class PaytableLoadException : Exception
    {
        private const string DefaultMessageFormat = "Could not load paytable file {0}.";
        private const string MessageFormatWithMessage = DefaultMessageFormat + "\nMessage: {1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoadException"/> class.
        /// </summary>
        /// <param name="paytableFile">The name of the paytable file.</param>
        public PaytableLoadException(string paytableFile)
            : base (string.Format(DefaultMessageFormat, paytableFile))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoadException"/> class with the given message.
        /// </summary>
        /// <param name="message">A message describing the cause of the exception.</param>
        /// <param name="paytableFile">The name of the file that could not be loaded.</param>
        public PaytableLoadException(string paytableFile, string message)
            : base(string.Format(MessageFormatWithMessage, paytableFile, message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoadException"/> class with the given exception.
        /// </summary>
        /// <param name="paytableFile">The name of the file that could not be loaded.</param>
        /// <param name="innerException">The exception which caused the paytable load to fail.</param>
        public PaytableLoadException(string paytableFile, Exception innerException)
            : base(string.Format(DefaultMessageFormat, paytableFile), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoadException"/> class with the given message
        /// and exception.
        /// </summary>
        /// <param name="message">A message describing the cause of the exception.</param>
        /// <param name="paytableFile">The name of the file that could not be loaded.</param>
        /// <param name="innerException">The exception which caused the paytable load to fail.</param>
        public PaytableLoadException(string paytableFile, string message, Exception innerException)
            : base(string.Format(MessageFormatWithMessage, paytableFile, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaytableLoadException"/> class with the data in the given
        /// <see cref="SerializationInfo"/> and <see cref="StreamingContext"/> objects.
        /// </summary>
        /// <param name="serializationInfo">A <see cref="SerializationInfo"/> object containing serialized data.</param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> object containing additional information about the serialization stream.
        /// </param>
        protected PaytableLoadException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
