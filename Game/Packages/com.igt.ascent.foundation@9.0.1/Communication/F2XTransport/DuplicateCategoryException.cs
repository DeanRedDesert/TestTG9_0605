//-----------------------------------------------------------------------
// <copyright file = "DuplicateCategoryException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when an attempt to register an already registered category is made.
    /// </summary>
    public class DuplicateCategoryException : Exception
    {
        /// <summary>
        /// The type of the object containing the duplicate category.
        /// </summary>
        public Type CategoryType { private set; get; }

        /// <summary>
        /// The category enum name which was duplicated.
        /// </summary>
        public MessageCategory Category { private set; get; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "{0} Category: {1} ({2}), Type: {3}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="category">The message category which was duplicated.</param>
        /// <param name="categoryType">The type of the object which was being installed.</param>
        public DuplicateCategoryException(string message, Type categoryType, MessageCategory category)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, category, (int)category, categoryType))
        {
            CategoryType = categoryType;
            Category = category;
        }
    }
}