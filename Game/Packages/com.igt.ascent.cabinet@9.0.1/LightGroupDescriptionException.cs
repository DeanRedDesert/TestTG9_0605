//-----------------------------------------------------------------------
// <copyright file = "LightGroupDescriptionException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that there was an error with a light group description.
    /// </summary>
    public class LightGroupDescriptionException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Error with group description: Type: {0} Message: {1}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureType">The type of the light device.</param>
        /// <param name="message">Message associated with the error.</param>
        public LightGroupDescriptionException(LightSubFeature featureType, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, featureType, message))
        {
        }

    }
}
