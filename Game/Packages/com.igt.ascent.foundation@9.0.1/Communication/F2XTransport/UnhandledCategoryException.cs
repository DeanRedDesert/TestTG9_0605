//-----------------------------------------------------------------------
// <copyright file = "UnhandledCategoryException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when a message is received for a category without a handler.
    /// </summary>
    public class UnhandledCategoryException : Exception
    {
        /// <summary>
        /// The category enum name which was not handled.
        /// </summary>
        public MessageCategory Category { private set; get; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "{0} Category Name: {1}, ({2})";

        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="category">The category which was not handled.</param>
        public UnhandledCategoryException(string message, MessageCategory category)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, category, (int)category))
        {
            Category = category;
        }
    }
}