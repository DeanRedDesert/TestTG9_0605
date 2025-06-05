//-----------------------------------------------------------------------
// <copyright file = "CategoryVersionMismatchException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;
    using CSI.Schemas.Internal;

    /// <summary>
    /// Exception which indicates that a category was not compatible with the received version.
    /// </summary>
    [Serializable]
    public class CategoryVersionMismatchException : Exception
    {
        /// <summary>
        /// The requested major version of the category.
        /// </summary>
        public ushort RequestedVersionMajor { private set; get; }

        /// <summary>
        /// The requested version minor of the category.
        /// </summary>
        public ushort RequestedVersionMinor { private set; get; }

        /// <summary>
        /// The received version major of the category.
        /// </summary>
        public ushort ReceivedVersionMajor { private set; get; }

        /// <summary>
        /// The received version minor of the category.
        /// </summary>
        public ushort ReceivedVersionMinor { private set; get; }

        /// <summary>
        /// The category that has the mismatch.
        /// </summary>
        public Category Category { private set; get; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat =
            "Requested version: {0}.{1} is not compatible with received version: {2}.{3}. Category: {4}";

        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="requestedMajor">The requested major version of the category.</param>
        /// <param name="requestedMinor">The requested minor version of the category.</param>
        /// <param name="receivedMajor">The received major version of the category.</param>
        /// <param name="receivedMinor">The received minor version of the category.</param>
        /// <param name="category">The category that has the mismatch.</param>
        public CategoryVersionMismatchException(ushort requestedMajor, ushort requestedMinor, ushort receivedMajor,
                                       ushort receivedMinor, Category category)
            : base(
                string.Format(CultureInfo.InvariantCulture, MessageFormat, requestedMajor, requestedMinor, receivedMajor,
                              receivedMinor, category))
        {
            RequestedVersionMajor = requestedMajor;
            RequestedVersionMinor = requestedMinor;
            ReceivedVersionMajor = receivedMajor;
            ReceivedVersionMinor = receivedMinor;
            Category = category;
        }
    }
}
