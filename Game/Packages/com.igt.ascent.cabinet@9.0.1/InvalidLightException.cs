//-----------------------------------------------------------------------
// <copyright file = "InvalidLightException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Thrown when a light cannot be found in a group.
    /// </summary>
    [Serializable]
    public class InvalidLightException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Light group: {0}/{1} does not contain light {2}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <param name="lightGroup">The group which was not valid for the device.</param>
        /// <param name="light">Light which was not valid.</param>
        public InvalidLightException(string featureId, byte lightGroup, uint light)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, featureId, lightGroup, light))
        {
        }
    }
}
