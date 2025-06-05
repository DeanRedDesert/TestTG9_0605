//-----------------------------------------------------------------------
// <copyright file = "InvalidFeatureIdException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Thrown when the feature ID specified for a device is not valid.
    /// </summary>
    [Serializable]
    public class InvalidFeatureIdException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Feature ID is not valid: {0}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID which was not valid.</param>
        public InvalidFeatureIdException(string featureId)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, featureId))
        {
        }
    }
}
